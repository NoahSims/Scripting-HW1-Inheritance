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
        _playerHealth = _player.GetComponent<Health>();
        _playerHealth.Damaged += OnPlayerDamaged;
        _playerHealth.Healed += OnPlayerHealed;
    }

    private void OnPlayerDamaged(int ammount)
    {
        _healthText.text = "Health = " + _playerHealth.CurrentHealth;
    }

    private void OnPlayerHealed(int ammount)
    {
        _healthText.text = "Health = " + _playerHealth.CurrentHealth;
    }

    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
}
