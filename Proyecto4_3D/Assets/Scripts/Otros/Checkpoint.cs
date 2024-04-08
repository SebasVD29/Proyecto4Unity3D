using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string level = SceneManager.GetActiveScene().name;
            float x = other.transform.position.x;
            float y = other.transform.position.y;
            float z = other.transform.position.z;

            other.GetComponent<PlayerRespawn>().ReachedCheckkPoint(level, x, y, z);
        }
    }
}
