using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public AudioSource MusicPlayer;
    public AnimationCurve MusicRaiseCurve;

    private static bool _hasLoadedTransition;
    private static int _currentLevelNumber;
    private static bool _shouldLoadLevel;
    private float _initialVolume;
    private float _musicVolume;
    private bool _muteMusic;

    public void Start()
    {
        DOTween.Init();
        SceneManager.LoadScene("Tutorial", LoadSceneMode.Additive);
        _initialVolume = MusicPlayer.volume;
        _musicVolume = 0;
        MusicPlayer.volume = 0;
        MusicPlayer.Play();
    }

    public void Update()
    {
        if (_shouldLoadLevel)
        {
            _shouldLoadLevel = false;
            StartCoroutine(ActuallyLoadNextLevel());
        }

        _musicVolume = Mathf.Clamp01(_musicVolume + Time.deltaTime * 0.2f);
        MusicPlayer.volume = _muteMusic ? 0 : MusicRaiseCurve.Evaluate(_musicVolume) * _initialVolume;

        if (Input.GetKeyDown(KeyCode.M))
            _muteMusic = !_muteMusic;
    }

    public static void MarkLoadNextLevel()
    {
        _shouldLoadLevel = true;
    }

    public IEnumerator ActuallyLoadNextLevel()
    {
        yield return SceneManager.UnloadSceneAsync("Tutorial");
        if (!_hasLoadedTransition)
        {
            yield return SceneManager.LoadSceneAsync("Transition", LoadSceneMode.Additive);
            _hasLoadedTransition = true;
        }

        _currentLevelNumber += 1;
        yield return SceneManager.LoadSceneAsync($"Level_{_currentLevelNumber}", LoadSceneMode.Additive);
    }
}
