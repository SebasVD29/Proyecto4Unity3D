using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
    {
        walking,
        sprinting,
        crounching,
        dashing,
        air
    }
    public MovementState state;
    private float speedChangeFactor;
    public bool dashing;
    private float desiredMoveSpeed;

    [Header("Componentes")]
    public Transform orientacion;
    Rigidbody playerRB;
    Vector2 inputMove;
    Vector3 moveDirection;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public float maxYSpeed;


    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    float startYScale;

    [Header("GroundChecks")]
    public float playerHeigth;
    public float groundDrag;
    public LayerMask groundLayer;
    bool grounded;

    [Header("Slope Handing")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    [Header("Player Action Input")]
    [SerializeField] bool sprintInput = false;
    [SerializeField] bool jumpInput = false;
    [SerializeField] bool crouchInput = false;

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();

        startYScale = transform.localScale.y;
    }
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeigth * 0.5f + 0.2f, groundLayer);
        
        HandleGrad();
        SpeedControl();
        StateHandler();
        //MovePlayer();
    }
    private void FixedUpdate()
    {
        MovePlayer();

        // handle drag
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crounching)
            playerRB.drag = groundDrag;
        else
            playerRB.drag = 0;
    }

    #region Movement 
    void MovePlayer()
    {
        if (state == MovementState.dashing) return;

        moveDirection = orientacion.forward * inputMove.y + orientacion.right * inputMove.x;

        if (OnSlope() && !exitingSlope)
        {
            playerRB.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (playerRB.velocity.y > 0)
            {
                playerRB.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {

            playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        }

    } 
    void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (playerRB.velocity.magnitude > moveSpeed)
            {
                playerRB.velocity = playerRB.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                playerRB.velocity = new Vector3(limitedVel.x, playerRB.velocity.y, limitedVel.z);
            }
        }
        // limit y vel
        if (maxYSpeed != 0 && playerRB.velocity.y > maxYSpeed)
            playerRB.velocity = new Vector3(playerRB.velocity.x, maxYSpeed, playerRB.velocity.z);

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

            playerRB.velocity = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z);
            playerRB.AddForce(transform.up * jumpForce, ForceMode.Impulse);

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
            playerRB.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        }
        if (context.canceled)
        {
            crouchInput = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    #endregion

    #region Otros 
    private bool isGrounded()
    {
       // grounded = Physics.Raycast(transform.position, Vector3.down, playerHeigth * 0.5f + 0.2f, groundLayer);
        return Physics.Raycast(transform.position, Vector3.down, playerHeigth * 0.5f + 0.2f, groundLayer);
    }
    void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    void HandleGrad()
    {
        if (grounded)
        {
            playerRB.drag = groundDrag;
        }
        else
        {
            playerRB.drag = 0;
        }
    }

    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (crouchInput)
        {
            state = MovementState.crounching;
            desiredMoveSpeed = crouchSpeed;

        }
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

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

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeigth * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    #endregion


}
