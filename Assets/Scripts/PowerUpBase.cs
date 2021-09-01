using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUpBase : MonoBehaviour
{
    [SerializeField] float _powerupDuration = 3.0f;

    protected abstract void PowerUp(Player player);
    protected abstract void PowerDown(Player player);

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            // powerUp
            PowerUp(player);

            // disable powerUp visuals
            gameObject.GetComponentInChildren<Renderer>().enabled = false;
            gameObject.GetComponentInChildren<Collider>().enabled = false;

            // powerDown after duration
            StartCoroutine(PowerDownCoroutine(player));
        }
    }

    IEnumerator PowerDownCoroutine(Player player)
    {
        yield return new WaitForSeconds(_powerupDuration);
        PowerDown(player);
        gameObject.SetActive(false);
    }
}
