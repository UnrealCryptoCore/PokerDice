using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    private static GameHandler _instance;
    public GameClient client = new();
    public IOverlayManager overlayManager;

    private readonly Queue<Action> _executionQueue = new();

    public static GameHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singleton = new("GameHandler");
                _instance = singleton.AddComponent<GameHandler>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update() {
        lock (_executionQueue) {
            while(_executionQueue.Count > 0) {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    // used to run code from async thread to be able to interact with main thread
    public void Enqueue(Action action) {
        lock(_executionQueue) {
            _executionQueue.Enqueue(action);
        }
    }

    private void OnApplicationQuit()
    {
        client.CloseConnection();
    }

    public void LoadScene(string sceneName, Action action)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, action));
    }

    
    IEnumerator LoadSceneCoroutine(string sceneName, Action action)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncLoad.isDone) {
            yield return null;
        }

        action();
        
    }
 
}
