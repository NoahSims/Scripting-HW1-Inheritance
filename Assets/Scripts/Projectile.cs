using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private Rigidbody _rb;

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 _moveOffset = transform.forward * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + _moveOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Player" && other.gameObject.tag != "PlayerProjectile")
        {
            Debug.Log("Collided with" + other.gameObject.name);
            Kill();
        }
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
