using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    Vector2 inputMove;
    bool wallRunJumpInput;  
    bool upWardsInput;  // R
    bool downWardsInput; // F

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    bool exitingWallRun;
    public float exitWallRunTime;
    private float exitWallRunTimer;

    [Header("References")]
    public Transform orientation;
    private PlayerMovementAdvanced pm;
    private LedgeGrabbing lg;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        lg = GetComponent<LedgeGrabbing>();
    }
    private void Update()
    {
        CheckForWall();
        StateMachine();
    }
    private void FixedUpdate()
    {
        if (pm.wallrunning)
        {
            WallRunningMovement();
        }
    }
    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }
    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }
    public void MoveWallRunning(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
        //if (inputMove.y > 0)
        //{
        //    upWardsInput = true;
        //    downWardsInput = false;
        //}
        //else
        //{
        //    downWardsInput = true;
        //    upWardsInput = false;
        //}
    }
    public void WallRunningJump(InputAction.CallbackContext context)
    {
        if ((wallRight || wallLeft) && inputMove.y > 0 && AboveGround() && !exitingWallRun)
        {
            if (context.performed )
            {
                wallRunJumpInput = true;
                WallJump();
            }
        }
        
        if (context.canceled)
        {
            wallRunJumpInput = false;
        }
    }
    void StateMachine()
    {
        if ((wallRight || wallLeft) && inputMove.y > 0 && AboveGround() && !exitingWallRun)
        {
            if (!pm.wallrunning)
            {
                StarWallRun();
            }
            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }
            if (wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWallRun = true;
                wallRunTimer = exitWallRunTime;
            }
            if (wallRunJumpInput)
            {
                WallJump();
            }

        }
        else if (exitingWallRun)
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
            if (exitWallRunTimer > 0)
            {
                exitWallRunTimer -= Time.deltaTime;
            }
            if (exitWallRunTimer <= 0)
            {
                exitingWallRun = false;
            }

        }
        else
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
        }
    }
    void StarWallRun()
    {
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;
    }
    void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3 (rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward =-wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        if (!(wallLeft && inputMove.x > 0) && !(wallRight && inputMove.x < 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
    }
    void StopWallRun()
    {
        pm.wallrunning = false;
    }
    void WallJump()
    {
        if (lg.holding || lg.exitingLedge)
        {
            return;
        }

        exitingWallRun = true;
        exitWallRunTimer = exitWallRunTime;
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
