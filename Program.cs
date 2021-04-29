using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FaaMvaToSectorFile
{
    class Program
    {
        private static double mCenterLat;
        private static double mCenterLon;
        private static string mColorKey;
        private static string mMapName;

        static void Main(string[] args)
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

                List<string> interiorCoords = new List<string>();
                if (member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                    .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                    .PolygonPatch.interior != null)
                {
                    foreach (var interior in member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                        .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                        .PolygonPatch.interior)
                    {
                        var tmp = interior.LinearRing.posList.Split(' ');
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

                List<string> interiorCoords = new List<string>();
                if (member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                    .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                    .PolygonPatch.interior != null)
                {
                    foreach (var interior in member.Airspace.timeSlice.AirspaceTimeSlice1.geometryComponent
                        .AirspaceGeometryComponent.theAirspaceVolume.AirspaceVolume.horizontalProjection.Surface.patches
                        .PolygonPatch.interior)
                    {
                        var tmp = interior.LinearRing.posList.Split(' ');
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
            if (!string.IsNullOrEmpty(mvaLabel))
            {
                var poly = PolyLabel.GetPolyLabel(polygonPoints);
                var x = poly.Centroid.X;
                var y = poly.Centroid.Y;

                var scale = averageCellSize;
                var xpos = x - (mvaLabel.Length - 1) * 20 * scale;

                foreach (var digit in mvaLabel.ToCharArray())
                {
                    switch (digit)
                    {
                        case '0':
                            GetCharacterPoints(HersheyFont.Zero, xpos, y, scale, sb);
                            break;
                        case '1':
                            GetCharacterPoints(HersheyFont.One, xpos, y, scale, sb);
                            break;
                        case '2':
                            GetCharacterPoints(HersheyFont.Two, xpos, y, scale, sb);
                            break;
                        case '3':
                            GetCharacterPoints(HersheyFont.Three, xpos, y, scale, sb);
                            break;
                        case '4':
                            GetCharacterPoints(HersheyFont.Four, xpos, y, scale, sb);
                            break;
                        case '5':
                            GetCharacterPoints(HersheyFont.Five, xpos, y, scale, sb);
                            break;
                        case '6':
                            GetCharacterPoints(HersheyFont.Six, xpos, y, scale, sb);
                            break;
                        case '7':
                            GetCharacterPoints(HersheyFont.Seven, xpos, y, scale, sb);
                            break;
                        case '8':
                            GetCharacterPoints(HersheyFont.Eight, xpos, y, scale, sb);
                            break;
                        case '9':
                            GetCharacterPoints(HersheyFont.Nine, xpos, y, scale, sb);
                            break;
                    }

                    xpos += (20 * scale);
                }
            }
        }

        private static void GetCharacterPoints(int[] characterArray, double xPos, double yPos, double scale,
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
                var x3 = i == 0 ? (xPos + temp[i].X * scale) : (xPos + temp[i - 1].X * scale);
                var y3 = i == 0 ? (yPos + (-1) * temp[i].Y * scale) : (yPos + (-1) * temp[i - 1].Y * scale);
                var pt1 = GeoUtils.PixelsToGeo(x3, y3, mCenterLat, mCenterLon);
                var lat3 = pt1.Lat.ToDMS();
                var lon3 = pt1.Lon.ToDMS();

                var x4 = i == 0 ? (xPos + temp[i + 1].X * scale) : (xPos + temp[i].X * scale);
                var y4 = i == 0 ? (yPos + (-1) * temp[i + 1].Y * scale) : (yPos + (-1) * temp[i].Y * scale);
                var pt2 = GeoUtils.PixelsToGeo(x4, y4, mCenterLat, mCenterLon);
                var lat4 = pt2.Lat.ToDMS();
                var lon4 = pt2.Lon.ToDMS();

                stringBuilder.AppendLine($"{new string(' ', 26)}{lat3} {lon3} {lat4} {lon4} {mColorKey}");
            }
        }

        private static void ReverseArray(IList<string> array, int splitLength = 2)
        {
            // Flips the coordinate arrays because we consume them as y,x/lon,lat but
            // we want them as x,y/lat,lon

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