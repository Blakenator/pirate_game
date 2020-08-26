using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CannonController : MonoBehaviour
{
    public GameObject cannonball;
    public Transform spawn;
    public Transform aim;
    public Rigidbody player;
    public HashSet<Collider> doNotCollide;
    public float force = 100f;
    public float minReloadTime = 3f;
    public float maxReloadTime = 7f;
    public float reloadTimeLeft = 0f;
    public AudioClipGroup shootAudioOptions;
    public AudioClip reloadAudio;
    private AudioSource audio;
    public VisualEffect shootEffect;
    public float forceFactor = 10f;
    public float maxAimAngle = 20f;
    public Animator animator;
    public bool playPlayerSounds = false;

    private Quaternion initialAim;
    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        initialAim = aim.localRotation.normalized;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (reloadTimeLeft > 0)
        {
            reloadTimeLeft -= Time.deltaTime;
            if (reloadTimeLeft < 0 && playPlayerSounds)
            {
                audio.PlayOneShot(reloadAudio, .3f);
                Debug.Log("reload sound");
            }
            if (reloadTimeLeft < .5f && !animator.IsInTransition(0))
            {
                animator.SetTrigger("doReload");
            }
        }
        lastPosition = transform.position;
    }

    public void ResetAim()
    {
        aim.localRotation = initialAim;
    }

    private Vector3 GetSpawnVelocity()
    {
        return (transform.position - lastPosition) / Time.deltaTime;
    }

    public void AimAt(Rigidbody enemy)
    {
        Vector3 solution;
        Vector3 solution2;
        var speed = force / forceFactor / cannonball.GetComponent<Rigidbody>().mass;

        int numSolutions = TrajectorySolver.SolveBallistic(spawn.position, speed, enemy.position, enemy.velocity - GetSpawnVelocity(), Physics.gravity.magnitude / 1f, out solution, out solution2);
        if (numSolutions > 0)
        {
            float angleBetween = Vector3.Angle(solution, transform.forward);
            Vector3 lookAim = solution.normalized;
            if (angleBetween > maxAimAngle)
            {
                lookAim = Vector3.RotateTowards(lookAim, transform.forward, angleBetween - maxAimAngle / 180f * Mathf.PI, 0.1f).normalized;
                Debug.DrawRay(spawn.position, lookAim, Color.red, 4);
            }
            else
            {
                Debug.DrawRay(spawn.position, lookAim, Color.cyan, 4);
            }
            aim.rotation = Quaternion.LookRotation(lookAim);
        }
        else
        {
            Debug.DrawRay(spawn.position, spawn.forward, Color.magenta, 4);
        }
    }

    public void Fire()
    {
        var instant = Instantiate(cannonball, spawn.position, spawn.rotation);
        instant.GetComponent<CannonballController>().doNotCollide = doNotCollide;
        var collider = instant.GetComponent<Rigidbody>();
        collider.velocity = player.velocity;
        collider.AddForce(spawn.forward * force);
        player.AddForceAtPosition(spawn.forward * force, spawn.position);

        shootAudioOptions.PlayOne(audio, .5f);

        shootEffect.SetVector3("position", spawn.position);
        shootEffect.SetVector3("outDirection", spawn.forward);
        shootEffect.Play();

        animator.ResetTrigger("doReload");
        animator.SetTrigger("doFire");

        reloadTimeLeft = Random.Range(minReloadTime, maxReloadTime);
    }

    public bool IsReloading()
    {
        return reloadTimeLeft > 0;
    }
}
