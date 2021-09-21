using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    public event Action<int> Damaged = delegate { };
    public event Action<int> Healed = delegate { };
    public event Action Killed = delegate { };

    [SerializeField] private int _maxHealth = 3;
    public int MaxHealth { get => _maxHealth; }
    private int _currentHealth;
    public int CurrentHealth { get => _currentHealth; }
    [SerializeField] private bool _isInvincible;
    public bool IsInvincible
    {
        get => _isInvincible;
        set => _isInvincible = value;
    }
    [SerializeField] private float _iFrameTime = 0;

    [SerializeField] ParticleSystem _damageParticles;
    [SerializeField] AudioClip _damageSound;
    [SerializeField] ParticleSystem _deathParticles;
    [SerializeField] AudioClip _deathSound;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!_isInvincible)
        {
            _currentHealth -= damage;
            Damaged.Invoke(damage);
            if (_currentHealth <= 0)
            {
                Kill();
            }
            else
            {
                Feedback(_damageParticles, _damageSound);
                StartCoroutine(IFrames());
            }
        }
    }

    public void IncreaseHealth(int amount)
    {
        _currentHealth += amount;
        Healed.Invoke(amount);
        Mathf.Clamp(_currentHealth, 0, _maxHealth);
    }

    public void Kill()
    {
        Killed.Invoke();
        Feedback(_deathParticles, _deathSound);
        gameObject.SetActive(false);
    }

    private void Feedback(ParticleSystem particles, AudioClip sound)
    {
        // particles
        if (particles != null)
        {
            ParticleSystem _particles = Instantiate(particles, transform.position, Quaternion.identity);
            _particles.Play();
        }
        // audio. TODO - consider Object Pooling for performance
        if (sound != null)
        {
            AudioHelper.PlayClip2D(sound, 1f);
        }
    }

    IEnumerator IFrames()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_iFrameTime);
        _isInvincible = false;
    }
}
