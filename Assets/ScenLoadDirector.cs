using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenLoadDirector : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene("MainGame");
    }
}
