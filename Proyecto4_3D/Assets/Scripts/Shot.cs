using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class Shot : MonoBehaviour
{

    public GameObject bullet;
    public Transform spawnPoint;
    public float ShotForce=1500;
    public float ShotRate=0.5F;

    private float ShotRateTime=0;

    
       void Update()
    {
        if(Input.GetButtonDown("Fire1")){
            if(Time.time>ShotRateTime)
            {

                GameObject newBullet;
                newBullet = Instantiate(bullet,spawnPoint.position, spawnPoint.rotation) ;

                newBullet.GetComponent<Rigidbody>().AddForce(spawnPoint.forward*ShotRate);

                ShotRateTime=Time.time + ShotRate;

                Destroy(newBullet,2);

            }


        }
    }
}
