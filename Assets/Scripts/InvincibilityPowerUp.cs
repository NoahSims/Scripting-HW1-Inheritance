using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibilityPowerUp : PowerUpBase
{
    [SerializeField] Material _invincibilityMaterial;

    protected override void PowerUp(Player player)
    {
        player.GetComponentInParent<Health>().IsInvincible = true;
        player.SetBodyColor(_invincibilityMaterial.color);
        Debug.Log("Setting Body Color");
    }

    protected override void PowerDown(Player player)
    {
        player.GetComponentInParent<Health>().IsInvincible = false;
        player.ResetMaterial();
    }
}
