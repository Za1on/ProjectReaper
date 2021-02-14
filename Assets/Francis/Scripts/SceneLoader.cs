using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void PlayedDied()
    {
        SceneManager.LoadScene(0);
    }
    public void RetryProto()
    {
        SceneManager.LoadScene(0);
    }
}
