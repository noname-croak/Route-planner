using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using Itinero;
using Itinero.Osm.Vehicles;
using Itinero.Geo;
using SharpKml.Engine;

namespace Route_planner
{
    public class Mapper
    {
        // класс для взаимодействия с картами
        static Router router;
        public static void Initialize()
        {
            var routerDb = new RouterDb();
            /*
            // использовалось при распаковке osm карты региона
            using (var stream = new FileInfo(@"map\central-fed-district-latest.osm.pbf").OpenRead())
            {
                // create the network for cars only.
                routerDb.LoadOsmData(stream, Vehicle.Car, Vehicle.Pedestrian);
                //routerDb.AddSupportedVehicle(Vehicle.Pedestrian);
                //routerDb.AddSupportedVehicle(Vehicle.Car);

            }

            // write the routerdb to disk.
            using (var stream = new FileInfo(@"map\central-fed-district-latest.routerdb").Open(FileMode.Create))
            {
                routerDb.Serialize(stream);
            }
            */

            using (var stream = new FileInfo(@"map\central-fed-district-latest.routerdb").Open(FileMode.Open))
            {
                routerDb = RouterDb.Deserialize(stream);
            }
            router = new Router(routerDb);
        }
        public static void ShowMap(GMapControl g, PointLatLng p)
        {
            g.MapProvider = GoogleMapProvider.Instance;
            g.MinZoom = 4; 
            g.MaxZoom = 16; 
            g.Zoom = 13;
            g.Position = p;
            g.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            g.CanDragMap = true;
            g.DragButton = MouseButtons.Left;
            g.ShowCenter = false;
            g.ShowTileGridLines = false;
        }
        public static TimeBetween CountTimePath(int fromid, int toid, PointLatLng from, PointLatLng to)
        {
            var profile = Vehicle.Pedestrian.Shortest();
            var start = router.Resolve(profile, Convert.ToSingle(from.Lat), Convert.ToSingle(from.Lng));
            var end = router.Resolve(profile, Convert.ToSingle(to.Lat), Convert.ToSingle(to.Lng));
            var route = router.Calculate(profile, start, end);
            var t = route.TotalTime;
            int min = (int)Math.Round(t / 60);
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var it in route.Shape)
            {
                var p = new PointLatLng(Convert.ToDouble(it.Latitude), Convert.ToDouble(it.Longitude));
                points.Add(p);
            }
            TimeBetween time = new TimeBetween(fromid, toid, min, points);
            return time;
        }
        public static string PointsToString(List<PointLatLng> path)
        {
            string text = "";
            foreach (var point in path)
            {
                text += point.Lat + " " + point.Lng + ";";
            }
            return text;
        }
        public static List<PointLatLng> StringToPoints(string text)
        {
            var points = new List<PointLatLng>();
            var t = text.Split(';');
            for (int i = 0; i < t.Length - 1; i++)
            {
                var coor = t[i].Split(' ');
                points.Add(new PointLatLng(double.Parse(coor[0]), double.Parse(coor[1])));
            }
            return points;
        }
    }
}
