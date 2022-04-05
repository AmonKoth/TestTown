using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Serialized Properties
    [Header("Movement")]
    [SerializeField]
    private float _maxForwardSpeed = 10.0f;
    [SerializeField]
    private float _runningRotateSpeed = 100.0f;
    //REMOVED
    // [SerializeField]
    // private float _standingRotateSpeed = 10.0f;
    [SerializeField]
    private float _jumpSpeed = 3000f;
    //For checking if there is a land below
    [SerializeField]
    private float _groundRayDistance = 1f;
    //Link the spine of the charecter to move it from waist up
    [SerializeField]
    private Transform _spine;

    [Header("Mouse Movement")]
    [SerializeField]
    private float _xSensitivity = 0.5f;
    [SerializeField]
    private float _ySensitivity = 0.5f;
    //Look dir limits, in angles
    [SerializeField]
    private float _xAngleLimit = 45.0f;
    [SerializeField]
    private float _yAngleLimit = 45.0f;

    [Header("Combat")]
    //For adjusting the laser dot distance
    [SerializeField]
    [Range(0.1f, 1.0f)]
    private float _laserPointDistance;
    [SerializeField]
    private float _fireRate;
    [SerializeField]
    private float _maxDamage = 10.0f;
    [SerializeField]
    private float _maxHealth = 100.0f;

    //Needed Componets
    private Animator _anim;
    private Rigidbody _rb;
    private ParticleSystem _particleSystem;

    //Input Related
    //movement
    private Vector2 _moveDir;
    //Jumping
    private float _jumpDir;
    private float _jumpEffort;
    //look
    private Vector2 _lookDir;
    //Rotate
    private float _rotateDir;

    //Maximum amount of speed
    private float _desiredSpeed;
    //Current Speed
    private float _forwardSpeed;

    const float groundAccel = 5.0f;
    const float groundDecel = 25f;

    //Variables needed for jumping
    private bool _isReadyJump = false;
    private bool _onGround = true;

    //Needed for Mouse rotation
    private Vector2 _lastLookDir;

    //For SpeedUp collectable 
    private bool _isSpeedLock = true;
    private bool _isParticlelock = true;

    //For weapon
    private Weapon _rifle;
    //For Laser Sight
    private LineRenderer _laser;
    private Light _laserDot;
    private bool _firing = false;

    //COMBAT
    private float _health;
    private float _damage;
    //am I dead?
    //Will use later with death animation, after when ai can dmg and chase the player
    private bool _isPlayerDead = false;

    private bool _isPaused = false;

    //Getters and Setters
    //Not used yet will add later on
    public void SwitchSpeedLock() { _isSpeedLock = !_isSpeedLock; }
    public bool GetSpeedLock() => _isSpeedLock;

    public void SwitchPartcileLock() { _isParticlelock = !_isParticlelock; }
    public bool GetParticleLock() => _isParticlelock;

    public bool GetPlayerDead() => _isPlayerDead;

    public bool GetIsPaused() => _isPaused;

    public void SetIsPause(bool pause) { _isPaused = pause; }

    ///Used by InputSystem in Editor
    //Check if there is any input
    bool isMoveInput
    {
        get { return !Mathf.Approximately(_moveDir.sqrMagnitude, 0f); }
    }
    //Movement with Input system
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDir = context.ReadValue<Vector2>();
    }
    //Looking with Input system
    public void OnLook(InputAction.CallbackContext context)
    //Jumping with Input system
    {
        _lookDir = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpDir = context.ReadValue<float>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        //Bug in here calls the firing 2 times when pressed one time when released. Will fix later
        if ((double)context.ReadValue<float>() == 1 && _anim.GetBool("Equiped") && _firing == false)
        {
            _firing = true;
            _anim.SetTrigger("Fire");
        }
    }
    public void OnEquip(InputAction.CallbackContext context)
    {
        _anim.SetBool("Equiped", !_anim.GetBool("Equiped"));
    }

    //Handles Movement
    void Move(Vector2 dir)
    {
        //Gets the directional inputs
        float fDirection = dir.y;
        float rotateAmount = dir.x;

        //to normalize
        if (dir.sqrMagnitude > 1f)
        {
            dir.Normalize();
        }
        ///Will implament later on with speedup powerup
        if (_isSpeedLock)
        {
            _maxForwardSpeed = 6.0f;
        }

        //The maximum forward speed value
        _desiredSpeed = dir.magnitude * _maxForwardSpeed * Mathf.Sign(fDirection);

        //Handles the acceleration and checks if there is input
        float acceleration = isMoveInput ? groundAccel : groundDecel;
        //Slowly increasing the speed
        _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredSpeed, acceleration * Time.deltaTime);
        //Sets the point in the animation max 10 min -10, actual movement speed is determined by the animation speed.
        _anim.SetFloat("ForwardSpeed", _forwardSpeed);

        transform.Rotate(0.0f, rotateAmount * _runningRotateSpeed * Time.deltaTime, 0.0f);
    }

    void Jump(float dir)
    {
        if (dir > 0 && _onGround)
        {
            _anim.SetBool("ReadyJump", true);
            //Increases as the spacebar is held
            _jumpEffort += Time.deltaTime;
            _isReadyJump = true;
        }
        else if (_isReadyJump)
        {
            _anim.SetBool("Launch", true);
            _isReadyJump = false;
            _anim.SetBool("ReadyJump", false);
        }
    }

    //This function is called in jump animation as an event
    public void Launch()
    {
        _rb.AddForce(0, _jumpSpeed * Mathf.Clamp(_jumpEffort, 1, 3), 0);
        _rb.AddForce(this.transform.forward * _forwardSpeed * 100);
        _anim.SetBool("Launch", false);
        _anim.SetBool("Land", false);
        //Stopped the root motion so the jump animation won't effect the actual jumping
        _anim.applyRootMotion = false;
    }

    //This function is called in landing animation as an event
    public void Land()
    {
        _anim.SetBool("Land", false);
        _anim.SetBool("Launch", false);
        _jumpEffort = 1;
        _anim.applyRootMotion = true;
    }
    //To check if there is a land above an trigger the landing animation.
    private void HandleLanding()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * _groundRayDistance * 0.5f, -Vector3.up);
        //Debug.DrawRay(transform.position + Vector3.up * _groundRayDistance * 0.5f, -Vector3.up, Color.red);
        if (Physics.Raycast(ray, out hit, _groundRayDistance))
        {
            if (_onGround == false)
            {
                _onGround = true;
                _anim.SetFloat("LandingVelocity", _rb.velocity.magnitude);
                _anim.SetBool("Land", true);
                _anim.SetBool("Falling", false);
            }
        }
        else
        {
            _onGround = false;
            _anim.SetBool("Falling", true);
            _anim.applyRootMotion = false;
        }
    }

    private void HandleLaser()
    {
        if (_anim.GetBool("Equiped"))
        {
            RaycastHit laserHit;
            Ray laserRay = new Ray(_laser.transform.position, _laser.transform.forward);
            //If this is null will set the laser to default value
            if (Physics.Raycast(laserRay, out laserHit))
            {
                if (_anim.GetBool("Fire") == false)
                {
                    _firing = false;
                }
                //Gets the hit location and sets the length of the laser to that
                _laser.SetPosition(1, _laser.transform.InverseTransformPoint(laserHit.point));
                _laserDot.transform.localPosition = new Vector3(0, 0, _laser.GetPosition(1).z * _laserPointDistance);

                if (_firing && laserHit.collider.gameObject.tag == "Enemy")
                {
                    laserHit.collider.GetComponent<EnemyAI>().RecieveDamage(_damage);
                    Invoke("CanFire", _fireRate);
                }
            }
        }
    }

    //for firerate
    private void CanFire()
    {
        _firing = false;
    }

    //For mouse looking
    private void HandleMouseLook()
    {
        if (_isPaused == false)
        {        //Storing the last look dir so that the animation won't cancel over it
                 //lookdir y is inverted so that the mouse control is propper
                 //Lookdir outputs are switched according to the rotation
            _lastLookDir += new Vector2(-_lookDir.y * _ySensitivity, _lookDir.x * _xSensitivity);
            //Clamping the x and y values to limit the motion of spine 
            _lastLookDir.x = Mathf.Clamp(_lastLookDir.x, -_xAngleLimit, _xAngleLimit);
            _lastLookDir.y = Mathf.Clamp(_lastLookDir.y, -_yAngleLimit, _yAngleLimit);

            //Since we are using angles we need to use eulerAngles instead of Rotate
            _spine.localEulerAngles = _lastLookDir;
        }
    }

    public void ParticleHandle()
    {
        var emission = _particleSystem.emission;
        if (!_isParticlelock)
        {
            //Add switch back to here from speedup
            emission.enabled = true;
        }
        else
        {
            emission.enabled = false;
        }
    }

    public void WeaponEquip()
    {
        _rifle.gameObject.SetActive(true);
    }
    public void WeaponUnequip()
    {
        _rifle.gameObject.SetActive(false);
    }
    public void RecieveDamage(float dmg)
    {
        _health -= dmg;
        _anim.SetTrigger("Hit");
        if (_health <= 0)
        {
            _isPlayerDead = true;
            _anim.SetLayerWeight(1, 0);
            _anim.SetBool("Death", true);
        }
    }

    //Initilizer
    private void InitilizePlayer()
    {
        //Components
        _anim = this.GetComponent<Animator>();
        _rb = this.GetComponent<Rigidbody>();
        _particleSystem = this.GetComponent<ParticleSystem>();

        _health = _maxHealth;
        _damage = _maxDamage;

        //Weapon
        _rifle = this.GetComponentInChildren<Weapon>();
        _laser = _rifle.GetComponentInChildren<LineRenderer>();
        _laserDot = _laser.GetComponentInChildren<Light>();
        _rifle.gameObject.SetActive(false);
    }

    void Start()
    {
        InitilizePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlayerDead == false)
        {

            Move(_moveDir);
            Jump(_jumpDir);
            HandleLanding();
            ParticleHandle();
            HandleLaser();
        }
    }

    //For Rotating with the mouse, in order for it to be not affected by animation
    private void LateUpdate()
    {
        if (_isPlayerDead == false)
        {
            HandleMouseLook();
        }
    }

}