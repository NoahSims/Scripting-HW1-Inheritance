using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWriter : MonoBehaviour
{
    [SerializeField] private List<Image> _playerHearts;
    [SerializeField] private Image _bossHealthBar;
    [SerializeField] private Image _damageVignette;
    [SerializeField] private float _damageVignetteIntensity = 0.5f;
    [SerializeField] private float _damageVignetteDuration = 0.5f;
    [SerializeField] private Text _endText;
    
    [SerializeField] private GameObject _player;
    private Health _playerHealth;
    [SerializeField] private GameObject _boss;
    private Health _bossHealth;

    private void Start()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerHealth.Damaged += OnPlayerDamaged;
        _playerHealth.Healed += OnPlayerHealed;
        _playerHealth.Killed += LoseScreen;

        _bossHealth = _boss.GetComponent<Health>();
        _bossHealth.Damaged += OnBossDamaged;
        _bossHealth.Killed += WinScreen;
    }

    private void OnDisable()
    {
        _playerHealth.Damaged -= OnPlayerDamaged;
        _playerHealth.Healed -= OnPlayerHealed;
        _playerHealth.Killed -= LoseScreen;

        _bossHealth.Damaged -= OnBossDamaged;
        _bossHealth.Killed -= WinScreen;
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
            }                
        }
        StartCoroutine(DamageVignette());
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
        //_bossHealthBar.rectTransform.localScale = new Vector3(((float)_bossHealth.CurrentHealth / (float)_bossHealth.MaxHealth), 1, 1);
        StartCoroutine(HealthBarLerp());
    }

    IEnumerator HealthBarLerp()
    {
        Vector3 healthGoal = new Vector3(((float)_bossHealth.CurrentHealth / (float)_bossHealth.MaxHealth), 1, 1);
        while (_bossHealthBar.rectTransform.localScale != healthGoal)
        {
            healthGoal = new Vector3(((float)_bossHealth.CurrentHealth / (float)_bossHealth.MaxHealth), 1, 1);
            Vector3 healthScale = Vector3.Lerp(_bossHealthBar.rectTransform.localScale, healthGoal, Time.deltaTime * 10);
            _bossHealthBar.rectTransform.localScale = healthScale;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DamageVignette()
    {

        Color newColor = _damageVignette.color;
        newColor.a = _damageVignetteIntensity;
        _damageVignette.color = newColor;
        yield return new WaitForSeconds(_damageVignetteDuration);
        newColor.a = 0;
        _damageVignette.color = newColor;
    }

    private void WinScreen()
    {
        _endText.text = "YOU WIN";
    }

    private void LoseScreen()
    {
        _endText.text = "YOU DIED";
    }

    /*
    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
    */
}
