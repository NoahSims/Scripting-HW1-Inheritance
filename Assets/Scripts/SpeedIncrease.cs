using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedIncrease : CollectableBase
{
    [SerializeField] float _speedAmount = 0.25f;
    [SerializeField] float _turnSpeedAmount = 2f;

    protected override void Collect(Player player)
    {
        // pull motor conrtoller from the player
        TankController controller = player.GetComponent<TankController>();
        if(controller != null)
        {
            controller.MoveSpeed += _speedAmount;
            controller.TurnSpeed += _turnSpeedAmount;
        }
    }

    protected override void Movement(Rigidbody rb)
    {
        // calculate rotation
        Quaternion turnOffset = Quaternion.Euler(MovementSpeed, MovementSpeed, MovementSpeed);
        rb.MoveRotation(rb.rotation * turnOffset);
    }
}
