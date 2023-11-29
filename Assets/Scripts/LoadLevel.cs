using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void LoadScene(string scenename)
    {
        Debug.Log("Scene name to load: " + scenename);
        SceneManager.LoadScene(scenename);
    }
}
