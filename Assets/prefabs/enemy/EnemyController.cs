using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Health health;
    public Rigidbody rigidbody;
    public AudioSource audioSource;
    public ShipController ship;

    public bool isFiring = false;

    void Start()
    {
        health.OnHealthDestroy += () => Destroy(gameObject);
        ship.InitializeShip(14);
    }

    private void Update()
    {
        if (ship.HasEnemy())
        {
            isFiring = true;
            ship.FireSomeCannons(true);
            ship.FireSomeCannons(false);
        }
        else
        {
            isFiring = false;
        }
    }
}
