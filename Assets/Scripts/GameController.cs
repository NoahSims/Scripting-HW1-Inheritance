using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private void FixedUpdate()
    {
        if(Input.GetAxis("Reset") != 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }
        if(Input.GetAxis("Cancel") != 0)
        {
            Debug.Log("quiting");
            Application.Quit();
        }
    }
}
