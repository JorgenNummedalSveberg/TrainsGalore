using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loadingScreen;
    
    public enum SceneIndex
    {
        Manager = 0,
        MenuScreen = 1,
        GameScreen = 2
    }

    private void Awake()
    {
        Instance = this;
        
        _LoadAndSetScene((int) SceneIndex.MenuScreen);
    }

    public static void LoadGame()
    {
        Instance._LoadGame();
    }

    private List<AsyncOperation> _scenesLoading = new List<AsyncOperation>();
    private void _LoadGame()
    {
        loadingScreen.SetActive(true);
        _scenesLoading.Add(_LoadAndSetScene((int) SceneIndex.GameScreen));
        _scenesLoading.Add(SceneManager.UnloadSceneAsync((int) SceneIndex.MenuScreen));

        StartCoroutine(GetSceneLoadProgress());
        StartCoroutine(GetTotalProgress());
    }

    private AsyncOperation _LoadAndSetScene(int buildIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
        operation.completed += asyncOperation =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
        };

        return operation;
    }

    private float _totalSceneProgress;
    private float _currentSceneProgress;
    public IEnumerator GetSceneLoadProgress()
    {
        _totalSceneProgress = 0;
        float progress = 0;
        
        for (int i = 0; i < _scenesLoading.Count; i++)
        {
            while (!_scenesLoading[i].isDone)
            {

                foreach (AsyncOperation asyncOperation in _scenesLoading)
                {
                    progress += asyncOperation.progress;
                }

                _totalSceneProgress = (progress / _scenesLoading.Count);
                
                yield return null;
            }

            _totalSceneProgress = 1;
        }
    }

    public IEnumerator GetTotalProgress()
    {
        float totalProgress = 0;

        while (EndlessTerrain.Current == null || !EndlessTerrain.Current.isDone)
        {
            if (EndlessTerrain.Current == null)
            {
                _currentSceneProgress = 0;
            }
            else
            {
                float prog = EndlessTerrain.Current.chunkAddProgress + EndlessTerrain.Current.meshGenProgress + EndlessTerrain.Current.treeGenProgress;
                _currentSceneProgress = prog / (EndlessTerrain.Current.chunkCount*3);
            }
            
            totalProgress = (_totalSceneProgress + _currentSceneProgress) / 2;
            UpdateProgress(totalProgress);

            yield return null;
        }

        _currentSceneProgress = 1;
        totalProgress = (_totalSceneProgress + _currentSceneProgress) / 2;
        UpdateProgress(totalProgress);
        
        loadingScreen.SetActive(false);
    }

    public static void QuitGame()
    {
        Instance._QuitGame();
    }

    private void _QuitGame()
    {
        Application.Quit();
    }

    public static void UpdateProgress(float progress)
    {
        Instance._UpdateProgress(progress);
    }

    private void _UpdateProgress(float progress)
    {
        Instance.loadingScreen.GetComponent<LoadingScreen>().SetProgress(progress);
    }
}
