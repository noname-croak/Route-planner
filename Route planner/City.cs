using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace Route_planner
{
    public class City
    {
        public City(int id, string name, string desc, double lat, double lng)
        {
            ID = id;
            Name = name;
            Description = desc;
            c = new PointLatLng(lat, lng);
        }
        public int ID;
        public string Name;
        public string Description;
        public PointLatLng c;
    }
}
