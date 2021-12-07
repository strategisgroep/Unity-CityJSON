using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Coordinates

{
    public static class Coord
    {
        public static double Meter_to_WM_source = 1.63364; 

        // Coordinates
        // loc  = 	Local [float] used to draw in Unity (in M)
        // sm	=	Spherical Mercator [double] (Web mercator auxiliary sphere) EPSG: 3857 = EPSG:900913 (in M)
        // wgs	=	WGS84 Datum [double] : EPSG:4326 (in Degree)


        public static double centerWebMercatorX = 516000;
        public static double centerWebMercatorY = 6869000;


        //public static double centerWebMercatorX = 672131;
        //public static double centerWebMercatorY = 5707418;


        public const double locScale = 100;

        //loc to sm
        

        public static Vector3d sm_loc(Vector3d sm_v3d)
        {
            sm_v3d.x = sm_loc_x_double(sm_v3d.x);
            sm_v3d.z = sm_loc_y_double(sm_v3d.z);
            return sm_v3d;
        }
        public static double sm_loc_x_double(double sm_x)
        {
            double loc_x = (sm_x - centerWebMercatorX);
            loc_x = loc_x / locScale;
            return loc_x;
        }
        
        
        

        public static double sm_loc_y_double(double sm_y)
        {
            double loc_y = (sm_y - centerWebMercatorY);
            loc_y = loc_y / locScale;
            return loc_y;
        }

        public static double ProjectPointToWebMercatorX(double lon)
        {
            double DegreeEqualsRadians = 0.017453292519943;
            double EarthsRadius = 6378137;

            double x = lon * DegreeEqualsRadians * EarthsRadius;

            return x;
        }

        public static Vector2d WGS84toGoogleBing(double lon, double lat)
        {

            Vector2d pos = new Vector2d();

            pos.x = lon * 20037508.34 / 180;
            pos.y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            pos.y *= 20037508.34 / 180;
            return pos;
        }

        public static Vector2d RDtoWGS84(double X, double Y)
        {

            double dX;
            double dY;
            double SomN;
            double SomE;

            Vector2d latLon = new Vector2d();

            dX = (X - 155000) * Math.Pow(10, -5);
            dY = (Y - 463000) * Math.Pow(10, -5);

            SomN = (3235.65389 * dY) + (-32.58297 * dX * dX) + (-0.2475 * dY * dY) + (-0.84978 * dX * dX * dY) +
                   (-0.0655 * dY * dY * dY) + (-0.01709 * dX * dX * dY * dY) + (-0.00738 * dX) +
                   (0.0053 * dX * dX * dX * dX) + (-0.00039 * dX * dX * dY * dY * dY) +
                   (0.00033 * dX * dX * dX * dX * dY) + (-0.00012 * dX * dY);
            SomE = (5260.52916 * dX) + (105.94684 * dX * dY) + (2.45656 * dX * dY * dY) + (-0.81885 * dX * dX * dX) +
                   (0.05594 * dX * dY * dY * dY) + (-0.05607 * dX * dX * dX * dY) + (0.01199 * dY) +
                   (-0.00256 * dX * dX * dX * dY * dY) + (0.00128 * dX * dY * dY * dY * dY) + (0.00022 * dY * dY) +
                   (-0.00022 * dX * dX) + (0.00026 * dX * dX * dX * dX * dX);

            latLon.y = 52.15517 + (SomN / 3600);
            latLon.x = 5.387206 + (SomE / 3600);

            return latLon;

        }
        
    }
}

