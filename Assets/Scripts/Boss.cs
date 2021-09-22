using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This whole script is kind of a mess. I still need to figure out a lot of the movement, but at least two of them are kind of working now
 */

public class Boss : Enemy
{
    [SerializeField] private float _arenaMaxX;
    [SerializeField] private float _arenaMinX;
    [SerializeField] private float _arenaMaxZ;
    [SerializeField] private float _arenaMinZ;

    [SerializeField] private GameObject _player;
    private int _rotationAngle = 80;

    [SerializeField] private Health _bossHealth;

    private enum movementStates { RAMPAGE, ROTATION, CHARGE, TO_PLAYER, PILLAR, WAIT};
    [SerializeField] private movementStates _movementState = movementStates.ROTATION;

    private Vector3 _chargeHeading;

    [SerializeField] private Vector3 _pillarSpot;
    [SerializeField] private GameObject _pillar; // this is sloppy, but I'm running out of time and just want it to work
    private float _tolerance; // when boss is within this distance to pillar, warp to pillar, rather than move to it

    private void Start()
    {
        _tolerance = 3;
        _chargeHeading = getRandomHeading();
    }

    private void OnEnable()
    {
        _bossHealth.Damaged += OnBossDamaged;
    }

    private void OnDisable()
    {
        _bossHealth.Damaged -= OnBossDamaged;
    }

    private void OnBossDamaged(int ammount)
    {
        if (_movementState == movementStates.RAMPAGE)
        {
            int coin = (int)Mathf.Round(Random.value);
            if (coin == 1)
            {
                _movementState = movementStates.ROTATION;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_movementState != movementStates.WAIT && _bossHealth.CurrentHealth < _bossHealth.MaxHealth / 2)
        {
            _movementState = movementStates.PILLAR;
        }

        switch (_movementState)
        {
            case movementStates.RAMPAGE:
                RampageMovement();
                break;
            case movementStates.ROTATION:
                RotationMovement();
                break;
            case movementStates.CHARGE:
                ChargeAttack();
                break;
            case movementStates.TO_PLAYER:
                MoveTowardsPlayer();
                break;
            case movementStates.PILLAR:
                PillarTime();
                break;
            case movementStates.WAIT:
                //FacePlayer();
                break;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    #region Rampage Movement

    private Vector3 getRandomHeading()
    {
        Vector3 heading = new Vector3(1, 0, 0);
        heading = Quaternion.Euler(0, Random.Range(-45, 45), 0) * heading;
        int coin = (int)Mathf.Round(Random.value);
        if (coin == 1)
            heading = heading * (-1);

        return Vector3.Normalize(heading);
    }

    private void RampageMovement()
    {
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(_chargeHeading), 0.5f));

        Vector3 move = _rb.position + (transform.forward * MoveSpeed * 5);
        if (move.x > _arenaMaxX || move.x < _arenaMinX)
        {
            StartCoroutine(RotateXCooldown());
        }
        else if(move.z > _arenaMaxZ || move.z < _arenaMinZ)
        {
            StartCoroutine(RotateZCooldown());
        }
        else
        {
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
    }

    private bool rotateXOnCooldown = false;
    IEnumerator RotateXCooldown()
    {
        if(!rotateXOnCooldown)
        {
            rotateXOnCooldown = true;
            _chargeHeading.x = _chargeHeading.x * (-1);
            _chargeHeading = Quaternion.Euler(0, Random.Range(-30, 30), 0) * _chargeHeading;

            yield return new WaitForSeconds(1f);
            rotateXOnCooldown = false;
        }
    }

    private bool rotateZOnCooldown = false;
    IEnumerator RotateZCooldown()
    {
        if (!rotateZOnCooldown)
        {
            rotateZOnCooldown = true;
            //_rampageHeading = new Vector3(_rampageHeading.x, _rampageHeading.y, _rampageHeading.z * (-1));
            _chargeHeading.z = _chargeHeading.z * (-1);
            _chargeHeading = Quaternion.Euler(0, Random.Range(-30, 30), 0) * _chargeHeading;

            yield return new WaitForSeconds(1f);
            rotateZOnCooldown = false;
        }
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region Rotation Movement
    private void RotationMovement()
    {
        StartCoroutine(StartChargeAttackSequence());
        // get heading
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = 0; // ignore vertical
        heading = Vector3.Normalize(heading);

        //rotate
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading) * Quaternion.Euler(0, _rotationAngle, 0), 0.5f));

        //don't run into wall
        Vector3 move = _rb.position + (transform.forward * MoveSpeed * 5);
        if (move.x > _arenaMaxX || move.x < _arenaMinX || move.z > _arenaMaxZ || move.z < _arenaMinZ)
        {
            _movementState = movementStates.TO_PLAYER;
            StartCoroutine(TurnAwayFromWall());
        }
        else
        // move
        {
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
    }

    IEnumerator TurnAwayFromWall()
    {
        yield return new WaitForSeconds(0.5f);
        _movementState = movementStates.ROTATION;
    }

    private bool isChargeAttackSequenceStarted = false;
    IEnumerator StartChargeAttackSequence()
    {
        if(!isChargeAttackSequenceStarted)
        {
            isChargeAttackSequenceStarted = true;
            yield return new WaitForSeconds(Random.Range(2, 5));
            isChargeAttackSequenceStarted = false;
            StartCoroutine(ChargeTelegraph());
        }
    }

    IEnumerator ChargeTelegraph()
    {
        _movementState = movementStates.WAIT;
        _bossHealth.IsInvincible = true;
        _chargeHeading = _player.transform.position - transform.position;
        _chargeHeading.y = 0;
        _chargeHeading = Vector3.Normalize(_chargeHeading);
        // feedback
        yield return new WaitForSeconds(0.5f);
        _bossHealth.IsInvincible = false;
        _movementState = movementStates.CHARGE;
    }

    private void ChargeAttack()
    {
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(_chargeHeading), 0.5f));

        Vector3 move = _rb.position + (transform.forward * MoveSpeed * 5);
        if (move.x > _arenaMaxX || move.x < _arenaMinX)
        {
            StartCoroutine(EndChargeAttack());
        }
        else if (move.z > _arenaMaxZ || move.z < _arenaMinZ)
        {
            StartCoroutine(EndChargeAttack());
        }
        else
        {
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
    }

    IEnumerator EndChargeAttack()
    {
        _movementState = movementStates.WAIT;
        _bossHealth.IsInvincible = true;
        // feedback
        yield return new WaitForSeconds(2f);
        _bossHealth.IsInvincible = false;
        _movementState = movementStates.RAMPAGE;
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------

    private void MoveTowardsPlayer()
    {
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = 0; // ignore vertical
        heading = Vector3.Normalize(heading);
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
        _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
    }

    private void PillarTime()
    {
        Vector3 heading = _pillarSpot - transform.position;
        heading.y = transform.position.y; // ignore vertical
        if (heading.magnitude > _tolerance) // if close to target, just warp to the target
        {
            //heading = heading / heading.magnitude;
            heading = Vector3.Normalize(heading);
            _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
        else
        {
            _rb.MovePosition(_pillarSpot);
            _pillar.GetComponent<BossPillar>().ActivatePillar();
            _movementState = movementStates.WAIT;
        }
    }

    //this does not work and I can't figure it out
    private void FacePlayer()
    {
        _rb.constraints = RigidbodyConstraints.FreezePosition;
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = transform.position.y; // ignore vertical
        //heading = heading / heading.magnitude;
        heading = Vector3.Normalize(heading);
        Debug.Log(heading);
        //_rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
        //_rb.MovePosition(new Vector3(_pillarSpot.x, transform.position.y, _pillarSpot.z));
        transform.LookAt(_player.transform);
    }
}
