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

    [SerializeField] private GameObject _turret;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _projectileSpawn;
    [SerializeField] private float _projectileCooldown = .2f;
    private bool _isGunOnCooldown = false;
    [SerializeField] ParticleSystem _projectileParticles;
    [SerializeField] AudioClip _projectileSound;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        MoveTank();
        TurnTank();
        if(Input.GetAxis("FireHorizontal") != 0 || Input.GetAxis("FireVertical") != 0)
        {
            TurnTurret();
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

    private void TurnTurret()
    {
        float _tempRotation = 0;

        if (Input.GetAxis("FireVertical") > 0)
        {
            if (Input.GetAxis("FireHorizontal") == 0)
                _tempRotation = 0;
            else if (Input.GetAxis("FireHorizontal") > 0)
                _tempRotation = 45;
            else if (Input.GetAxis("FireHorizontal") < 0)
                _tempRotation = -45;
        }
        else if (Input.GetAxis("FireVertical") < 0)
        {
            if (Input.GetAxis("FireHorizontal") == 0)
                _tempRotation = 180;
            else if (Input.GetAxis("FireHorizontal") > 0)
                _tempRotation = 135;
            else if (Input.GetAxis("FireHorizontal") < 0)
                _tempRotation = 225;
        }
        else if (Input.GetAxis("FireHorizontal") > 0)
            _tempRotation = 90;
        else if (Input.GetAxis("FireHorizontal") < 0)
            _tempRotation = -90;

        _turret.transform.rotation = Quaternion.Euler(_turret.transform.rotation.x, _tempRotation, _turret.transform.rotation.z);
    }

    public void ShootProjectile()
    {
        if (!_isGunOnCooldown)
        {
            //Debug.Log(_turret.transform.rotation.y);
            //Quaternion _projectileRotation = Quaternion.Euler(0, _turret.transform.rotation.y, 0);
            Instantiate(_projectile, _projectileSpawn.transform.position, _turret.transform.rotation);
            _isGunOnCooldown = true;
            ProjectileFeedback();
            StartCoroutine(ProjectileCooldown());
        }
    }

    private void ProjectileFeedback()
    {
        // particles
        if (_projectileParticles != null)
        {
            ParticleSystem _particles = Instantiate(_projectileParticles, _projectileSpawn.transform.position, _turret.transform.rotation);
            _particles.Play();
        }
        // audio. TODO - consider Object Pooling for performance
        if (_projectileSound != null)
        {
            AudioHelper.PlayClip2D(_projectileSound, 1f);
        }
    }

    IEnumerator ProjectileCooldown()
    {
        yield return new WaitForSeconds(_projectileCooldown);
        _isGunOnCooldown = false;
    }

    private void OnDisable()
    {
        _turret.SetActive(false);
    }

    private void OnEnable()
    {
        _turret.SetActive(true);
    }
}
