using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWriter : MonoBehaviour
{
    [SerializeField] Text _healthText;
    [SerializeField] Text _bossHealthText;
    //[SerializeField] Text _treasureText;
    [SerializeField] private GameObject _player;
    private Health _playerHealth;
    [SerializeField] private GameObject _boss;
    private Health _bossHealth;

    private void Start()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerHealth.Damaged += OnPlayerDamaged;
        _playerHealth.Healed += OnPlayerHealed;

        _bossHealth = _boss.GetComponent<Health>();
        _bossHealth.Damaged += OnBossDamaged;

        _healthText.text = "Health = " + _playerHealth.MaxHealth;
        _bossHealthText.text = "Boss Health = " + _bossHealth.MaxHealth;
    }

    private void OnPlayerDamaged(int ammount)
    {
        _healthText.text = "Health = " + _playerHealth.CurrentHealth;
    }

    private void OnPlayerHealed(int ammount)
    {
        _healthText.text = "Health = " + _playerHealth.CurrentHealth;
    }

    private void OnBossDamaged(int ammount)
    {
        _bossHealthText.text = "Boss Health = " + _bossHealth.CurrentHealth;
    }

    /*
    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
    */
}
