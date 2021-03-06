﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Helpers
{
    public static class Extensions
    {
        public static int ManhattanTo(this Vector2 v, Vector2 t)
        {
            return (int)Math.Abs(v.x - t.x) + (int)Math.Abs(v.y - t.y);
        }

        public static Vector2 ToVector2xz(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 ToVector3xz(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector2 LatLonToTile(this Vector2 v, int zoom)
        {
            Vector2 p = new Vector2();
            p.x = (float)((v.x + 180.0) / 360.0 * (1 << zoom));
            p.y = (float)((1.0 - Math.Log(Math.Tan(v.y * Math.PI / 180.0) +
                1.0 / Math.Cos(v.y * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        public static Vector2 TileToLatLon(this Vector2 v, int zoom)
        {
            Vector2 p = new Vector2();
            double n = Math.PI - ((2.0 * Math.PI * v.y) / Math.Pow(2.0, zoom));

            p.x = (float)((v.x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            p.y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
        }

        public static RoadType ToRoadType(this string s)
        {
            switch (s)
            {
                case "highway":
                    return RoadType.HIGHWAY;
                case "major_road":
                    return RoadType.MAJORROAD;
                case "minor":
                    return RoadType.MINORROAD;
                case "rail":
                    return RoadType.RAIL;
                case "path":
                    return RoadType.PATH;
            }

            return RoadType.PATH;
        }

        public static float ToWidthFloat(this RoadType s)
        {
            switch (s)
            {
                case RoadType.HIGHWAY:
                    return 10;
                case RoadType.MAJORROAD:
                    return 5;
                case RoadType.MINORROAD:
                    return 3;
                case RoadType.RAIL:
                    return 3;
                case RoadType.PATH:
                    return 2;
            }

            return 2;
        }
    }
}
