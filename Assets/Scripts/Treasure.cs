using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class Treasure : CollectableBase
{
    [SerializeField] int _treasureAdded = 1;
    protected override void Collect(Player player)
    {
        player.TreasureCount += _treasureAdded;
        Debug.Log("Treasure = " + player.TreasureCount);
    }

    protected override void Movement(Rigidbody rb)
    {
        // calculate rotation
        Quaternion turnOffset = Quaternion.Euler(MovementSpeed, 0, MovementSpeed);
        rb.MoveRotation(rb.rotation * turnOffset);
    }
}
*/