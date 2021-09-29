using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This whole script is kind of a mess. I still need to figure out a lot of the movement, but at least two of them are kind of working now
 */

public class Boss : Enemy
{
    public event System.Action<float, float> Crashed = delegate { };
    public event System.Action PillarDown = delegate { };
    [Header("Boss")]
    [SerializeField] private GameObject _player;
    [SerializeField] private Health _bossHealth;
    [Header("Standard Movement")]
    [SerializeField] private float _arenaMaxX;
    [SerializeField] private float _arenaMinX;
    [SerializeField] private float _arenaMaxZ;
    [SerializeField] private float _arenaMinZ;
    private int _rotationAngle = 80;

    private enum movementStates { RAMPAGE, STRAFE, CHARGE, PILLAR_MOVEMENT, PILLAR_ATTACK, WAIT};
    [SerializeField] private movementStates _movementState = movementStates.RAMPAGE;

    [SerializeField] private AudioClip _smallCrashSound = null;

    [Header("Charge Attack")]
    [SerializeField] private ParticleSystem _chargeExclamationParticles = null;
    [SerializeField] private AudioClip _chargeSound = null;
    [SerializeField] private ParticleSystem _crashParticles = null;
    [SerializeField] private AudioClip _crashSound = null;
    [SerializeField] private float _crashCameraShakeDuration = 0.5f;
    [SerializeField] private float _crashCameraShakeMagnitude = 1f;
    private Vector3 _chargeHeading;

    [Header("Mortar & Pillar")]
    [SerializeField] private Vector3 _pillarSpot;
    [SerializeField] private GameObject _pillar; // this is sloppy, but I'm running out of time and just want it to work
    private bool _isPillarUsed = false;
    private float _tolerance; // when boss is within this distance to pillar, warp to pillar, rather than move to it
    [SerializeField] private AudioClip _buildPillarSound = null;
    
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
        _tolerance = 3; // I know this shouldn't be hard coded, but I wasn't getting it to work consistently otherwise
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
                _movementState = movementStates.PILLAR_MOVEMENT;
                _isPillarUsed = true;
            }
            else
            {
                int coin = (int)Mathf.Round(Random.value);
                if (coin == 1)
                {
                    _movementState = movementStates.STRAFE;
                }
            }
        }
    }

    private void OnPillarInPosition(bool pillarAlive)
    {
        if(pillarAlive) // pillar has finished rising out of the ground, boss can start shooting
        {
            _rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _movementState = movementStates.PILLAR_ATTACK;
        }
        else // pillar has been destroyed, boss should return to standard movement pattern
        {
            _rb.constraints = RigidbodyConstraints.None;
            PillarDown.Invoke();
            StartCoroutine(DelayedScreenShake());
            StartCoroutine(EndChargeAttack()); // this is basically a stun for a couple of seconds, it's name should probably be changed
        }
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region State Machine
    private void FixedUpdate()
    {
        switch (_movementState)
        {
            case movementStates.RAMPAGE:
                RampageMovement();
                break;
            case movementStates.STRAFE:
                CircleStrafeMovement();
                break;
            case movementStates.CHARGE:
                ChargeAttack();
                break;
            case movementStates.PILLAR_MOVEMENT:
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
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region Rampage Movement
    /*
     *  Stages:
     *      - on starting this state, choose a random direction to start moving in
     *      - Move in the random direction
     *      - On hitting a wall, change direction   (this is currently using a defined set of coordinates to determine the boss's area of effect. 
     *            If I was doing this properly, I would try to do propper collision detection, but I don't have the time for that)
     */

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
            Feedback(null, _smallCrashSound, transform);
            Crashed.Invoke(0.1f, 0.1f);

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
            Feedback(null, _smallCrashSound, transform);
            Crashed.Invoke(0.1f, 0.1f);
            
            _chargeHeading.z = _chargeHeading.z * (-1);
            _chargeHeading = Quaternion.Euler(0, Random.Range(-30, 30), 0) * _chargeHeading;

            yield return new WaitForSeconds(1f);
            rotateZOnCooldown = false;
        }
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region Charge Attack
    /*
     *  Stages:
     *      - Boss circle strafes the player for some ammount of time (currently a random time between 2 and 4 seconds)
     *          - during circle strafing, if boss runs into wall, change directions
     *      - Stop moving briefly and telegraph attack
     *      - Charge at the player. Use player location at beginning of telegraph as charge target so that the player can use the telegraph as time to dodge
     *      - Keep moving past the player until running into the wall
     *      - On hitting the wall, become stunned for a couple of seconds
     *      - Return to standard movement state
     */

    private void CircleStrafeMovement()
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
        if (!isFlipRotationOnCooldown && (move.x > _arenaMaxX || move.x < _arenaMinX || move.z > _arenaMaxZ || move.z < _arenaMinZ))
        {
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
            Feedback(null, _smallCrashSound, transform);
            Crashed.Invoke(0.1f, 0.1f);
            isFlipRotationOnCooldown = true;
            _rotationAngle = (-1) * _rotationAngle;
            yield return new WaitForSeconds(0.5f);
            isFlipRotationOnCooldown = false;
        }
    }

    private bool isChargeAttackSequenceStarted = false;
    IEnumerator StartChargeAttackSequence()
    {
        if(!isChargeAttackSequenceStarted)
        {
            isChargeAttackSequenceStarted = true;
            yield return new WaitForSeconds(Random.Range(2, 4));    // use rotation movement for 2 to 4 seconds
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

        Feedback(_chargeExclamationParticles, _chargeSound, transform);

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
        Feedback(_crashParticles, _crashSound, transform);
        Crashed.Invoke(_crashCameraShakeDuration, _crashCameraShakeMagnitude);

        yield return new WaitForSeconds(2f);

        _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _movementState = movementStates.RAMPAGE;
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region Pillar Attack
    /*
     *  Stages:
     *      - Boss moves to designated pillar location (right now there's only one set location, I considered giving the boss more control over it, but I didn't have time for it)
     *      - Pillar rises out of the ground, lifting the boss
     *      - while on pillar, model rotates to face player, and crosshairs are spawned that follow the player
     *      - when the crosshair finds the player, it tells the boss to fire a mortar
     *      - When the pillar is destroyed, the boss falls to the ground and is briefly stunned
     *      - Boss returns to previous movement pattern
     */

    private void MoveToPillar()
    {
        Vector3 heading = _pillarSpot - transform.position;
        heading.y = transform.position.y; // ignore vertical
        if (heading.magnitude > _tolerance) // if far from target, just move to the target
        {
            heading = Vector3.Normalize(heading);
            _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, Quaternion.LookRotation(heading), 0.1f));
            _rb.MovePosition(_rb.position + (transform.forward * MoveSpeed));
        }
        else // when close enough, warp to it and stop using MoveToPillar
        {
            _rb.MovePosition(_pillarSpot);
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _movementState = movementStates.WAIT;
            _pillar.GetComponent<BossPillar>().ActivatePillar();  // bring the pillar out of the floor
            Feedback(null, _buildPillarSound, transform);
            Crashed.Invoke(0.5f, 1f);
        }
    }

    private void FacePlayer()
    {
        Vector3 heading = _player.transform.position - transform.position;
        heading.y = transform.position.y; // ignore vertical
        transform.LookAt(heading);
    }

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
        }
    }

    // called by the crosshair when it finds its target. This could probably be organized better, but I was running out of time
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

    // when the pillar is destroyed, screenshake should happen when the boss hits the ground. If I was doing this properly, I would probably want to use
    // actual collision detection to find when the boss is on the gound. But this works well enough that the player wouldn't notice
    IEnumerator DelayedScreenShake()
    {
        yield return new WaitForSeconds(0.3f);
        Crashed.Invoke(_crashCameraShakeDuration, _crashCameraShakeMagnitude);
    }
    #endregion
    // ----------------------------------------------------------------------------------------------------
    #region Helper Methods
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
}
