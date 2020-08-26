using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ShipController ship;
    public int numCannons = 14;

    // Start is called before the first frame update
    void Start()
    {
        ship.InitializeShip(numCannons);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            ship.FireSomeCannons(true);
        }
        if (Input.GetButton("Fire2"))
        {
            ship.FireSomeCannons(false);
        }
    }
}
