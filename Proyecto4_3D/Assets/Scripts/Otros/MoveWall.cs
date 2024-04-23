using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWall : MonoBehaviour
{
    public Transform wall;
    public LayerMask whatIsShoot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "shoot")
        {
            Debug.Log("Entro");
            wall.position = new Vector3(39.1599998f, -3.87289143f, 19.2f);

        }
        
    }
}
