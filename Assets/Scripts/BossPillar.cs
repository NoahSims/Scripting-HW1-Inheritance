using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 *  Not gonna lie, I may have borrowed parts of this script from another project I'm working on, just because I needed to get this
 *  working. I will probably clean this up later.
 */

public class BossPillar : MonoBehaviour
{
    public event Action<bool> PillarInPosition = delegate { };
    [SerializeField] private List<Vector3> _points;
    private int _currentTarget = 0;
    [SerializeField] public bool _isCurentlyActive = false;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _delayTime = 0;
    private float _delayStartTime;
    private float _tolerance; // the distance moved in one fixed update

    [SerializeField] private Health _health;
    [SerializeField] private Material _standardMaterial;
    [SerializeField] private Material _damagedMaterial;

    private void Start()
    {
        _tolerance = _moveSpeed * Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        _health.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        PillarInPosition.Invoke(false);
        _health.Damaged -= OnDamaged;
    }

    private void FixedUpdate()
    {
        if (_isCurentlyActive)
        {
            if (_currentTarget <= _points.Count && transform.position != _points[_currentTarget])
            {
                MovePlatform();
            }
            else
            {
                UpdateTarget();
            }
        }
    }

    private void MovePlatform()
    {
        Vector3 _heading = _points[_currentTarget] - transform.position;
        if (_heading.magnitude < _tolerance)
        {
            transform.position = _points[_currentTarget];
        }
        else
        {
            transform.position += (_heading / _heading.magnitude) * _moveSpeed * Time.fixedDeltaTime;
            _delayStartTime = Time.time;
        }
    }

    private void UpdateTarget()
    {
        if (Time.time - _delayStartTime >= _delayTime)
        {
            _currentTarget++;
            if(_currentTarget >= _points.Count)
            {
                _isCurentlyActive = false;
                PillarInPosition.Invoke(true);
            }
        }
    }

    public void ActivatePillar()
    {
        _isCurentlyActive = true;
    }

    private void OnDamaged(int ammount)
    {
        StartCoroutine(FlashOnDamage());
    }

    IEnumerator FlashOnDamage()
    {
        if(_damagedMaterial != null)
        {
            gameObject.GetComponent<MeshRenderer>().material = _damagedMaterial;
            yield return new WaitForSeconds(0.5f);
            gameObject.GetComponent<MeshRenderer>().material = _standardMaterial;
        }
    }
}
