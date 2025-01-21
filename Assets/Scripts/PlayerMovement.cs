using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Setup Values -> Awake
    private Controls _inputActions;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    public static PlayerMovement instance;

    private Rigidbody _rb;
    private Gravity _gravity;
    #endregion

    #region SerializeField Values
    [Header("Movement Values")]
    [SerializeField] float movementSpeed;
    [SerializeField] float accel;
    [SerializeField] float deccel;
    [SerializeField] float frictionPercentage;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float coyoteTime;

    [Header("Checks")]
    [SerializeField] Vector3 checkGroundSize;
    [SerializeField] Vector3 checkGroundOffset;
    [SerializeField] LayerMask groundLayers;

    [Header("Physics")]
    [SerializeField] float gravityScale;
    [SerializeField] float fallGravityMultiplier;

    [Header("Camera")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] Transform orientation;
    #endregion

    private float lastGroundTime;
    private float lastJumpTime;

    private bool isJumping;

    public Vector2 moveDir;

    private void Awake()
    {
        instance = this;

        _inputActions = new Controls();

        _moveAction = _inputActions.Movements.Move;
        _jumpAction = _inputActions.Movements.Jump;

        _rb = GetComponent<Rigidbody>();
        _gravity = GetComponent<Gravity>();
    }

    #region Enable/Disable
    private void OnEnable()
    {
        _moveAction.Enable();

        _jumpAction.Enable();
        _jumpAction.performed += Jump;
    }

    private void OnDisable()
    {
        _moveAction.Disable();

        _jumpAction.Disable();
        _jumpAction.performed -= Jump;
    }
    #endregion

    private void FixedUpdate()
    {
        #region Timers
        lastGroundTime -= Time.fixedDeltaTime;
        lastJumpTime -= Time.fixedDeltaTime;
        #endregion

        #region Checks
        if (Physics.OverlapBox(transform.position + checkGroundOffset, checkGroundSize/2, Quaternion.identity, groundLayers).Length != 0)
        {
            lastGroundTime = 0;
            if (lastJumpTime < -0.1f)
            {
                isJumping = false;
            }
        }
        #endregion

        #region Physics
        if (_rb.velocity.y < 0.5f)
        {
            _gravity.gravityScale = gravityScale * fallGravityMultiplier;
        } 
        else
        {
            _gravity.gravityScale = gravityScale;
        }

        if (lastGroundTime >= 0 && moveDir == Vector2.zero && ToVector2(_rb.velocity) != Vector2.zero)
        {
            if (ToVector2(_rb.velocity).sqrMagnitude <= 1)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0); ;
            }
            Vector2 brakeVector = -ToVector2(_rb.velocity);
            _rb.AddForce(brakeVector * frictionPercentage, ForceMode.Force);
        }
        #endregion

        #region Movements
        moveDir = _moveAction.ReadValue<Vector2>().normalized;

        //Vector2 targetMovement = moveDir * movementSpeed ;
        //Vector2 movementDiff = targetMovement - ToVector2(_rb.velocity);
        //float accelRate = (Mathf.Abs(targetMovement.sqrMagnitude) > 0.1f) ? accel : deccel;

        //Vector2 movement = movementDiff * accelRate;

        //_rb.AddForce(ToVector3(movement));
        #endregion
    }


    private void Jump(InputAction.CallbackContext context)
    {
        if(lastGroundTime >= -coyoteTime && !isJumping)
        {
            _rb.velocity =  new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            isJumping = true;
            _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
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
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + checkGroundOffset, checkGroundSize);
    }
}
