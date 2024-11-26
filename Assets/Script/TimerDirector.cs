using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{

    public TMP_Text timer_ui;
    float timer = 5.00f;

    public bool game_start = false;
    public GameDirector gameDirector;
    public GameObject game_end_ui;
    public bool game_end = false;

    //사운드 관련
    public MainBackSound main_back;
    public AudioSource game_over;

    void Start()
    {
        UpdateTimerUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))  // G키를 누르면 타이머 작동
        {
            game_start = true;
        }

        if (timer > 0 && game_start)
        {
            timer -= Time.deltaTime; // 타이머 감소
            if (timer < 0)
            {
                game_over.Play();
                timer = 0; // 타이머가 0 이하로 내려가지 않도록 제한
            }
            UpdateTimerUI();
        }
        
        if( timer == 0 || gameDirector.current_max_line_weight <= 0)
        {
            game_end_ui.gameObject.SetActive(true);
            Debug.Log("Game End");
            
            main_back.Off_main_backsound();
            game_end = true;
        }

    }
    void UpdateTimerUI()
    {
        // 소수점 2자리까지 표시
        timer_ui.text = timer.ToString("F2");
    }


}
