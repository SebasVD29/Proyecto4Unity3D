using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Vector2 inputDirection;


    public float sensX;
    public float sensY;

    public Transform orientacion;

    float xRotation; 
    float yRotation;


    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked; 
    }

    // Update is called once per frame
    void Update()
    {
        CameraMove();
    }

    public void Look(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    void CameraMove()
    {
        float mouseX = inputDirection.x * Time.deltaTime * sensX;
        float mouseY = inputDirection.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientacion.rotation = Quaternion.Euler(0, yRotation, 0);
    }



}
