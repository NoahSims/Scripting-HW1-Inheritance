using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MortarProjectile : MonoBehaviour
{
    public static event Action<float, float> MortarExploded;
    [SerializeField] private float _mortarExplosionShakeDuration = 0.1f;
    [SerializeField] private float _mortarExplosionShakeMagnitude = 0.1f;
    [SerializeField] private int _damageAmmount = 1;
    private bool _isMoving = true;
    private float _moveSpeed = 1f;
    private GameObject _target = null;
    private GameObject _boss = null;
    [SerializeField] private ParticleSystem _impactParticles = null;
    [SerializeField] private AudioClip _impactSound = null;

    public void SetValues(float speed, GameObject target, GameObject boss)
    {
        _moveSpeed = speed;
        _target = target;
        _boss = boss;
    }

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 heading = _target.transform.position - transform.position;
        //Debug.Log(heading.magnitude);
        if (heading.magnitude > 0.7f)
        {
            heading = Vector3.Normalize(heading);
            transform.position += heading * _moveSpeed;
        }
        else
        {
            _isMoving = false;
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("player hit");
            other.gameObject.GetComponent<Health>()?.TakeDamage(_damageAmmount);
            Explode();
        }
    }

    private void Explode()
    {
        ImpactFeedback();
        MortarExploded.Invoke(_mortarExplosionShakeDuration, _mortarExplosionShakeMagnitude);
        Destroy(_target); // target is the crosshair this is aimed at
        Destroy(gameObject);
    }

    private void ImpactFeedback()
    {
        // particles
        if (_impactParticles != null)
        {
            ParticleSystem _particles = Instantiate(_impactParticles, transform.position, Quaternion.identity);
            _particles.Play();
        }
        // audio. TODO - consider object pooling for performance
        if (_impactSound != null)
        {
            AudioHelper.PlayClip2D(_impactSound, 1f);
        }
    }
}
