using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private float _playerDamagedShakeDuration;
    private Health _playerHealth;

    private void Start()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerHealth.Damaged += OnPlayerDamaged;
    }

    private void OnPlayerDamaged(int ammount)
    {
        StartCoroutine(ShakeCamera(_playerDamagedShakeDuration, ammount));
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        transform.localPosition = originalPos;
    }
}
