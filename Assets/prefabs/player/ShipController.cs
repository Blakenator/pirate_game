using Crest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : DestroyableBase
{
    public Transform[] cannonSlotsLeft;
    public Transform[] cannonSlotsRight;
    public GameObject cannonPrefab;
    public CannonController[] cannonsLeft;
    public CannonController[] cannonsRight;
    public Rigidbody player;
    public Collider[] playerColliders;
    public Health myHealth;
    public GameObject deathParticles;
    public GameObject damageParticles;
    public AudioClipGroup damageAudio;
    public BoatProbes boatProbes;

    public float fireRate = 2f;

    public float currentCannonLeft = 0f;
    public float currentCannonRight = 0f;
    public LinkedList<Health> enemies;
    Health closestEnemy = null;
    public bool playPlayerSounds = false;

    private float initialMass = 0;
    private float initialBuoyancy = 0;

    public bool HasEnemy()
    {
        return enemies.Count > 0;
    }

    public void InitializeShip(int numCannons)
    {
        int numCannonsToCreate = Mathf.Min(numCannons, cannonSlotsLeft.Length + cannonSlotsRight.Length);
        var numCannonsLeft = Mathf.FloorToInt(numCannonsToCreate / 2);
        var numCannonsRight = Mathf.CeilToInt(numCannonsToCreate / 2);
        cannonsLeft = new CannonController[numCannonsLeft];
        cannonsRight = new CannonController[numCannonsRight];
        currentCannonRight = 0.5f;
        var doNotCollide = new HashSet<Collider>(playerColliders);
        for (int i = 0; i < numCannonsRight; i++)
        {
            if (i < numCannonsLeft)
            {
                InitializeCannon(cannonSlotsLeft[i], i, cannonsLeft, doNotCollide);
            }
            InitializeCannon(cannonSlotsRight[i], i, cannonsRight, doNotCollide);
        }
        enemies = new LinkedList<Health>();
        myHealth.onDamage += OnDamage;
        myHealth.OnHealthDestroy += OnHealthDestroy;
        initialMass = player.mass;
        initialBuoyancy = boatProbes._forceMultiplier;
    }

    private void InitializeCannon(Transform slot, int i, CannonController[] cannons, HashSet<Collider> doNotCollide)
    {
        var cannon = Instantiate(cannonPrefab, slot.position, Quaternion.identity, slot);
        cannon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        cannons[i] = cannon.GetComponentInChildren<CannonController>();
        cannons[i].player = player;
        cannons[i].doNotCollide = doNotCollide;
        cannons[i].playPlayerSounds = playPlayerSounds;
    }

    public void FireSomeCannons(bool fireLeft)
    {
        float currentValue = fireLeft ? currentCannonLeft : currentCannonRight;
        int currentValueInt = Mathf.FloorToInt(currentValue);
        float nextValue = currentValue + Time.deltaTime * fireRate;
        int nextValueInt = Mathf.FloorToInt(nextValue);

        var closestDistance = float.MaxValue;
        foreach (var enemy in enemies)
        {
            var distance = Vector3.Distance(enemy.transform.position, player.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }



        int numToFire = nextValueInt - currentValueInt;
        var cannons = fireLeft ? cannonsLeft : cannonsRight;
        int i = 0, visited = 0;
        while (i < numToFire && visited < cannons.Length)
        {
            int index = (currentValueInt + i) % cannons.Length;
            var cannon = cannons[index];
            if (!cannon.IsReloading())
            {
                if (closestEnemy != null)
                {
                    cannon.AimAt(closestEnemy.rigidbody);
                }
                else
                {
                    cannon.ResetAim();
                }
                cannon.Fire();
            }
            i++;
            visited++;
        }
        if (fireLeft)
        {
            currentCannonLeft = nextValue;
        }
        else
        {
            currentCannonRight = nextValue;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        var enemy = collision.GetComponent<Health>();
        if (enemy != null && enemy.IsEnemy(myHealth.team))
        {
            enemies.AddLast(enemy);
            enemy.OnHealthDestroy += () => { if (enemies.Contains(enemy)) enemies.Remove(enemy); };
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        var enemy = collision.GetComponent<Health>();
        if (enemy != null)
        {
            enemies.Remove(enemy);
        }
    }

    private void OnDrawGizmos()
    {
        if (closestEnemy)
        {
            Gizmos.DrawWireSphere(closestEnemy.transform.position, 3);
        }
    }

    void OnDamage(Vector3 point, Rigidbody cannonball)
    {
        var damageInstance = Instantiate(damageParticles, point, Quaternion.identity);
        damageInstance.GetComponent<ParticleSystem>().Play();
        Debug.DrawRay(point, Vector3.up);
        damageAudio.PlayOne(audioSource);
        player.AddForceAtPosition(cannonball.velocity * cannonball.mass, point);
        float healthPercent = myHealth.health / myHealth.GetInitialHealth();
        // more mass
        player.mass = initialMass * (2 - healthPercent);
        // less buoyancy
        boatProbes._forceMultiplier = initialBuoyancy * Mathf.Pow(1 - healthPercent, .2f);
    }

    private void OnHealthDestroy()
    {
        ParticleSystem deathParticleInstance = null;
        var deathInstance = Instantiate(deathParticles, transform.position, Quaternion.identity);
        deathParticleInstance = deathInstance.GetComponent<ParticleSystem>();
        deathParticleInstance.Play();
    }
}
