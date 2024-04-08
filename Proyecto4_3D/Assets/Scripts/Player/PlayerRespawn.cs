using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    string actualLevel;
    string level;
    float xPos, yPos, zPos;

    // Start is called before the first frame update
    void Start()
    {
        actualLevel = SceneManager.GetActiveScene().name;
        //Obtien las variables del checkpoint 
        level = PlayerPrefs.GetString("checkPointLevel");

        xPos = PlayerPrefs.GetFloat("checkPointX");
        yPos = PlayerPrefs.GetFloat("checkPointY");
        zPos = PlayerPrefs.GetFloat("checkPointZ");
        //Valida si el nivel actual es el mismo del ultimo checkpoint 
        if (actualLevel == level)
        {
            transform.position = new Vector3 (xPos, yPos, zPos);
        }
    }

    public void ReachedCheckkPoint(string level, float x, float y, float z)
    {
        PlayerPrefs.SetString("checkPointLevel", level);
        PlayerPrefs.SetFloat("checkPointX", x);
        PlayerPrefs.SetFloat("checkPointY", y);
        PlayerPrefs.SetFloat("checkPointZ", z);
    }

    public void PlayerDeath()
    {
        Invoke("LoadLevel", 0.1f);
    }

    void LoadLevel()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("checkPointLevel")))
        {
            //Debug.Log("name == null" + PlayerPrefs.GetString("checkPointLevel"));
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            //Debug.Log("name != null "+PlayerPrefs.GetString("checkPointLevel"));
            SceneManager.LoadScene(PlayerPrefs.GetString("checkPointLevel"));
        }

    }
}
