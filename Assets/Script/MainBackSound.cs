using UnityEngine;

public class MainBackSound : MonoBehaviour
{

    public AudioSource main_back;

    
    public void Play_main_backsound()
    {
        main_back.Play();
    }

    public void Off_main_backsound()
    {
        main_back.Stop();
    }

}
