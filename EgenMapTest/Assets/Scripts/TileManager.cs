﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UniRx;

public class TileManager : MonoBehaviour
{ //Is gonna be a superclass later

    private readonly string mapzenURL = "https://tile.mapzen.com/mapzen/vector/v1/{0}/{1}/{2}/{3}.{4}?api_key={5}"; //Changed from tut
    private readonly string mapzenKey = "mapzen-ncia6gL";
    private readonly string mapzenLayer = "buildings,roads";
    private readonly string mapzenFormat = "json";

    protected RoadFactory roadFac;
    protected BuildingFactory buildFac;
    protected Transform mapParent;

    protected bool loadImages;
    protected int zoom = 16; //Detail level
    protected int range = 1;
    protected Dictionary<Vector2, Tile> tiles;
    protected Vector2 centerTMS; //TMS (Tile Map Service)
    protected Vector2 centerInMercator; //This is distance (meter) in mercator

    public virtual void Initialize(BuildingFactory _buildFac, RoadFactory _roadFac, WorldMap.Settings _settings)
    {
        Vector2 vector = GM.LatLonToMeters(_settings.latitude, _settings.longtitude); //Converts latitude and longtitude to xy cordinates in meters
        Vector2 tile = GM.MetersToTile(vector, _settings.detailLevel); //Gives a tile based on xy cordinates

        mapParent = new GameObject("Map").transform;
        mapParent.SetParent(transform, false);

        roadFac = _roadFac;
        buildFac = _buildFac;
        tiles = new Dictionary<Vector2, Tile>();
        centerTMS = tile;
        zoom = _settings.detailLevel;
        centerInMercator = GM.TileBounds(centerTMS, zoom).center;
        range = _settings.range;
        loadImages = _settings.loadImages;

        //Makes red dot
        GameObject gm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gm.transform.position = new Vector3(0, 50, 0);
        gm.GetComponent<MeshRenderer>().material.color = new Color(255, 0, 0, 255);
        gm.transform.localScale = new Vector3(50, 50, 50);
        print("X: " + centerTMS.x + " Y: " + centerTMS.y + "\nX: " + centerInMercator.x + " Y: " + centerInMercator.y);
        //End Makes red dot

        LoadTiles(centerTMS, centerInMercator);
    }

    protected void LoadTiles(Vector2 _tileTMS, Vector2 _centerInMercator)
    {
        for (int i = -range; i <= range; i++)
        {
            for (int j = -range; j <= range; j++) //Runs throught the range around the tile
            {
                Vector2 vec = new Vector2(_tileTMS.x + i, _tileTMS.y + j); //Neighbour tile
                if (tiles.ContainsKey(vec)) //If the tile already has been found, look again
                    continue;
                StartCoroutine(CreateTile(vec, _centerInMercator)); //Starts a coroutine to create the tile
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_tileTMS">The new Tiles vector location</param>
    /// <param name="_centerInMercator">The center in x/y meters</param>
    /// <returns></returns>
    protected virtual IEnumerator CreateTile(Vector2 _tileTMS, Vector2 _centerInMercator)
    {
        Rect rect = GM.TileBounds(_tileTMS, zoom); //The new Tile bounds
        Tile tile = new GameObject("Tile " + _tileTMS.x + "-" + _tileTMS.y).AddComponent<Tile>().Initialize(buildFac ,roadFac, //Creates the tile with the settings
            new Tile.Settings()
            {
                Zoom = zoom,
                TileTMS = _tileTMS,
                TileCenter = rect.center,
                LoadImages = loadImages
            });

        tiles.Add(_tileTMS, tile); //Adds the tile to the dictionary
        tile.transform.SetParent(mapParent, true); //Sets the map parent as the tile's paren, but the tile keeps it's position in the world
        tile.transform.position = (rect.center - centerInMercator).ToVector3xz(); //Moves the tile to the right position, and makes the y coordinate the z coordinate
        LoadTile(_tileTMS, tile);

        yield return null;
    }

    private void LoadTile(Vector2 _tileTMS, Tile _tile)
    {
        string url = string.Format(mapzenURL, mapzenLayer, zoom, _tileTMS.x, _tileTMS.y, mapzenFormat, mapzenKey); //Formats the mapzen url with parameters
        ObservableWWW.Get(url) //Third party code, meant for making task threaded
            .Subscribe(
            _tile.ConstructTile, //succes
            exp => Debug.Log("Error fetching -> " + url)); //Error
    }
}