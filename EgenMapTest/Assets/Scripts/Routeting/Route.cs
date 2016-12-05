﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Helpers;
using UniRx;
using System.Linq;

/// <summary>
/// Class containing all the informaton the makes a route
/// This includes points following the map roads, 
/// between start and via points, generated by the API.
/// Via points located outside an acutal road is snapped to the nearest road,
/// however the gab can be too large. The API will ignore that via point.
/// 
/// TODO: Error Handle when a via point is ignored.
/// </summary>
public class Route {

    private readonly string distanceFormat = "M";
    private readonly string apiUrl = "http://openls.geog.uni-heidelberg.de/route?api_key=ee0b8233adff52ce9fd6afc2a2859a28&start={0}&end={1}&via={2}&lang={3}&distunit={4}&routepref={5}&weighting={6}&avoidAreas=&useTMC=false&noMotorways=true&noTollways=false&noUnpavedroads=&noSteps=&noFerries=true&instructions=false";
    private readonly string transportType = "Pedestrian";
    private readonly string routingLanguage = "en";
    private readonly string routeWeight = "Recommended";

    private float distance;
    private List<Vector2> viaLatLongs;
    private List<Vector2> routeLatLongs;
    private List<Vector2> routeInMercCoords = new List<Vector2>();
    private bool dataLoaded;
    private TimeSpan estimatedTime;
    private int zoom = 16;
    private Vector2 startPosition;

    private RouteLength length;

    /// <summary>
    /// Whether the data is loaded from the API or not
    /// </summary>
    public bool DataLoaded { get { return dataLoaded; } }
    /// <summary>
    /// List of coordinates of points, which makes out the route generated by the OSM API
    /// </summary>
    public List<Vector2> RouteInMercCoords { get { return routeInMercCoords; } }
    /// <summary>
    /// The distance, in meters, of the whole route
    /// </summary>
    public float Distance { get { return distance; } }
    /// <summary>
    /// The estimated time it takes to complete the route
    /// </summary>
    public TimeSpan EstimatedTime { get { return estimatedTime; } }
    /// <summary>
    /// Used for debug purposes
    /// </summary>
    public List<Vector2> ViaLatLongs { get { return viaLatLongs; } }
    /// <summary>
    /// Used for debug purposes
    /// </summary>
    public List<Vector2> RouteLatLongs { get { return routeLatLongs; } }

    /// <summary>
    /// Initializes a route
    /// </summary>
    /// <param name="_startPos">The position of the user</param>
    /// <param name="_via">List of via points API uses to generate route</param>
    /// <param name="_zoom">The detail level of the GPS</param>
    /// <param name="_length">The desired length of the route</param>
    /// <returns>A complete route containing all needed information</returns>
    public Route Initialize(Vector2 _startPos, List<Vector2> _via, int _zoom, RouteLength _length)
    {
        viaLatLongs = _via;
        zoom = _zoom;
        dataLoaded = false;
        distance = 0;

        length = _length;
        startPosition = _startPos;

        LoadAPIData(_startPos, _via);

        return this;
    }

    /// <summary>
    /// Gets the data need to create a route from the API
    /// </summary>
    /// <param name="_startPos">The position of the user</param>
    /// <param name="_via">List of via points API uses to generate route</param>
    private void LoadAPIData(Vector2 _startPos, List<Vector2> _via)
    {
        string startEnd = _startPos.y + "," + _startPos.x;

        string viaFormatedString = "";

        //Runs through the _via list to add potential via points to the string
        for (int i = 0; i < _via.Count; i++)
        {
            viaFormatedString += _via[i].y + "," + _via[i].x + "%20"; //%20 is the URL equivalent to a space (" ")
        }

        viaFormatedString = viaFormatedString.Remove(viaFormatedString.Length - 3); //Deletes the last "%20" from the string

        //string containing all the data we need from the API
        string url = string.Format(apiUrl, startEnd, startEnd, viaFormatedString, routingLanguage, distanceFormat, transportType, routeWeight);
        ObservableWWW.Get(url) //Third party code, meant for making task threaded
            .Subscribe(
            ConvertAPIData, FailToGetRouteAPIData);//succes
            //exp => Debug.Log("Error fetching -> " + url)); //Error
        
    }

    /// <summary>
    /// Exeption Handling if failed to get data from api
    /// </summary>
    /// <param name="ex">The exeption</param>
    private void FailToGetRouteAPIData(Exception ex)
    {
        Debug.Log("Error getting route - " + ex);
    }

    /// <summary>
    /// Converts collected API xls data to useable data in lists
    /// </summary>
    /// <param name="_text">Everything revieced from the API</param>
    private void ConvertAPIData(string _text)
    {
        #region delete?
        //_text = _text.Trim('\n'); //Removes all instances of "\n"

        //_text = _text.Replace("<gml:pos>", ";"); //Replaces unuseable string with ";"
        //_text = _text.Replace("</gml:pos>", ";");//Replaces unuseable string with ";"

        //string[] splitResult = _text.Split(';'); //Splits on ";"
        //List<string> coords = new List<string>();

        ////Runs through the string array
        //for (int i = 0; i < splitResult.Length; i++)
        //{
        //    if (numberArray.Contains(splitResult[i][0])) //Adds it to the list if it contains numbers on the first index
        //    {
        //        coords.Add(splitResult[i]);
        //    }
        //}

        //coords.RemoveAt(0); //Forgot to format first two values :)
        //coords.RemoveAt(0); //Forgot to format first two values :)

        //for (int i = 0; i < coords.Count; i++) //Format and add all of the routes latitudes and longtitude
        //{
        //    string[] longLat = coords[i].Split(' ');

        //    routeLatLongs.Add(new Vector2(float.Parse(longLat[1]), float.Parse(longLat[0])));
        //}
        #endregion

        if (_text.Contains("Error")) //If response contains errors
        {
            Debug.Log("Shit happende");

            RouteManager.Instance.RecalculateViaPoints(startPosition, length);
            return;
        }

        APIDataExtractor extract = new APIDataExtractor(_text); //Instances an extractor and gives it the API data

        if (extract.Data.DistanceOfRoute > 1000 * (int)length * 1.2f || extract.Data.DistanceOfRoute < 1000 * (int)length * 0.8f)
        {
            //It is too long or to short try again
            RouteManager.Instance.RecalculateViaPoints(startPosition, length);
            return;
        }

        routeLatLongs = new List<Vector2>(extract.Data.RouteLatLongs);//Instances new routeLatLongs list with data from extractor with no reference.

        //Find the tile the player stands in
        Vector2 vector = GM.LatLonToMeters(routeLatLongs[0].x, routeLatLongs[0].y);
        Vector2 tile = GM.MetersToTile(vector, zoom);

        Vector2 centerInMercator = GM.TileBounds(tile, zoom).center; //Finds the center of the tile

        //Finds the position reletive to the tile the player stands on
        for (int i = 0; i < routeLatLongs.Count; i++)
        {
            vector = GM.LatLonToMeters(routeLatLongs[i].x, routeLatLongs[i].y);

            routeInMercCoords.Add((vector - centerInMercator)); //In mercator coordinates
        }

        distance = extract.Data.DistanceOfRoute;
        estimatedTime = new TimeSpan(0,0,extract.Data.TotalTimeInSeconds);

        dataLoaded = true;
        Debug.Log("Route data loaded");
    }

    /// <summary>
    /// Tries to load data from route API again
    /// </summary>
    /// <param name="_startPos">The position of the player in the world</param>
    /// <param name="_via">The via points for the route</param>
    public void Retry(Vector2 _startPos, List<Vector2> _via)
    {
        viaLatLongs = _via;

        LoadAPIData(_startPos, _via);
    }
}