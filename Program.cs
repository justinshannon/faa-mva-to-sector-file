using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FaaMvaToSectorFile
{
    internal class Program
    {
        private static double mCenterLat;
        private static double mCenterLon;
        private static string mColorKey;
        private static string mMapName;

        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Command line arguments:");
                Console.WriteLine("Path\\to\\mva.xml map_name optional:color_key");
                Console.ReadLine();
                return;
            }

            string path = args[0];
            mMapName = args[1];
            mColorKey = args.Length > 2 ? args[2] : "";

            var xml = SerializationUtils.DeserializeObjectFromFile<AIXMBasicMessage>(path);

            var sb = new StringBuilder();

            GetCenterCoordinateValues(xml);

            CalculateAveragePolylabelCellSize(xml, out double averageCellSize);

            CreatePolygonsAndText(xml, sb, averageCellSize);

            var outputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{mMapName}_{DateTime.UtcNow:yyyyMMdd_hhmmss}.txt");
            File.WriteAllText(outputPath, sb.ToString());
        }

        private static void GetCenterCoordinateValues(AIXMBasicMessage msg)
        {
            var firstMember = msg.hasMember.Select(t =>
                t.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent.AirspaceGeometryComponent.theAirspaceVolume
                    .AirspaceVolume.horizontalProjection.Surface.patches.PolygonPatch.exterior.LinearRing.posList
                    .Split(' ')).First();

            mCenterLat = double.Parse(firstMember[1]);
            mCenterLon = double.Parse(firstMember[0]);
        }

        private static void CalculateAveragePolylabelCellSize(AIXMBasicMessage msg, out double averageCellSize)
        {
            // Calculate the average polylabel cell size to "guestimate" what the font scaling value should be

            double aggregatedCellSize = 0;
            int totalTests = 0;

            foreach (var member in msg.hasMember)
            {
                var polygonTest = new List<MyPoint>();

                var coords = member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent.AirspaceGeometryComponent
                    .theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches.PolygonPatch.exterior
                    .LinearRing.posList.Split(' ');

                ReverseArray(coords);

                var interiorCoords = new List<string>();
                if (member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                    .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                    .PolygonPatch.interior != null)
                {
                    foreach (var tmp in member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                        .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                        .PolygonPatch.interior.Select(interior => interior.LinearRing.posList.Split(' ')))
                    {
                        ReverseArray(tmp);
                        interiorCoords.AddRange(tmp);
                    }
                }

                var combined = new List<LatLng>();
                if (interiorCoords.Count > 0)
                {
                    // If the polygons are overlapping, then we need to do some extra
                    // magic to ensure the mva label is placed in the correct polygon.
                    // To do this, we combine the exterior and interior coordinate groups
                    // which will be used to calculate the polylabel bounding box.
                    var combinedCoords = coords.Concat(interiorCoords).ToArray();

                    for (var i = 0; i < combinedCoords.Length; i += 2)
                    {
                        combined.Add(new LatLng(double.Parse(combinedCoords[i]), double.Parse(combinedCoords[i + 1])));
                    }

                    for (var i = 0; i < combined.Count; i++)
                    {
                        var x1 = i == 0 ? (combined[i].Lat) : (combined[i - 1].Lat);
                        var y1 = i == 0 ? (combined[i].Lon) : (combined[i - 1].Lon);

                        var geo = GeoUtils.GeoToPixels(x1, y1, mCenterLat, mCenterLon);
                        polygonTest.Add(new MyPoint(geo.X, geo.Y));
                    }
                }

                var temp = new List<LatLng>();

                for (var i = 0; i < coords.Length; i += 2)
                {
                    temp.Add(new LatLng(double.Parse(coords[i]), double.Parse(coords[i + 1])));
                }

                for (var i = 0; i < temp.Count; i++)
                {
                    var x1 = i == 0 ? (temp[i].Lat) : (temp[i - 1].Lat);
                    var y1 = i == 0 ? (temp[i].Lon) : (temp[i - 1].Lon);

                    if (interiorCoords.Count == 0)
                    {
                        var geo = GeoUtils.GeoToPixels(x1, y1, mCenterLat, mCenterLon);
                        polygonTest.Add(new MyPoint(geo.X, geo.Y));
                    }
                }

                var poly = PolyLabel.GetPolyLabel(polygonTest);
                aggregatedCellSize += poly.Radius / 100;
                totalTests++;
            }

            averageCellSize = aggregatedCellSize / totalTests;
        }

        private static void CreatePolygonsAndText(AIXMBasicMessage msg, StringBuilder sb, double averageCellSize)
        {
            sb.AppendLine(
                $"{mMapName.PadRight(26, ' ')}N000.00.00.000 W000.00.00.000 N000.00.00.000 W000.00.00.000 {mColorKey}");

            foreach (var member in msg.hasMember)
            {
                var polygonPoints = new List<MyPoint>();

                var mva = (member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent.AirspaceGeometryComponent
                    .theAirspaceVolume.AirspaceVolume.minimumLimit.Value / 100).ToString("00");

                var coords = member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent.AirspaceGeometryComponent
                    .theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches.PolygonPatch.exterior
                    .LinearRing.posList.Split(' ');

                ReverseArray(coords);

                var interiorCoords = new List<string>();
                if (member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                    .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                    .PolygonPatch.interior != null)
                {
                    foreach (var tmp in member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                        .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                        .PolygonPatch.interior.Select(interior => interior.LinearRing.posList.Split(' ')))
                    {
                        ReverseArray(tmp);
                        interiorCoords.AddRange(tmp);
                    }
                }

                var combined = new List<LatLng>();
                if (interiorCoords.Count > 0)
                {
                    // If the polygons are overlapping, then we need to do some extra
                    // magic to ensure the mva label is placed in the correct polygon.
                    // To do this, we combine the exterior and interior coordinate groups
                    // which will be used to calculate the polylabel bounding box.
                    var combinedCoords = coords.Concat(interiorCoords).ToArray();

                    for (var i = 0; i < combinedCoords.Length; i += 2)
                    {
                        combined.Add(new LatLng(double.Parse(combinedCoords[i]), double.Parse(combinedCoords[i + 1])));
                    }

                    for (var i = 0; i < combined.Count; i++)
                    {
                        var x1 = i == 0 ? (combined[i].Lat) : (combined[i - 1].Lat);
                        var y1 = i == 0 ? (combined[i].Lon) : (combined[i - 1].Lon);

                        var geo = GeoUtils.GeoToPixels(x1, y1, mCenterLat, mCenterLon);
                        polygonPoints.Add(new MyPoint(geo.X, geo.Y));
                    }
                }

                var temp = new List<LatLng>();

                for (var i = 0; i < coords.Length; i += 2)
                {
                    temp.Add(new LatLng(double.Parse(coords[i]), double.Parse(coords[i + 1])));
                }

                for (var i = 0; i < temp.Count; i++)
                {
                    var x1 = i == 0 ? (temp[i].Lat) : (temp[i - 1].Lat);
                    var y1 = i == 0 ? (temp[i].Lon) : (temp[i - 1].Lon);

                    var x2 = i == 0 ? (temp[i + 1].Lat) : (temp[i].Lat);
                    var y2 = i == 0 ? (temp[i + 1].Lon) : (temp[i].Lon);

                    if (interiorCoords.Count == 0)
                    {
                        var geo = GeoUtils.GeoToPixels(x1, y1, mCenterLat, mCenterLon);
                        polygonPoints.Add(new MyPoint(geo.X, geo.Y));
                    }

                    sb.AppendLine(
                        $"{new string(' ', 26)}{x1.ToDMS()} {y1.ToDMS()} {x2.ToDMS()} {y2.ToDMS()} {mColorKey}");
                }

                CreateMvaTextLabels(sb, averageCellSize, mva, polygonPoints);
            }
        }

        private static void CreateMvaTextLabels(StringBuilder sb, double averageCellSize, string mvaLabel,
            List<MyPoint> polygonPoints)
        {
            if (string.IsNullOrEmpty(mvaLabel))
            {
                return;
            }

            var scale = Math.Min(Math.Max(0.03f, averageCellSize), 0.5f);

            var polyLabel = PolyLabel.GetPolyLabel(polygonPoints);

            var x = polyLabel.Centroid.X - (mvaLabel.Length - 1) * 20 * scale;
            var y = polyLabel.Centroid.Y;

            foreach (var digit in mvaLabel.ToCharArray())
            {
                switch (digit)
                {
                    case '0':
                        GetCharacterLines(HersheyFont.Zero, x, y, scale, sb);
                        break;
                    case '1':
                        GetCharacterLines(HersheyFont.One, x, y, scale, sb);
                        break;
                    case '2':
                        GetCharacterLines(HersheyFont.Two, x, y, scale, sb);
                        break;
                    case '3':
                        GetCharacterLines(HersheyFont.Three, x, y, scale, sb);
                        break;
                    case '4':
                        GetCharacterLines(HersheyFont.Four, x, y, scale, sb);
                        break;
                    case '5':
                        GetCharacterLines(HersheyFont.Five, x, y, scale, sb);
                        break;
                    case '6':
                        GetCharacterLines(HersheyFont.Six, x, y, scale, sb);
                        break;
                    case '7':
                        GetCharacterLines(HersheyFont.Seven, x, y, scale, sb);
                        break;
                    case '8':
                        GetCharacterLines(HersheyFont.Eight, x, y, scale, sb);
                        break;
                    case '9':
                        GetCharacterLines(HersheyFont.Nine, x, y, scale, sb);
                        break;
                }

                x += (20 * scale);
            }
        }

        private static void GetCharacterLines(int[] characterArray, double xPos, double yPos, double scale,
            StringBuilder stringBuilder)
        {
            // Translate the font points into usable geo points for sector files

            var temp = new List<MyPoint>();

            for (var i = 0; i < characterArray.Length; i += 2)
            {
                temp.Add(new MyPoint(characterArray[i], characterArray[i + 1]));
            }

            for (var i = 0; i < temp.Count; i++)
            {
                var x1 = i == 0 ? (xPos + temp[i].X * scale) : (xPos + temp[i - 1].X * scale);
                var y1 = i == 0 ? (yPos + (-1) * temp[i].Y * scale) : (yPos + (-1) * temp[i - 1].Y * scale);
                var pt1 = GeoUtils.PixelsToGeo(x1, y1, mCenterLat, mCenterLon);
                var lat1 = pt1.Lat.ToDMS();
                var lon1 = pt1.Lon.ToDMS();

                var x2 = i == 0 ? (xPos + temp[i + 1].X * scale) : (xPos + temp[i].X * scale);
                var y2 = i == 0 ? (yPos + (-1) * temp[i + 1].Y * scale) : (yPos + (-1) * temp[i].Y * scale);
                var pt2 = GeoUtils.PixelsToGeo(x2, y2, mCenterLat, mCenterLon);
                var lat2 = pt2.Lat.ToDMS();
                var lon2 = pt2.Lon.ToDMS();

                stringBuilder.AppendLine($"{new string(' ', 26)}{lat1} {lon1} {lat2} {lon2} {mColorKey}");
            }
        }

        private static void ReverseArray(IList<string> array, int splitLength = 2)
        {
            // Flips the coordinate arrays because we consume them as [x,y] or [lon,lat] but
            // we want [x,y] or [lat,lon]

            for (var i = 0; i < array.Count; i += splitLength)
            {
                var left = i;

                var right = Math.Min(i + splitLength - 1, array.Count - 1);

                while (left < right)
                {
                    var temp = array[left];
                    array[left] = array[right];
                    array[right] = temp;
                    left += 1;
                    right -= 1;
                }
            }
        }
    }
}