using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace Route_planner
{
    public class solRoute
    {
        // класс для хранения маршрутов
        public solRoute(long time, int price, List<PointLatLng> path, List<Sight> points)
        {
            Time = time;
            Price = price;
            Path = path;
            Points = points;
        }
        public long Time;
        public int Price;
        public List<PointLatLng> Path;
        public List<Sight> Points;
    }
}
