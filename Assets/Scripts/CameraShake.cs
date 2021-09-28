using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 _originalPos = new Vector3();

    [SerializeField] private GameObject _player;
    [SerializeField] private float _playerDamagedShakeDuration;
    private Health _playerHealth;

    [SerializeField] private GameObject _bossObject;
    private Boss _boss;

    private void Start()
    {
        _originalPos = transform.localPosition;

        _playerHealth = _player.GetComponent<Health>();
        _playerHealth.Damaged += OnPlayerDamaged;

        _boss = _bossObject.GetComponent<Boss>();
        _boss.Crashed += OnBossCrashed;

        MortarProjectile.MortarExploded += OnMortarExploded;
    }

    private void OnDisable()
    {
        _playerHealth.Damaged -= OnPlayerDamaged;
        _boss.Crashed -= OnBossCrashed;
        MortarProjectile.MortarExploded -= OnMortarExploded;
    }

    private void OnPlayerDamaged(int ammount)
    {
        StartCoroutine(ShakeCamera(_playerDamagedShakeDuration, ammount));
    }

    private void OnBossCrashed(float duration, float magnitude)
    {
        StartCoroutine(ShakeCamera(duration, magnitude));
    }

    private void OnMortarExploded(float duration, float magnitude)
    {
        StartCoroutine(ShakeCamera(duration, magnitude));
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, _originalPos.z);

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        transform.localPosition = _originalPos;
    }
}
