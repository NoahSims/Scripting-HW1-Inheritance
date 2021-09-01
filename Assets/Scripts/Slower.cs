using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slower : Enemy
{
    [SerializeField] float _speedMult = 0.5f;
    [SerializeField] float _turnSpeedMult = 0.5f;

    protected override void PlayerImpact(Player player)
    {
        TankController controller = player.GetComponent<TankController>();
        if (controller != null)
        {
            controller.MoveSpeed = controller.MoveSpeed * _speedMult;
            controller.TurnSpeed = controller.TurnSpeed * _turnSpeedMult;
        }
    }
}
