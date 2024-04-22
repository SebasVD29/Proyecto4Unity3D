using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float dashSpeed;
    public float climbSpeed;
    public float wallRunningSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Dashing")]
    public float dashSpeedChangeFactor;
    public float maxYSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    //[Header("Keybinds")]
    //public KeyCode jumpKey = KeyCode.Space;
    //public KeyCode sprintKey = KeyCode.LeftShift;
    //public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Componentes")]
    public Climbing climbingScript;
    public Transform orientation;
    Vector2 inputMove;
    Vector3 moveDirection;
    Rigidbody rb;
    
    private bool keepMomentum;
    private float speedChangeFactor;
    //float horizontalInput;
    //float verticalInput;

    private MovementState lastState;
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        climbing,
        wallrunning,
        dashing,
        air
    }

    [Header("Player Action Input")]
    [SerializeField] bool sprintInput = false;
    [SerializeField] bool jumpInput = false;
    [SerializeField] bool crouchInput = false;
    public bool sliding;
    public bool dashing;
    public bool wallrunning;
    public bool climbing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        //MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
        //TextStuff();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    //private void MyInput()
    //{
    //    horizontalInput = Input.GetAxisRaw("Horizontal");
    //    verticalInput = Input.GetAxisRaw("Vertical");

    //    // when to jump
    //    if (Input.GetKey(jumpKey) && readyToJump && grounded)
    //    {
    //        readyToJump = false;

    //        Jump();

    //        Invoke(nameof(ResetJump), jumpCooldown);
    //    }

    //    // start crouch
    //    if (Input.GetKeyDown(crouchKey))
    //    {
    //        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
    //        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    //    }

    //    // stop crouch
    //    if (Input.GetKeyUp(crouchKey))
    //    {
    //        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    //    }
    //} //lISTO PARA ELIMINAR

    //private void Jump()
    //{
    //    exitingSlope = true;

    //    // reset y velocity
    //    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    //    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    //}

    #region MovePlayer
    private void MovePlayer()
    {
        
        if (state == MovementState.dashing || climbingScript.exitingWall)
        {
            return;
        }
        // calculate movement direction
        moveDirection = orientation.forward * inputMove.y + orientation.right * inputMove.x;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        // limit y vel
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    #endregion

    #region Inputs 
    public void Move(InputAction.CallbackContext context)
    {
        //state = MovementState.walking;
        inputMove = context.ReadValue<Vector2>();
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded && readyToJump)
        {
            jumpInput = true;
            readyToJump = false;

            exitingSlope = true;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (context.canceled)
        {
            jumpInput = false;
        }
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintInput = true;

            // moveSpeed = sprintSpeed;
        }
        if (context.canceled)
        {
            sprintInput = false;

            //moveSpeed = walkSpeed;
        }
    }
    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            crouchInput = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        }
        if (context.canceled)
        {
            crouchInput = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    #endregion

    #region Otros
    private void StateHandler()
    {
        // Mode - climbing
        if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        // Mode - wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunningSpeed;
        }
        // Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        // Mode - Crouching
        else if (crouchInput)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
            {
                desiredMoveSpeed = walkSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) 
        { 
            keepMomentum = true; 
        }

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;

            //if (OnSlope())
            //{
            //    float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

            //    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            //}
            //else
            //{
            //    time += Time.deltaTime * speedIncreaseMultiplier;

            //}

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    #region UI
    //public TextMeshProUGUI text_speed;
    //public TextMeshProUGUI text_ySpeed;
    //public TextMeshProUGUI text_mode;
    //private void TextStuff()
    //{
    //    Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    //    if (OnSlope())
    //        text_speed.SetText("Speed: " + Round(rb.velocity.magnitude, 1) + " / " + Round(moveSpeed, 1));

    //    else
    //        text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(moveSpeed, 1));

    //    //float yVel = rb.velocity.y;
    //    //float yMax = maxYSpeed == 0 ? 0 : maxYSpeed;
    //    //text_ySpeed.SetText("YSpeed: " + Round(yVel, 0) + " / " + yMax);

    //    text_mode.SetText(state.ToString());
    //}

    //public static float Round(float value, int digits)
    //{
    //    float mult = Mathf.Pow(10.0f, (float)digits);
    //    return Mathf.Round(value * mult) / mult;
    //}

    #endregion


    #endregion
}
