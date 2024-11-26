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

    //���� ����
    public MainBackSound main_back;
    public AudioSource game_over;

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
            if (timer < 0)
            {
                game_over.Play();
                timer = 0; // Ÿ�̸Ӱ� 0 ���Ϸ� �������� �ʵ��� ����
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
        // �Ҽ��� 2�ڸ����� ǥ��
        timer_ui.text = timer.ToString("F2");
    }


}
