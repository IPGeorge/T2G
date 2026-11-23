using System;
using UnityEngine;


public class PlayerSwatController : MonoBehaviour
{
    private readonly string _animparam_Velocity = "Velocity";
    private readonly string _animparam_Strafe = "Strafe";

    [SerializeField] private float _WalkSpeed = 1.5f;
    [SerializeField] private float _RunSpeed = 3.2f;
    [SerializeField] private float _StrafeSpeed = 3.0f;
    [SerializeField] private float _RotationSpeed = 5.0f;
    [SerializeField] private string _spineBoneName = "Spine1";

    private Rigidbody _rigidBody;
    private Animator _animator;
    private Transform _spineBone;

    public float sensitivity = 2.0f;
    public float clampAngle = 30f;
    private float rotX, rotY;

    [HideInInspector] public bool IsRunning = true;

    private RifleController _rifle = null;

    int _ammo;
    int _score;
    public Action<int, int> OnAmmoScoreChanged;

    public void Reset()
    {
        _ammo = 20;
        _score = 0;
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _spineBone = null;
        var transforms = transform.GetComponentsInChildren<Transform>();
        foreach(var t in transforms)
        {
            if(t.name.IndexOf(_spineBoneName) >= 0)
            {
                _spineBone = t;
                break;
            }
        }

        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
    }

    private void Update()
    {
        Vector2 axisInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (axisInputs == Vector2.zero)
        {
            _animator.SetFloat(_animparam_Velocity, 0.0f);
        }
        else
        {
            MovePlayer(axisInputs);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsRunning = !IsRunning;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(_rifle == null)
            {
                _rifle = GetComponentInChildren<RifleController>();
                _rifle.OnHitTarget = () => 
                {
                    _score++;
                    OnAmmoScoreChanged?.Invoke(_ammo, _score);
                };
            }

            _rifle?.Fire();
            _ammo--;
            OnAmmoScoreChanged?.Invoke(_ammo, _score);
        }
    }

    public void MovePlayer(in Vector2 axisMove)
    {
        Vector3 trans = axisMove.y * _rigidBody.transform.forward + axisMove.x * _rigidBody.transform.right;
        Vector3 vel = trans.normalized;
        if (axisMove.y == 0.0f)      //Strafe
        {
            _rigidBody.linearVelocity = new Vector3(vel.x * _StrafeSpeed, _rigidBody.linearVelocity.y, vel.z * _StrafeSpeed);
            _animator.SetFloat(_animparam_Velocity, 0.0f);
            _animator.SetFloat(_animparam_Strafe, axisMove.x);
        }
        else
        {
            float speed = IsRunning ? _RunSpeed : _WalkSpeed;
            _rigidBody.linearVelocity = new Vector3(vel.x * speed, _rigidBody.linearVelocity.y, vel.z * speed);
            if (axisMove.x != 0.0f)
            {
                _rigidBody.transform.rotation = Quaternion.Lerp(_rigidBody.transform.rotation,
                    Quaternion.LookRotation(trans),
                    Time.deltaTime * _RotationSpeed);
            }
            _animator.SetFloat(_animparam_Velocity, _rigidBody.linearVelocity.magnitude * Mathf.Sign(axisMove.y));
            _animator.SetFloat(_animparam_Strafe, 0.0f);
        }
    }

    public void LateUpdate()
    {
        if (_spineBone != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            rotX += mouseY;
            rotY += mouseX;
            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
            rotY = Mathf.Clamp(rotY, -10, 80);

            _spineBone.rotation *= Quaternion.Euler(rotX, rotY, 0.0f);
        }
    }
}
