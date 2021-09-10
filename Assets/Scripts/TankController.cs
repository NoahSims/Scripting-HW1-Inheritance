using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = .25f;
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }
    [SerializeField] float _turnSpeed = 2f;
    public float TurnSpeed
    {
        get => _turnSpeed;
        set => _turnSpeed = value;
    }

    Rigidbody _rb = null;

    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _projectileSpawn;
    [SerializeField] private float _projectileCooldown = .2f;
    [SerializeField] private bool _isGunOnCooldown = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        MoveTank();
        TurnTank();
        if(Input.GetAxis("Jump") != 0 && !_isGunOnCooldown)
        {
            ShootProjectile();
        }
    }

    public void MoveTank()
    {
        // calculate the move amount
        float moveAmountThisFrame = Input.GetAxis("Vertical") * _moveSpeed;
        // create a vector from amount and direction
        Vector3 moveOffset = transform.forward * moveAmountThisFrame;
        // apply vector to the rigidbody
        _rb.MovePosition(_rb.position + moveOffset);
        // technically adjusting vector is more accurate! (but more complex)
    }

    public void TurnTank()
    {
        // calculate the turn amount
        float turnAmountThisFrame = Input.GetAxis("Horizontal") * _turnSpeed;
        // create a Quaternion from amount and direction (x,y,z)
        Quaternion turnOffset = Quaternion.Euler(0, turnAmountThisFrame, 0);
        // apply quaternion to the rigidbody
        _rb.MoveRotation(_rb.rotation * turnOffset);
    }

    public void ShootProjectile()
    {
        Instantiate(_projectile, _projectileSpawn.transform);
        _isGunOnCooldown = true;
        StartCoroutine(ProjectileCooldown());
    }
    
    IEnumerator ProjectileCooldown()
    {
        yield return new WaitForSeconds(_projectileCooldown);
        _isGunOnCooldown = false;
    }
}
