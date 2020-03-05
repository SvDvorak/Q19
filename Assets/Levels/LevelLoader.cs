using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private static bool _hasLoadedTransition;
    private static int _currentLevelNumber;
    private Scene _tutorial;
    private static bool _shouldLoadLevel;

    public void Start()
    {
        DOTween.Init();
        SceneManager.LoadScene("Tutorial", LoadSceneMode.Additive);
        //SceneManager.LoadScene("Transition", LoadSceneMode.Additive);
        //SceneManager.LoadScene($"Level_{_currentLevelNumber + 1}", LoadSceneMode.Additive);
    }

    public void Update()
    {
        if (_shouldLoadLevel)
        {
            _shouldLoadLevel = false;
            StartCoroutine(ActuallyLoadNextLevel());
        }
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
