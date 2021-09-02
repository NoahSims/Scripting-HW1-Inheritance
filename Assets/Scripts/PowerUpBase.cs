using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUpBase : MonoBehaviour
{
    [SerializeField] float _powerupDuration = 3.0f;
    [SerializeField] ParticleSystem _powerUpParticles;
    [SerializeField] AudioClip _powerUpSound;

    protected abstract void PowerUp(Player player);
    protected abstract void PowerDown(Player player);

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            // powerUp
            PowerUp(player);
            Feedback();

            // disable powerUp visuals
            gameObject.GetComponentInChildren<Renderer>().enabled = false;
            gameObject.GetComponentInChildren<Collider>().enabled = false;

            // powerDown after duration
            StartCoroutine(PowerDownCoroutine(player));
        }
    }

    private void Feedback()
    {
        // particles
        if (_powerUpParticles != null)
        {
            _powerUpParticles = Instantiate(_powerUpParticles, transform.position, Quaternion.identity);
            _powerUpParticles.Play();
        }
        // audio. TODO - consider Object Pooling for performance
        if (_powerUpSound != null)
        {
            AudioHelper.PlayClip2D(_powerUpSound, 1f);
        }
    }

    IEnumerator PowerDownCoroutine(Player player)
    {
        yield return new WaitForSeconds(_powerupDuration);
        PowerDown(player);
        gameObject.SetActive(false);
    }
}
