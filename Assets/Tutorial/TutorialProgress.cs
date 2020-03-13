using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class TutorialProgress : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public TMP_Text TextElement;
    public Transform VisualsPane;

    private TutorialStep[] _steps;
    private int _step;
    private bool _loadingNextStep;
    private int _slideLength;
    private bool _starting;

    private bool StepsLeft => _step < _steps.Length;

    public void Start()
    {
        _steps = GetComponentsInChildren<TutorialStep>();

        _slideLength = 1920;
        VisualsPane.localPosition += Vector3.left * _slideLength;

        StartCoroutine(NextStep());
    }

    public void Update()
    {
        if (!_loadingNextStep)
        {
            if (Input.GetKeyDown(KeyCode.Space) && StepsLeft)
                StartCoroutine(NextStep());
            else if((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)) && !_starting)
                StartCoroutine(StartGame());
        }
    }

    private IEnumerator NextStep()
    {
        _loadingNextStep = true;

        _step += 1;
        var currentStep = _steps[_step - 1];

        yield return HideVisuals();
        LoadVideo(currentStep);
        var vpos = VisualsPane.localPosition;
        VisualsPane.localPosition = new Vector3(-vpos.x, vpos.y, vpos.z);

        while (VideoPlayer.clip != null && !VideoPlayer.isPrepared)
            yield return null;

        yield return ShowVisuals();

        _loadingNextStep = false;
        yield return null;
    }

    private void LoadVideo(TutorialStep currentStep)
    {
        VideoPlayer.clip = currentStep.Video;
        TextElement.text = currentStep.Text;
    }

    private IEnumerator HideVisuals()
    {
        yield return VisualsPane
            .DOLocalMoveX(-_slideLength, 0.35f)
            .SetEase(Ease.InSine)
            .WaitForCompletion();
    }

    private IEnumerator ShowVisuals()
    {
        yield return VisualsPane.transform
            .DOLocalMoveX(0, 0.35f)
            .SetEase(Ease.OutSine)
            .WaitForCompletion();
    }

    private IEnumerator StartGame()
    {
        _starting = true;
        yield return HideVisuals();
        LevelLoader.MarkLoadNextLevel();
    }
}
