using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// Colors :
// Red : FF4E32
// "White" : CCDDEE
// "Gray" : 666F77


public class PlayerMovement : MonoBehaviour
{
    #region Setup Values -> Awake
    private Controls _inputActions;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;

    public static PlayerMovement instance;

    private Rigidbody _rb;
    private Gravity _gravity;
    private Animator _anim;
    #endregion

    #region SerializeField Values
    [Header("Movement Values")]
    [SerializeField] float movementSpeed;
    [SerializeField] float accel;
    [SerializeField] float deccel;
    [SerializeField] float frictionPercentage;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float bufferTime;
    [SerializeField] float coyoteTime;

    [Header("Dash")]
    [SerializeField] float dashingTime;
    [SerializeField] float dashingPower;
    [SerializeField] float dashRecoverCooldown;

    [Header("Checks")]
    [SerializeField] Vector3 checkGroundSize;
    [SerializeField] Vector3 checkGroundOffset;
    [SerializeField] LayerMask groundLayers;

    [Header("Physics")]
    [SerializeField] float gravityScale;
    [SerializeField] float fallGravityMultiplier;

    [Header("Assigns")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    [SerializeField] CinemachineFreeLook playerCamera;
    [SerializeField] float dashFov;
    [SerializeField] float normalFov;
    #endregion

    private float lastGroundTimer;
    private float lastJumpTimer;
    private float lastDashTimer;
    private float jumpBufferTimer;

    private bool isJumping;
    private bool isDashing;

    private bool grounded;

    [DoNotSerialize] public bool canDash;

    public Vector2 moveDir;

    private float goalFOV;

    private void Awake()
    {
        instance = this;

        _inputActions = new Controls();

        _moveAction = _inputActions.Movements.Move;
        _jumpAction = _inputActions.Movements.Jump;
        _dashAction = _inputActions.Movements.Dash;

        _rb = GetComponent<Rigidbody>();
        _gravity = GetComponent<Gravity>();
        _anim = GetComponent<Animator>();

        playerCamera.m_Lens.FieldOfView = normalFov;
        goalFOV = normalFov;
    }

    #region Enable/Disable
    private void OnEnable()
    {
        _moveAction.Enable();

        _jumpAction.Enable();
        _jumpAction.performed += Jump;

        _dashAction.Enable();
        _dashAction.performed += Dash;
    }

    private void OnDisable()
    {
        _moveAction.Disable();

        _jumpAction.Disable();
        _jumpAction.performed -= Jump;

        _dashAction.Disable();
        _dashAction.performed -= Dash;
    }
    #endregion

    private void Update()
    {
        playerCamera.m_Lens.FieldOfView = Mathf.Lerp(playerCamera.m_Lens.FieldOfView, goalFOV, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        #region Timers
        lastGroundTimer -= Time.fixedDeltaTime;
        lastJumpTimer -= Time.fixedDeltaTime;
        jumpBufferTimer -= Time.fixedDeltaTime;
        lastDashTimer -= Time.fixedDeltaTime;

        if (lastDashTimer <= 0 && isDashing)
        {
            isDashing = false;
            goalFOV = normalFov;
        }
        #endregion

        #region Checks
        if (Physics.OverlapBox(transform.position + checkGroundOffset, checkGroundSize/2, Quaternion.identity, groundLayers).Length != 0)
        {
            if (!grounded)
            {
                _anim.Play("Land");
            }

            lastGroundTimer = 0;
            grounded = true;
            if (!isDashing && lastDashTimer < -dashRecoverCooldown)
            {
                canDash = true;
            }
            if (lastJumpTimer < -0.1f)
            {
                isJumping = false;
            }
            if(jumpBufferTimer >= 0)
            {
                Jump(new());
                jumpBufferTimer = 0;
            }
        }
        else
        {
            if(grounded)
            {
                _anim.Play("AirStart");
            }
            grounded = false;
        }
        #endregion

        #region Physics
        if (!isDashing)
        {
            if (_rb.velocity.y < 0.5f)
            {
                _gravity.gravityScale = gravityScale * fallGravityMultiplier;
            }
            else
            {
                _gravity.gravityScale = gravityScale;
            }

            if (lastGroundTimer >= 0 && moveDir == Vector2.zero && ToVector2(_rb.velocity) != Vector2.zero)
            {
                if (ToVector2(_rb.velocity).sqrMagnitude <= 1)
                {
                    _rb.velocity = new Vector3(0, _rb.velocity.y, 0); ;
                }
                Vector2 brakeVector = -ToVector2(_rb.velocity);
                _rb.AddForce(brakeVector * frictionPercentage, ForceMode.Force);
            }
        }
        #endregion

        #region Movements
        moveDir = _moveAction.ReadValue<Vector2>().normalized;

        if (!isDashing)
        {
            Vector2 targetMovement = OrientVector2(moveDir, orientation) * movementSpeed;
            Vector2 movementDiff = targetMovement - ToVector2(_rb.velocity);
            float accelRate = (Mathf.Abs(targetMovement.sqrMagnitude) > 0.1f) ? accel : deccel;

            Vector2 movement = movementDiff * accelRate;

            _rb.AddForce(ToVector3(movement), ForceMode.Force);
        }
        #endregion
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(lastGroundTimer >= -coyoteTime && !isJumping)
        {
            _rb.velocity =  new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            isJumping = true;
            lastJumpTimer = 0;
            _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
        else
        {
            jumpBufferTimer = bufferTime;
        }
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (canDash && !isDashing)
        {
            canDash = false;
            isDashing = true;
            lastDashTimer = dashingTime;
            _gravity.gravityScale = 0;
            _rb.velocity = Vector3.zero;
            _rb.AddForce(playerObj.forward * dashingPower, ForceMode.Impulse);
            goalFOV = dashFov;
        }
    }

    #region Maths
    public Vector3 ToVector3(Vector2 vector)
    {
        return new Vector3(vector.x, 0, vector.y);
    }

    public Vector2 ToVector2(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    public Vector2 OrientVector2(Vector2 vector, Transform orientation)
    {
        return ToVector2(orientation.forward * vector.y + orientation.right * vector.x);
    }
    #endregion

    #region Utils
    public void Vibrate(float lowF, float highF, float t0ime)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowF, highF);
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + checkGroundOffset, checkGroundSize);
    }
}
