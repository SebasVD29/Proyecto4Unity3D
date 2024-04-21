using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public int damage;

    private Rigidbody rb;

    private bool targetHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Asegurar que se adhiera al primer objetivo al que le pega
        if (targetHit)
        {
            return;
        }
        else
            targetHit = true;

        //Verificar si se golpeo a un enemigo 
        if(collision.gameObject.GetComponent<BasicEnemy>() != null)
        {
            BasicEnemy enemy = collision.gameObject.GetComponent<BasicEnemy>();

            enemy.TakeDamage(damage);

            Destroy(gameObject);
        }

        // Asegurarse que el proyectil se adhiere a la superficie 
        rb.isKinematic = true;

        //Asegurarse que el proyectil se mueve con el objetivo 
        transform.SetParent(collision.transform);
    }


}
