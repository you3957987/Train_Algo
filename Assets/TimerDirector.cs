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
        if (Input.GetKeyDown(KeyCode.L))  // GŰ�� ������ Ÿ�̸� �۵�
        {
            game_start = true;
        }

        if (timer > 0 && game_start)
        {
            timer -= Time.deltaTime; // Ÿ�̸� ����
            if (timer < 0) timer = 0; // Ÿ�̸Ӱ� 0 ���Ϸ� �������� �ʵ��� ����
            UpdateTimerUI();
        }
        else if( timer == 0)
        {
            Debug.Log("Time End");
        }
    }
    void UpdateTimerUI()
    {
        // �Ҽ��� 2�ڸ����� ǥ��
        timer_ui.text = timer.ToString("F2");
    }


}
