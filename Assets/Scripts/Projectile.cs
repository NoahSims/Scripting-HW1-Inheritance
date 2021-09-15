using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] ParticleSystem _impactParticles;
    [SerializeField] AudioClip _impactSound;

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 _moveOffset = transform.forward * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + _moveOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Player" && other.gameObject.tag != "PlayerProjectile")
        {
            Debug.Log("Collided with" + other.gameObject.name);
            Feedback();
            Kill();
        }
    }

    private void Kill()
    {
        Destroy(gameObject);
    }

    private void Feedback()
    {
        // particles
        if (_impactParticles != null)
        {
            ParticleSystem _particles = Instantiate(_impactParticles, transform.position, Quaternion.identity);
            _particles.Play();
        }
        // audio. TODO - consider Object Pooling for performance
        if (_impactSound != null)
        {
            AudioHelper.PlayClip2D(_impactSound, 1f);
        }
    }
}
