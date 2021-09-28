using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCrosshair : MonoBehaviour
{
    private bool _isMoving = true;
    private float _moveSpeed = 1f;
    private GameObject _target = null;
    private GameObject _boss = null;
    [SerializeField] private Material _mTargetAquired = null;

    public void SetValues(float speed, GameObject target, GameObject boss)
    {
        _moveSpeed = speed;
        _target = target;
        _boss = boss;
    }

    private void FixedUpdate()
    {
        if(_isMoving)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 heading = _target.transform.position - transform.position;
        //Debug.Log(heading.magnitude);
        if (heading.magnitude > 0.7f)
        {
            heading.y = 0; // ignore vertical
            heading = Vector3.Normalize(heading);
            transform.position += heading * _moveSpeed;
        }
        else
        {
            _isMoving = false;
            gameObject.GetComponent<MeshRenderer>().material = _mTargetAquired;
            _boss.GetComponent<Boss>().TargetAquired(gameObject);
        }
    }
}
