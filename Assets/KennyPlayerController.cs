using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Framework;

public class KennyPlayerController : MonoBehaviour
{
    // Services
    private InputService InputService;

    // Components
    protected CameraComponent _CameraComponent;
    protected MovementComponent _MovementComponent;
    protected WeaponComponent _WeaponComponent;
    protected AnimationComponent _AnimationComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;
    protected KennyKinematicComponent _KennyKinematicComponent;
    [SerializeField] protected Animator _Animator;

    // Variables
    [SerializeField] protected Transform _lookAtTransform;
    protected Vector2 _lookVector2 = Vector2.up;
    protected Vector2 _moveVector = Vector2.zero;
    protected Vector2 _directionVector = Vector2.zero;
    protected Vector2 _lookVector2Weapon = Vector2.up;
    [SerializeField] protected Transform _attackSpawn;

    private Vector2 _currentMousePosition = Vector2.zero;
    private Vector2 _lastMovementInput = Vector2.zero;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float animationTransitionSpeed = 8f;

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

    private Vector2 _directionEulerAngles = Vector3.zero;

    protected void Awake()
    {
        InputService = Game.GetService<InputService>();

        TryGetComponent<CameraComponent>(out _CameraComponent);
        TryGetComponent<MovementComponent>(out _MovementComponent);
        TryGetComponent<ScreenToPlaneComponent>(out _ScreenToPlaneComponent);
        TryGetComponent<KennyKinematicComponent>(out _KennyKinematicComponent);
        TryGetComponent<WeaponComponent>(out _WeaponComponent);
        //TryGetComponent<AnimationComponent>(out _AnimationComponent);

        _ScreenToPlaneComponent.SetMask(LayerMask.GetMask("Mouse"));

        InputService.Connect("Move", "Gameplay", Move);
        InputService.Connect("Fire", "Gameplay", Attack);
        InputService.Connect("FireSecondary", "Gameplay", AttackSecondary);
        InputService.Connect("Look", "Gameplay", LookMouse);
        InputService.Connect("Reload", "Gameplay", Reload);

        if (_attackSpawn == null)
        {
            _attackSpawn = new GameObject("AttackSpawn").transform;
            _attackSpawn.SetParent(this.transform);
            _attackSpawn.localPosition = new Vector3(0, 1, 0);
        }
        _WeaponComponent.SetAttackSpawn(_attackSpawn);
    }

    protected void Update()
    {
        Vector3 worldMousePosition = _ScreenToPlaneComponent.ScreenToPlane(_currentMousePosition);
        Vector3 lookVector3 = worldMousePosition - new Vector3(transform.position.x, worldMousePosition.y, transform.position.z);
        _lookVector2 = new Vector2(lookVector3.x, lookVector3.z);
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
                    _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), 1, Time.deltaTime * animationTransitionSpeed));
                }
                else
                {
                    _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), -1, Time.deltaTime * animationTransitionSpeed));
                }
                _KennyKinematicComponent.LerpRootRotation(new Vector3(0, offsetYaw + characterRotation.eulerAngles.y, 0), Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            _KennyKinematicComponent.LerpRootRotation(new Vector3(0, characterRotation.eulerAngles.y + 60, 0), Time.deltaTime * rotationSpeed);
            _Animator.SetFloat("MoveZ", Mathf.Lerp(_Animator.GetFloat("MoveZ"), 0, Time.deltaTime * animationTransitionSpeed));
        }

        if (lookVector3.magnitude < 2)
        {
            _lookAtTransform.position = new Vector3(transform.position.x, worldMousePosition.y, transform.position.z) + (lookVector3.normalized * 2);
        }
        else
        {
            _lookAtTransform.position = worldMousePosition;
        }

        _CameraComponent.PanTowards(worldMousePosition);
        _WeaponComponent.SetAttackDirecton(_lookVector2);
    }

    void Move(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
        _directionVector = new Vector2
        (
            Mathf.Approximately(_moveVector.x, 0f) ? 0f : Mathf.Sign(_moveVector.x),
            Mathf.Approximately(_moveVector.y, 0f) ? 0f : Mathf.Sign(_moveVector.y)
        );

        _MovementComponent.SetMoveVector(_moveVector);
    }

    void LookMouse(InputAction.CallbackContext context)
    {
        _currentMousePosition = context.ReadValue<Vector2>();
    }

    void Attack(InputAction.CallbackContext context)
    {
       if (context.performed)
        {
            _WeaponComponent.AttackStarted(true);
        }
        if (context.canceled)
        {
            _WeaponComponent.AttackCancelled(true);
        }
    }

    void Reload(InputAction.CallbackContext context)
    {
        // No reloading
    }

    void AttackSecondary(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _WeaponComponent.AttackStarted(false);
        }
        if (context.canceled)
        {
            _WeaponComponent.AttackCancelled(false);
        }
    }
}
