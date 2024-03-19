using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;


    [Header("Componentes")]
    public Transform orientacion;
    Rigidbody playerRB;
    Vector2 inputMove;
    Vector3 moveDirection;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    public void Move(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    void MovePlayer()
    {
        moveDirection = orientacion.forward * inputMove.y + orientacion.right * inputMove.x;
        playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

    } 


}
