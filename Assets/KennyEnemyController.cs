using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Framework;

public class KennyEnemyController : MonoBehaviour
{
    // Components
    protected MovementComponent _MovementComponent;
    protected AnimationComponent _AnimationComponent;
    protected KennyKinematicComponent _KennyKinematicComponent;
    [SerializeField] protected Animator _Animator;

    // Variables
    [SerializeField] protected Transform _target;
    [SerializeField] protected Transform _lookAtTransform;
    protected Vector2 _lookVector2 = Vector2.up;
    protected Vector2 _lookVector2Lerped = Vector2.up;
    protected Vector2 _moveVector = Vector2.zero;
    protected Vector2 _directionVector = Vector2.zero;
    [SerializeField] protected Transform _attackSpawn;

    private Vector3 _currentTargetPosition = Vector3.zero;
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private float _shuffleSpeed = 8f;
    [SerializeField] private float _animationTransitionSpeed = 8f;
    [SerializeField] private float _idleRotation = 45f;
    private float _idleRotationOverride = 0f;
    private float lastAttackTime = Mathf.NegativeInfinity; // Seconds
    private int lastAttackValue = -1;
    private bool _attacking = false;

    public bool moveTowardsTarget = true;
    public float moveTowardsAcceptanceRange = 3f;
    public float attackSpeed = 3f;
    public float attackRange = 4.5f;
    public float attackAttemptRange = 7.5f;

    private Dictionary<Vector2, float> _moveVectorMap = new Dictionary<Vector2, float>()
    {
        // Run Fowards
        [new Vector2(-1, 1)] = -45f, // Forwards-Left
        [new Vector2(0, 1)] = 0f, // Forwards
        [new Vector2(1, 1)] = 45f, // Forwards-Right
        [new Vector2(1, 0)] = 90f, // Right

        // Run Backwards
        [new Vector2(1, -1)] = -45f, // Backwards-Right
        [new Vector2(0, -1)] = 0f, // Backwards
        [new Vector2(-1, -1)] = 45f, // Backwards-Left
        [new Vector2(-1, 0)] = 90f, // Left
    };

    private readonly HashSet<Vector2> _moveVectorForwardDirections = new HashSet<Vector2>
    {
        new Vector2(-1, 1), // Forwards-Left
        new Vector2(0, 1) , // Forwards
        new Vector2(1, 1) , // Forwards-Right
        new Vector2(1, 0) , // Right
    };

    private Dictionary<string, float> _animationWeights = new Dictionary<string, float>()
    {
        ["WholeBody"] = 0,
        ["IK"] = 1,
    };

    private Vector2 _directionEulerAngles = Vector3.zero;

    protected void Awake()
    {
        TryGetComponent<MovementComponent>(out _MovementComponent);
        TryGetComponent<KennyKinematicComponent>(out _KennyKinematicComponent);

        if (_attackSpawn == null)
        {
            _attackSpawn = new GameObject("AttackSpawn").transform;
            _attackSpawn.SetParent(this.transform);
            _attackSpawn.localPosition = new Vector3(0, 1, 0);
        }
    }

    protected void Start()
    {
        SetTarget(GameObject.FindGameObjectWithTag("Player"));
    }

    void FixedUpdate()
    {
        if (moveTowardsTarget)
        {
            if ((transform.position - _target.position).sqrMagnitude > moveTowardsAcceptanceRange && _attacking == false)
            {
                Move(new Vector2(_lookVector2Lerped.x, _lookVector2Lerped.y));
            }
            else
            {
                Move(Vector2.zero);
            }
        }
    }

    protected void Update()
    {
        if (_target == null) return;

        _currentTargetPosition = _target.position;

        Vector3 worldTargetPosition = new Vector3(_currentTargetPosition.x, 1, _currentTargetPosition.z);
        Vector3 lookVector3 = worldTargetPosition - new Vector3(transform.position.x, 1, transform.position.z);
        _lookVector2 = new Vector2(lookVector3.x, lookVector3.z);
        _lookVector2Lerped = Vector2.Lerp(_lookVector2Lerped, _lookVector2, Time.deltaTime * _shuffleSpeed);

        if (!((transform.position - _target.position).sqrMagnitude > moveTowardsAcceptanceRange))
        {
            TryAttack();
        }

        _Animator.SetLayerWeight(2, Mathf.Lerp(_Animator.GetLayerWeight(2), _animationWeights["WholeBody"], Time.deltaTime * _animationTransitionSpeed));
        _KennyKinematicComponent.LerpRigConstraintWeight(_animationWeights["IK"], Time.deltaTime * _animationTransitionSpeed);

        Quaternion characterRotation = Quaternion.LookRotation(lookVector3.normalized);

        if (_moveVector.sqrMagnitude > 0.001f)
        {
            Vector3 directionVector3 = new Vector3(_directionVector.x, 0f, _directionVector.y);

            // Relative to character
            Vector3 directionLocal3 = Quaternion.Inverse(characterRotation) * directionVector3;

            // Snap to 8 directions
            Vector2 direction8Axis = new Vector2
            (
                Mathf.Abs(directionLocal3.x) < 0.5f ? 0f : Mathf.Sign(directionLocal3.x),
                Mathf.Abs(directionLocal3.z) < 0.5f ? 0f : Mathf.Sign(directionLocal3.z)
            );

            Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), directionVector3.normalized * 2, Color.red);

            if (direction8Axis != Vector2.zero && _moveVectorMap.TryGetValue(direction8Axis, out float offsetYaw))
            {
                if (_moveVectorForwardDirections.Contains(direction8Axis))
                {
                    _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), 1, Time.deltaTime * _animationTransitionSpeed));
                }
                else
                {
                    _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), -1, Time.deltaTime * _animationTransitionSpeed));
                }
                _KennyKinematicComponent.LerpRootRotation(new Vector3(0, offsetYaw + characterRotation.eulerAngles.y, 0), Time.deltaTime * _rotationSpeed);
            }
        }
        else
        {
            _KennyKinematicComponent.LerpRootRotation(new Vector3(0, characterRotation.eulerAngles.y + _idleRotation + _idleRotationOverride, 0), Time.deltaTime * _rotationSpeed);
            _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), 0, Time.deltaTime * _animationTransitionSpeed));
        }

        if (lookVector3.magnitude < 2)
        {
            _lookAtTransform.position = new Vector3(transform.position.x, 1, transform.position.z) + (lookVector3.normalized * 2);
        }
        else
        {
            _lookAtTransform.position = worldTargetPosition;
        }
    }

    void Move(Vector2 setMoveVector)
    {
        _moveVector = setMoveVector.normalized;
        _directionVector = new Vector2
        (
            Mathf.Abs(_moveVector.x) < 0.5f ? 0f : Mathf.Sign(_moveVector.x),
            Mathf.Abs(_moveVector.y) < 0.5f ? 0f : Mathf.Sign(_moveVector.y)
        );

        _MovementComponent.SetMoveVector(_moveVector);
    }

    protected virtual async UniTask TryAttack()
    {
        Debug.Log("Attack Attempted");
        if (Time.time - lastAttackTime < attackSpeed) return;

        Debug.Log("Swinging");
        lastAttackTime = Time.time;
        int attackValue = UnityEngine.Random.Range(0, 4);
        while (attackValue == lastAttackValue)
        {
            attackValue = UnityEngine.Random.Range(0, 4);
            await UniTask.Yield();
        }
        lastAttackValue = attackValue;
        _Animator.SetFloat("AttackValue", attackValue);
        _Animator.SetTrigger("Attack");
        _animationWeights["WholeBody"] = 1;
        _animationWeights["IK"] = 0;
        _idleRotationOverride = -_idleRotation;
        _attacking = true;
        await UniTask.Delay(1000);
        _attacking = false;
        _animationWeights["WholeBody"] = 0;
        _animationWeights["IK"] = 1;
        _idleRotationOverride = 0;
        // Melee attack, sphere cast
    }

    public void SetTarget(GameObject gameObject)
    {
        if (gameObject != null)
        {
            _target = gameObject.transform;
        }
    }
}
