using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    public float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    //public KeyCode slideKey = KeyCode.LeftControl;
    //private float horizontalInput;
    //private float verticalInput;

    Vector2 inputMove;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();

        startYScale = playerObj.localScale.y;

    }

    private void Update()
    {
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        //if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        //{
        //    StartSlide();
        //}

        //if (Input.GetKeyUp(slideKey) && pm.sliding)
        //    StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }
    public void MoveSliding(InputAction.CallbackContext context)
    {
        //state = MovementState.walking;
        inputMove = context.ReadValue<Vector2>();
    }
    public void Slid(InputAction.CallbackContext context)
    {
        if (context.performed && (inputMove.x != 0 || inputMove.y != 0))
        {
            StartSlide();
        }
        if (context.canceled && pm.sliding)
        {
            StopSlide();
        }
    }
    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;


    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * inputMove.y + orientation.right * inputMove.x;

        // Sliding de manera normal 
        if(!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        //Sliding down a slope 
        else
        {

            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

    }

}
