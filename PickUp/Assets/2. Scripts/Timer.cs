using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TMP_Text timeText;
    private float gamePlayTimer = 15.0f;
    public bool  isRunning = true;

    void Start()
    {
        timeText.text = "15";
    }
    
    void Update()
    {
        if (isRunning)
        {
            if (gamePlayTimer > 0)
            {
                gamePlayTimer -= Time.deltaTime;
                UpdateTimerDisplay(gamePlayTimer);
            }
            else
            {
                gamePlayTimer = 0;
                isRunning = false;
                // ������ �̾Ƴ����Ĺ���
            }
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int seconds = Mathf.FloorToInt(time);

        timeText.text = seconds.ToString("00");      
    }
}