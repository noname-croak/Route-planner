using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace Route_planner
{
    public class TimeBetween
    {
        // хранение данных о пути между парой точек
        public TimeBetween(int from, int to, int time, List<PointLatLng> path)
        {
            FromID = from;
            ToID = to;
            Time = time;
            Path = path;
        }
        public int FromID;
        public int ToID;
        public int Time;
        public List<PointLatLng> Path;
    }
}
