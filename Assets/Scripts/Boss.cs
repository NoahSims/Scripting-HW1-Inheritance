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

    private enum movementStates { RAMPAGE, ROTATION, CHARGE, TO_PLAYER, PILLAR, PILLAR_ATTACK, WAIT};
    [SerializeField] private movementStates _movementState = movementStates.ROTATION;

    private Vector3 _chargeHeading;

    [SerializeField] private Vector3 _pillarSpot;
    [SerializeField] private GameObject _pillar; // this is sloppy, but I'm running out of time and just want it to work
    private bool _isPillarUsed = false;
    private float _tolerance; // when boss is within this distance to pillar, warp to pillar, rather than move to it

    [Header("Mortar Stuff")]
    [SerializeField] private GameObject _bossCrosshair = null;
    [SerializeField] private float _crosshairSpeed = 0.15f;
    [SerializeField] private GameObject _mortarProjectile = null;
    [SerializeField] private float _mortarSpeed = 1f;
    [SerializeField] private float _mortarShotDelay = 0.5f;
    [SerializeField] private ParticleSystem _mortarSmokeParticles = null;
    [SerializeField] private AudioClip _mortarShotSound = null;
    [SerializeField] private GameObject _mortarSpawnL;
    [SerializeField] private GameObject _mortarSpawnR;

    // ----------------------------------------------------------------------------------------------------
    #region Unity Events
    private void Start()
    {
        _tolerance = 3;
        _chargeHeading = getRandomHeading();
        _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        _bossHealth.Damaged += OnBossDamaged;
        _pillar.GetComponent<BossPillar>().PillarInPosition += OnPillarInPosition;
    }

    private void OnDisable()
    {
        _bossHealth.Damaged -= OnBossDamaged;
        if(_pillar != null)
            _pillar.GetComponent<BossPillar>().PillarInPosition -= OnPillarInPosition;
    }

    private void OnBossDamaged(int ammount)
    {
        if (_movementState == movementStates.RAMPAGE)
        {
            if (!_isPillarUsed && _bossHealth.CurrentHealth <= _bossHealth.MaxHealth / 2)
            {
                _movementState = movementStates.PILLAR;
                _isPillarUsed = true;
            }
            else
            {
                int coin = (int)Mathf.Round(Random.value);
                if (coin == 1)
                {
                    _movementState = movementStates.ROTATION;
                }
            }
        }
    }

    private void OnPillarInPosition(bool pillarAlive)
    {
        if(pillarAlive)
        {
            _rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _movementState = movementStates.PILLAR_ATTACK;
        }
        else
        {
            _rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(EndChargeAttack()); // this is basically a stun for a couple of seconds, it's name should probably be changed
        }
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------

    private void FixedUpdate()
    {
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
                MoveToPillar();
                break;
            case movementStates.PILLAR_ATTACK:
                FacePlayer();
                MortarFireSequence();
                break;
            case movementStates.WAIT:
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
    #region Charge Attack
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
            //_movementState = movementStates.TO_PLAYER;
            //StartCoroutine(TurnAwayFromWall());
            StartCoroutine(FlipRotationCooldown());
        }
        else
        // move
        {
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
    }

    private bool isFlipRotationOnCooldown = false;
    IEnumerator FlipRotationCooldown()
    {
        if(!isFlipRotationOnCooldown)
        {
            isFlipRotationOnCooldown = true;
            _rotationAngle = (-1) * _rotationAngle;
            yield return new WaitForSeconds(0.5f);
            isFlipRotationOnCooldown = false;
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
    #region Pillar Attack
    private void MoveToPillar()
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
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _movementState = movementStates.WAIT;
            _pillar.GetComponent<BossPillar>().ActivatePillar();
        }
    }

    private void FacePlayer()
    {
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = transform.position.y; // ignore vertical
        //heading = heading / heading.magnitude;
        //heading = Vector3.Normalize(heading);
        //Debug.Log(heading);
        //_rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
        //_rb.MovePosition(new Vector3(_pillarSpot.x, transform.position.y, _pillarSpot.z));
        transform.LookAt(heading);
    }

    private int mortarCounter = 0;
    private float timeLastFired = 0f;
    private void MortarFireSequence()
    {
        if(Time.time - timeLastFired > 1f)
        {
            timeLastFired = Time.time;

            Vector3 crossHairPos = _pillar.transform.position;
            crossHairPos.y = 0.01f;
            GameObject newCrosshair = Instantiate(_bossCrosshair, crossHairPos, Quaternion.identity);
            newCrosshair.GetComponent<BossCrosshair>().SetValues(_crosshairSpeed, _player, gameObject);

            mortarCounter++;
        }
    }

    public void TargetAquired(GameObject crossHair)
    {
        StartCoroutine(ShootMortar(crossHair));
    }

    IEnumerator ShootMortar(GameObject crossHair)
    {
        // "Shoot" into the air
        Feedback(_mortarSmokeParticles, _mortarShotSound, _mortarSpawnL.transform);
        Feedback(_mortarSmokeParticles, null, _mortarSpawnR.transform);

        // wait
        yield return new WaitForSeconds(_mortarShotDelay);

        // spawn mortar falling straight down
        Vector3 pos = crossHair.transform.position;
        pos += Vector3.up * 20;
        GameObject newMortarProjectile = Instantiate(_mortarProjectile, pos, Quaternion.identity);
        newMortarProjectile.GetComponent<MortarProjectile>().SetValues(_mortarSpeed, crossHair, gameObject);
    }

    private void Feedback(ParticleSystem particles, AudioClip audio, Transform transform)
    {
        // particles
        if (particles != null)
        {
            ParticleSystem _particles = Instantiate(particles, transform.position, transform.rotation);
            _particles.Play();
        }
        // audio. TODO - consider Object Pooling for performance
        if (audio != null)
        {
            AudioHelper.PlayClip2D(audio, 1f);
        }
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region No Longer Used
    private void MoveTowardsPlayer()
    {
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = 0; // ignore vertical
        heading = Vector3.Normalize(heading);
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
        _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
    }

    #endregion
    // ----------------------------------------------------------------------------------------------------
}
