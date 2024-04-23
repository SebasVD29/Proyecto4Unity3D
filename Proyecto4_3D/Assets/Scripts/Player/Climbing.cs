using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public PlayerMovementAdvanced pm;
    public LedgeGrabbing lg;
    public LayerMask whatIsWall;
    Vector2 inputMove;
    bool forwardInput;


    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;
    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    [Header("Player Action Input")]
    [SerializeField] bool climJumpInput = false;
    //public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;


    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
        { 
            ClimbingMovement(); 
        }
    }

    private void StateMachine()
    {
        if (lg.holding )
        {
            if (climbing)
            {
                StopClimbing();
            }
        }
        // State 1 - Climbing
        else if (wallFront && forwardInput && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0)
            {
                StartClimbing();
            }
            // timer
            if (climbTimer > 0)
            {
                climbTimer -= Time.deltaTime;
            }
            if (climbTimer < 0)
            {
                StopClimbing();
            }
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (climbing) 
            { 
                StopClimbing(); 
            }
            if (exitWallTimer > 0) 
            { 
                exitWallTimer -= Time.deltaTime; 
            }
            if (exitWallTimer < 0) 
            { 
                exitingWall = false; 
            }
        }
        // State 3 - None
        else
        {
            if (climbing) 
            { 
                StopClimbing(); 
            }
        }
        if (wallFront && climJumpInput && climbJumpsLeft > 0) 
        { 
            ClimbJump(); 
        }
    }
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall ||
            Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }
    public void ClimbJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            climJumpInput = true;
        }
        if (context.canceled)
        {
            climJumpInput = false;
        }
    }
    public void MoveClimbing(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
        if (inputMove.y > 0)
        {
            forwardInput = true;
        }
        else
        {
            forwardInput = false;
        }
    }
    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        /// idea - camera fov change
    }
    private void ClimbingMovement()
    { 
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);

        /// idea - sound effect
    }
    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;

        /// idea - particle effect
        /// idea - sound effect
    }
    private void ClimbJump()
    {
        if (pm.grounded)
        {
            return;
        }
        if (lg.holding || lg.exitingLedge)
        {
            return;
        }

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}

