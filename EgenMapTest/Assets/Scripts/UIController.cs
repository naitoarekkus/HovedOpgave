﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// In-scene singleton for UI elements not part of a game activity and scene transistion
/// </summary>
public class UIController : MonoBehaviour
{
    private WorldMap map = null;

    private static UIController instance;
    public Button btnStartGame;
    public Image pnlEndRoute;
    public Image pnlInGameMenu;
    public Button btnDebugStartRoute;
    public Button btnDebugEndRoute;
    private GameObject refMainScene;

    /// <summary>
    /// Unity Singleton
    /// </summary>
    public static UIController Instance
    {
        get
        {
            if (instance == null) instance = GameObject.Find("UIController").GetComponent<UIController>();

            return instance;
        }
    }

    public GameObject RefMainScene
    {
        get
        {
            return refMainScene;
        }

        set
        {
            refMainScene = value;
        }
    }

    public WorldMap Map
    {
        get
        {
            return map;
        }

        set
        {
            map = value;
        }
    }

    /// <summary>
    /// Unity method called every time game object is enabled
    /// </summary>
    void OnEnable()
    {
#if UNITY_ANDROID
        btnDebugStartRoute.gameObject.SetActive(true);
        btnDebugEndRoute.gameObject.SetActive(true);
#endif
    }

    /// <summary>
    /// Add new scene to the scene hierarchy and then activate that scene
    /// </summary>
    /// <param name="sceneName">Name of scene</param>
    public void LoadScene(string _sceneName)
    {
        StartCoroutine(LoadNextScene(_sceneName));
    }

    /// <summary>
    /// Coroutine to load a scene
    /// </summary>
    /// <param name="_sceneName">Name of of scene</param>
    /// <returns></returns>
    private IEnumerator LoadNextScene(string _sceneName)
    {
        print("Load Next Scene Started");
        AsyncOperation _async = new AsyncOperation();
        _async = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        _async.allowSceneActivation = false;

        while (!_async.isDone) //Keep in leep as long as scene isn't loaded
        {
            yield return null;
            if (_async.progress >= 0.9f) //W´hen a scene is 90% loaded, it needs allowSceneActivation as true to load the remaining 10%
            {
                _async.allowSceneActivation = true;
            }
        }

        Scene nextScene = SceneManager.GetSceneByName(_sceneName);
        if (nextScene.IsValid()) //Makes sure it's a valid scene
        {
            if (nextScene.name == "GameScene") //Is it the game scene that is being loaded?
            {
                refMainScene = GameObject.FindGameObjectWithTag("AllMainObjects"); //Find the parent to all objects in main scene. Needed when unloading from game scene
                refMainScene.SetActive(false); //Disable all objects in Main scene
                btnStartGame.gameObject.SetActive(false); //Hide button
            }
            //Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(nextScene); //Activates the loaded scene
            print("Loaded next scene");
        }
    }

    /// <summary>
    /// Button Click Method.
    /// Generates a route
    /// </summary>
    public void ClickStartRoute(int _length)
    {
        if (Map == null) return;

        RouteLength length = 0;
        switch (_length)
        {
            case 2:
                length = RouteLength.SHORT;
                break;
            case 5:
                length = RouteLength.MIDDLE;
                break;
            case 10:
                length = RouteLength.LONG;
                break;
        }

        if (length == 0) return;
        Map.BeginRouteGeneration(length);
    }

    /// <summary>
    /// Button Click Method
    /// Ends a route
    /// </summary>
    public void ClickEndRoute()
    {
        RouteManager.Instance.EndRoute();
    }
}
