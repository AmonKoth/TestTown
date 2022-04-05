using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private float _maxHealth = 100.0f;

    [Header("Combat")]
    [SerializeField]
    private float _seekRange = 10f;
    [SerializeField]
    private float _fireRate = 0.25f;
    [SerializeField]
    private float _bulletSpeed = 4000f;
    //Prefab of the Bullet
    [SerializeField]
    private Bullet _bullet;
    //Bullet Spawn Location
    [SerializeField]
    private GameObject _bulletSpawn;
    [SerializeField]
    private float _damageForgetTime = 4f;

    [Header("Movement")]
    [SerializeField]
    [Range(0, 100)]
    private float _wanderChance = 10f;
    [SerializeField]
    private float _maxWanderRange = 50f;
    [SerializeField]
    private float _maxAttackRange = 3f;

    //Components
    private NavMeshAgent _agent;

    //Used to make AI switch states, they will be defined below
    enum STATE { IDLE, WANDER, FOLLOW, ATTACK };
    STATE _state = STATE.WANDER;

    private PlayerController _target;
    private bool _canShoot = true;
    private float _stopDistance;
    private bool _isRecentlyAttacked = false;

    //Runtime Values
    private float _health;

    //Calculates the distancs to player returns infinity if the player is dead.
    private float DistanceToPLayer()
    {
        if (_target.GetPlayerDead())
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(_target.transform.position, this.transform.position);
    }

    bool IsPlayerNear()
    {
        if (DistanceToPLayer() < _seekRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool IsPlayerTooFar()
    {
        if (DistanceToPLayer() > _seekRange * 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Needs projectile
    private void Fire()
    {
        if (_canShoot == false)
        {
            return;
        }
        var bulletObject = Instantiate(_bullet, _bulletSpawn.transform.position, this.transform.rotation);
        bulletObject.GetComponent<Rigidbody>().AddForce(this.transform.forward * _bulletSpeed);
        //Locking the canshoot so there is a drawback time
        _canShoot = false;
        Invoke("EnableShoot", _fireRate);
    }
    //For adding a fireRate
    private void EnableShoot()
    {
        _canShoot = true;
    }
    //Called in PlayerController
    public void RecieveDamage(float dmg)
    {
        _health -= dmg;
        _isRecentlyAttacked = true;
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }
    private bool ForgetbeingHurt() => _isRecentlyAttacked = false;

    private void IdleState()
    {
        if (IsPlayerNear())
        {
            _state = STATE.FOLLOW;
        }
        else if (Random.Range(0, 1000) < _wanderChance)
        {
            _state = STATE.WANDER;
        }
    }

    private void WanderState()
    {
        if (_agent.hasPath == false)
        {   //Creates a random destination point relative to agents position
            float wanderX = this.transform.position.x + Random.Range(-_maxWanderRange, _maxWanderRange + 1);
            float wanderZ = this.transform.position.z + Random.Range(-_maxWanderRange, _maxWanderRange + 1);
            //Checks if there is a valid path if so moves the agent to there
            NavMeshHit hit;
            Vector3 randomTarget = new Vector3(wanderX, 0, wanderZ);
            if (NavMesh.SamplePosition(randomTarget, out hit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 target = randomTarget;
                _agent.SetDestination(target);
                _agent.stoppingDistance = 0;
            }
        }

        //If it detects the player
        if (IsPlayerNear() || _isRecentlyAttacked)
        {
            _state = STATE.FOLLOW;
            if (_isRecentlyAttacked)
            {
                Invoke("ForgetbeingHurt", _damageForgetTime);
            }
        }
        //Posibility of going into idle
        else if (Random.Range(0, 10000) < _wanderChance)
        {
            _state = STATE.IDLE;
            _agent.ResetPath();
        }
    }

    private void FollowState()
    {
        //If the player is dead ignore the rest
        if (_target.GetPlayerDead())
        {
            _state = STATE.IDLE;
            return;
        }
        _agent.SetDestination(_target.transform.position);
        _agent.stoppingDistance = _stopDistance;
        //If the player is near
        if (_agent.remainingDistance <= _agent.stoppingDistance && _agent.pathPending == false)
        {
            _state = STATE.ATTACK;
        }
        //For forgeting the player
        if (IsPlayerTooFar())
        {
            _state = STATE.WANDER;
            _agent.ResetPath();
        }
    }

    private void AttackState()
    {
        if (_target.GetPlayerDead())
        {
            _state = STATE.IDLE;
        }
        this.transform.LookAt(_target.transform.position, Vector3.up);
        Fire();
        if (DistanceToPLayer() < _agent.stoppingDistance + _maxAttackRange)
        {
            _state = STATE.FOLLOW;
        }
    }

    private void HandleMovement()
    {
        switch (_state)
        {
            case STATE.IDLE:
                IdleState();
                break;
            case STATE.WANDER:
                WanderState();
                break;
            case STATE.FOLLOW:
                FollowState();
                break;
            case STATE.ATTACK:
                AttackState();
                break;
            default: break;
        }
    }
    //To Initilize Enemy
    private void Initilize()
    {
        _health = _maxHealth;
        _agent = this.GetComponent<NavMeshAgent>();
        _target = FindObjectOfType<PlayerController>();
        _stopDistance = _agent.stoppingDistance;
    }

    void Start()
    {
        Initilize();
    }

    private void Update()
    {
        HandleMovement();
    }
}
