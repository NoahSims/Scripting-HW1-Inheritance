using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankController))]
public class Player : MonoBehaviour
{
    TankController _tankController;
    [SerializeField] UIWriter _uiWriter;

    [SerializeField] int _maxHealth = 3;
    int _currentHealth;

    [SerializeField] private int _treasureCount = 0;
    public int TreasureCount
    {
        get => _treasureCount;
        set
        {
            _treasureCount = value;
            _uiWriter.SetUITreasure(_treasureCount);
        }
    }
    

    private void Awake()
    {
        _tankController = GetComponent<TankController>();
    }

    
    void Start()
    {
        _currentHealth = _maxHealth;
        _uiWriter.SetUIHealth(_currentHealth);
        _uiWriter.SetUITreasure(_treasureCount);
    }

    public void IncreaseHealth(int amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        Debug.Log("Player's health: " + _currentHealth);
        _uiWriter.SetUIHealth(_currentHealth);
    }

    public void DecreaseHealth(int amount)
    {
        _currentHealth -= amount;
        Debug.Log("Player's health: " + _currentHealth);
        _uiWriter.SetUIHealth(_currentHealth);
        if (_currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        gameObject.SetActive(false);
        // play particles
        // play sounds
    }
}
