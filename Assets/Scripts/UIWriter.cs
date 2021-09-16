using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWriter : MonoBehaviour
{
    [SerializeField] Text _healthText;
    [SerializeField] Text _treasureText;
    [SerializeField] private GameObject _player;
    private Health _playerHealth;

    private void Start()
    {
        _playerHealth = _player.GetComponentInChildren<Health>();
    }

    private void FixedUpdate()
    {
        _healthText.text = "Health = " + _playerHealth.CurrentHealth;
    }

    /*
    public void SetUIHealth(int value)
    {
        _healthText.text = "Health = " + value;
    }
    */

    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
}
