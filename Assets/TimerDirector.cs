using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{

    public TMP_Text timer_ui;
    float timer = 30.00f;

    bool game_start = false;

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
            if (timer < 0) timer = 0; // 타이머가 0 이하로 내려가지 않도록 제한
            UpdateTimerUI();
        }
        else if( timer == 0)
        {
            Debug.Log("Time End");
        }
    }
    void UpdateTimerUI()
    {
        // 소수점 2자리까지 표시
        timer_ui.text = timer.ToString("F2");
    }


}
