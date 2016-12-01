﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Class the handles the overall map
/// Uses other managers to create and handle aspects of the map
/// </summary>
public class WorldMap : MonoBehaviour
{

    [SerializeField]
    private Settings settings;
    public static MapColorPalet colorPalet;

    private RoadFactory roadFac;
    private BuildingFactory buildFac;
    private TileManager tileMan;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        roadFac = GetComponentInChildren<RoadFactory>();
        buildFac = GetComponentInChildren<BuildingFactory>();
        tileMan = GetComponent<TileManager>();

#if UNITY_EDITOR
        tileMan.Initialize(buildFac, roadFac, settings);
        if (settings.mapColorPalet != colorPalet)
            colorPalet = settings.mapColorPalet;

        new GameObject("DebugRouting").AddComponent<DebugRouting>().Initialize(settings);
#endif
#if UNITY_ANDROID
        StartCoroutine(Init());
#endif

        UIController.Instance.Map = this;
    }

    /// <summary>
    /// Coroutines used on devices with GPS to initialize the map
    /// </summary>
    /// <returns></returns>
    private IEnumerator Init()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;


        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            LocationInfo locData = Input.location.lastData;

            settings.latitude = locData.latitude;
            settings.longtitude = locData.longitude;
            tileMan.Initialize(buildFac, roadFac, settings);
        }
    }

    public void BeginRouteGeneration(RouteLength _routeLength)
    {
        List<Vector2> temp = null;
        GameObject.Find("RouteManager").GetComponent<RouteManager>().InitiateRouteGeneration(new Vector2(settings.latitude, settings.longtitude), temp, settings, _routeLength);
    }

    /// <summary>
    /// Settings for the map
    /// </summary>
    [Serializable]
    public class Settings
    {

        //56.407051, 10.876623
        [SerializeField]
        public float latitude = 56.407051f;//56.410394f;
        [SerializeField]
        public float longtitude = 10.876623f;//10.886543f;
        [SerializeField]
        public int detailLevel = 16;
        [SerializeField]
        public int range = 3;
        [SerializeField]
        public bool loadImages = false;
        [SerializeField]
        public MapColorPalet mapColorPalet;
    }
}