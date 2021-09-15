using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if(_currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {

    }
}
