using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    [Header("MovementJump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Componentes")]
    public Transform orientacion;
    Rigidbody playerRB;
    Vector2 inputMove;
    Vector3 moveDirection;

    [Header("GroundChecks")]
    public float playerHeigth;
    public float groundDrag;
    public LayerMask groundLayer;
    bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleGrad();
        SpeedControl();
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private bool isGrounded()
    {
       // grounded = Physics.Raycast(transform.position, Vector3.down, playerHeigth * 0.5f + 0.2f, groundLayer);
        return Physics.Raycast(transform.position, Vector3.down, playerHeigth * 0.5f + 0.2f, groundLayer);
    }
    void MovePlayer()
    {
        moveDirection = orientacion.forward * inputMove.y + orientacion.right * inputMove.x;
        playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

    } 

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            playerRB.velocity = new Vector3(limitedVel.x, playerRB.velocity.y, limitedVel.z);
        }
    }


    void HandleGrad()
    {
        if (isGrounded())
        {
            playerRB.drag = groundDrag;
        }
        else
        {
            playerRB.drag = 0;
        }
    }


    public void Move(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }


}
