using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Throwing : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;


    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;


    [Header("Throwing")]
   // public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
    }

    private void Update()
    {
        //if(Input.GetKeyDown(throwKey) && readyToThrow && totalThrows > 0)
        //{
        //    Throw();
        //}
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed && readyToThrow && totalThrows > 0)
        {
            Throw();
        }
    }
    private void Throw()
    {
        readyToThrow = false;

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        Vector3 forceToAdd = cam.transform.forward * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse );

        totalThrows--;

        Invoke(nameof(ResetThrow), throwCooldown);

    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
         
}
