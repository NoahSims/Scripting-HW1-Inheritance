using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWriter : MonoBehaviour
{
    //[SerializeField] Text _healthText;
    [SerializeField] private List<Image> _playerHearts;
    [SerializeField] private ParticleSystem _playerHeartParticles;
    [SerializeField] private Image _bossHealthBar;
    //[SerializeField] Text _bossHealthText;
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

        //_healthText.text = "Health = " + _playerHealth.MaxHealth;
        //_bossHealthText.text = "Boss Health = " + _bossHealth.MaxHealth;
    }

    private void OnPlayerDamaged(int ammount)
    {
        //_healthText.text = "Health = " + _playerHealth.CurrentHealth;
        for (int i = _playerHearts.Count - 1; i >= 0; i--)
        {
            if (i < _playerHealth.CurrentHealth)
            {
                continue;
            }
            else if (Vector3.Equals(_playerHearts[i].rectTransform.localScale, new Vector3(1, 1, 1)))
            {
                _playerHearts[i].rectTransform.localScale = new Vector3(0.3f, 0.3f, 1);
                ParticleSystem particles = _playerHearts[i].GetComponentInChildren<ParticleSystem>();
                
                if (particles != null)
                {
                    //ParticleSystem _particles = Instantiate(particles, particles.transform.position, Quaternion.identity);
                    particles.Play();

                }
            }                
        }
    }

    private void OnPlayerHealed(int ammount)
    {
        //_healthText.text = "Health = " + _playerHealth.CurrentHealth;
        for (int i = 0; i < _playerHearts.Count; i++)
        {
            if (i < _playerHealth.CurrentHealth)
            {
                _playerHearts[i].rectTransform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void OnBossDamaged(int ammount)
    {
        //_bossHealthText.text = "Boss Health = " + _bossHealth.CurrentHealth;
        //_bossHealthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, );
        _bossHealthBar.rectTransform.localScale = new Vector3(((float)_bossHealth.CurrentHealth / (float)_bossHealth.MaxHealth), 1, 1);
    }

    /*
    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
    */
}
