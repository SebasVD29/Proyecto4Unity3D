using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Throwing : MonoBehaviour
{
    [Header("Reference")]

    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Settings")]
    public int totalThrows;
    public float throwCoolDown;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(throwKey) && readyToThrow && totalThrows > 0)
        {
            Throw();
        }
    }
    private void Throw()
    {
        readyToThrow = false;

        //Instanciar el objeto a tirar
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        //Obtener el rigidbody del componente
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();


        //Calcular la direccion 
        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;
        

        if(Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        //A;adir fuerza
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        //Implementar funcion ThrowCoolDown
        Invoke(nameof(ResetThrow), throwCoolDown);
    }

    private void ResetThrow()
    {
        readyToThrow = true; 
    }
}
