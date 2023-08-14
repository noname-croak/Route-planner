using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace Route_planner
{
    public class Sight
    {
        // класс для хранения данных о достопримечательностях
        public Sight(int id, string name, PointLatLng p, string time, int visit, int price, double rate, int relid)
        {
            ID = id;
            Name = name;
            coordinate = p;
            TimeWin = time;
            VisitTime = visit;
            Price = price;
            Rate = rate;
            RelativeID = relid;
        }
        public int ID;
        public string Name;
        public PointLatLng coordinate;
        public string TimeWin;
        public int VisitTime;
        public int Price;
        public double Rate;
        public int RelativeID;

    }
}
