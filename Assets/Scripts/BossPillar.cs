using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPillar : MonoBehaviour
{
    [SerializeField] private List<Vector3> _points;
    private int _currentTarget = 0;
    private int _pointsListDirection = 1;
    [SerializeField] public bool _isCurentlyActive;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _delayTime = 2f;
    private float _delayStartTime;
    private float _tolerance; // the distance moved in one fixed update

    private void Start()
    {
        _tolerance = _moveSpeed * Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        if (_isCurentlyActive)
            ActivatedAction();
        else
            DeactivatedAction();
    }

    private void ActivatedAction()
    {
        if (transform.position != _points[_currentTarget])
        {
            MovePlatform();
        }
        else
        {
            UpdateTarget();
        }
    }

    private void DeactivatedAction()
    {
        if (transform.position != _points[0])
        {
            _currentTarget = 0;
            _pointsListDirection = 1;
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        Vector3 _heading = _points[_currentTarget] - transform.position;
        if (_heading.magnitude < _tolerance)
        {
            //_rb.MovePosition(_points[_currentTarget]);
            transform.position = _points[_currentTarget];
        }
        else
        {
            //_rb.MovePosition(_rb.position + ((_heading / _heading.magnitude) * _moveSpeed * Time.fixedDeltaTime));
            transform.position += (_heading / _heading.magnitude) * _moveSpeed * Time.fixedDeltaTime;
            _delayStartTime = Time.time;
        }
    }

    private void UpdateTarget()
    {
        if (Time.time - _delayStartTime >= _delayTime)
        {
            _currentTarget += _pointsListDirection;
            if (_currentTarget < 0 || _currentTarget >= _points.Count)
            {
                _pointsListDirection = _pointsListDirection * -1;
                _currentTarget += _pointsListDirection * 2;
                Mathf.Clamp(_currentTarget, 0, _points.Count);
            }
        }
        else if (_currentTarget > 0 && _currentTarget < _points.Count - 1)
        {
            _currentTarget += _pointsListDirection;
        }
    }
}
