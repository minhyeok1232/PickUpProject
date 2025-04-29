using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private ClawMover clawMover;
    
    public TMP_Text timeText;
    
    public float startTime = 15.0f;
    
    private float gamePlayTimer;
    
    private bool hasTriggered = false;
    public  bool isRunning = true;
    

    void Start()
    {
        gamePlayTimer = startTime;
        UpdateTimerDisplay(gamePlayTimer);
    }

    void Update()
    {
        if (!isRunning || hasTriggered) return;

        gamePlayTimer -= Time.deltaTime;
        
        if (gamePlayTimer <= 0f)
        {
            gamePlayTimer = 0f;
            isRunning = false;
            TriggerTimeout();
        }

        UpdateTimerDisplay(gamePlayTimer);
    }

    void UpdateTimerDisplay(float time)
    {
        int seconds = Mathf.FloorToInt(Mathf.Max(time, 0f));
        timeText.text = $"남은시간 {seconds}초";
    }

    void TriggerTimeout()
    {
        hasTriggered = true;
        
        if(clawMover) clawMover.AutoGrabWhenTimerZero();
    }
}