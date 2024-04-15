using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public bool freeze; 
    public enum MovementState
    {
        walking,
        sprinting,
        crounching,
        air
    }
    public MovementState state;

    [Header("Componentes")]
    public Transform orientacion;
    Rigidbody playerRB;
    Vector2 inputMove;
    Vector3 moveDirection;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

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
        MovePlayer();
        if (freeze)
        {
            playerRB.velocity = Vector3.zero;
        }
    
    }
    //private void FixedUpdate()
    //{
    //    MovePlayer();
    //}

    #region Movement 
    void MovePlayer()
    {
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
    void StateHandler()
    {
        if (crouchInput)
        {
            state = MovementState.crounching;
            moveSpeed = crouchSpeed;

        }
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
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
