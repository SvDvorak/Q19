using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;
    private float ScoreTimeInSeconds = float.MaxValue;
    private float BestScoreTimeInSeconds = float.MaxValue;

    private float _startTime;

    public void StartTimer()
    {
        _startTime = Time.time;

        TextMesh.enabled = false;
    }

    public void StopTimer()
    {
        ScoreTimeInSeconds = Time.time - _startTime;

        if (ScoreTimeInSeconds < BestScoreTimeInSeconds)
            BestScoreTimeInSeconds = ScoreTimeInSeconds;

        TextMesh.text = $"Last: {ScoreTimeInSeconds:F3}\n" +
                        $"Best: {BestScoreTimeInSeconds:F3}\n" + 
                        $"Developer: {2.48:F3}";

        TextMesh.enabled = true;
    }
}
