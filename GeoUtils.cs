using System;

namespace FaaMvaToSectorFile
{
    public class MyPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MyPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class LatLng
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public LatLng(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }

    public static class GeoUtils
    {
        public const int NmPerDegLat = 60;
        public const int NmPerDegLon = 51;
        public const double MagVar = 0;
        public const double Width = 1920; // we just need a size to do some quick math
        public const double Height = 1080;

        public static double SecCosMag = Math.Cos(-MagVar * (Math.PI / 180));
        public static double SecSinMag = Math.Sin(-MagVar * (Math.PI / 180));
        public static double RevSecCosMag = Math.Cos(MagVar * (Math.PI / 180));
        public static double RevSecSinMag = Math.Sin(MagVar * (Math.PI / 180));

        public static string ToDMS(this double coord)
        {
            var isLat = coord >= -90 && coord <= 90;
            var nsew = (coord >= 0.0) ? (isLat ? "N" : "E") : (isLat ? "S" : "W");
            coord = Math.Abs(coord);
            var deg = Math.Floor(coord);
            var min = Math.Floor((coord - deg) * 60.0);
            var sec = (coord - deg - (min / 60.0)) * 3600.0;
            return $"{nsew}{deg:000}.{min:00}.{sec:00.000}";
        }

        public static MyPoint GeoToPixels(double lat, double lon, double centerLat, double centerLon)
        {
            var dx = (lon - centerLon) * NmPerDegLon;
            var dy = (centerLat - lat) * NmPerDegLat;

            var dx1 = (dx * SecCosMag) - (dy * SecSinMag);
            var dy1 = (dx * SecSinMag) + (dy * SecCosMag);

            var x = (dx1 + (Width / 2));
            var y = (dy1 + (Height / 2));

            return new MyPoint((float) x, (float) y);
        }

        public static LatLng PixelsToGeo(double x, double y, double centerLat, double centerLon)
        {
            var dx = x - Width / 2;
            var dy = y - Height / 2;
            var dx1 = (dx * RevSecCosMag - dy * RevSecSinMag) / NmPerDegLon;
            var dy1 = (dx * RevSecSinMag + dy * RevSecCosMag) / NmPerDegLat;
            return new LatLng(centerLat - dy1, centerLon + dx1);
        }
    }
}
