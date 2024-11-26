using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenLoadDirector : MonoBehaviour
{

    public void LoadScene_TO_MainGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void LoadScene_TO_Start()
    {
        SceneManager.LoadScene("start");
    }
}
