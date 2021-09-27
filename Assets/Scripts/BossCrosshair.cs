using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCrosshair : MonoBehaviour
{
    private bool _isMoving = true;
    private float _moveSpeed = 5f;
    public GameObject _target = null;

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            Vector3 heading = _target.transform.position - transform.position;
            heading.y = 0.01f; // ignore vertical
            heading = Vector3.Normalize(heading);
            transform.position += heading * _moveSpeed;
        }
    }
}
