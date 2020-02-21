using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float ScoreTimeInSeconds = float.MaxValue;
    public float BestScoreTimeInSeconds = float.MaxValue;

    private float _startTime;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartTimer()
    {
        _startTime = Time.time;
    }

    public void StopTimer()
    {
        ScoreTimeInSeconds = Time.time - _startTime;

        if (ScoreTimeInSeconds < BestScoreTimeInSeconds)
            BestScoreTimeInSeconds = ScoreTimeInSeconds;
    }
}
