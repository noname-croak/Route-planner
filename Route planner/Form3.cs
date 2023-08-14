using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace Route_planner
{
    public partial class Form3 : Form
    {
        // окно с результатом
        public Form3()
        {
            InitializeComponent();
        }
        string desc;
        private void Form3_Load(object sender, EventArgs e)
        {
            var markers = new GMapOverlay();
            gMapControl1.Overlays.Add(markers);

            var routes = new GMapOverlay();
            gMapControl1.Overlays.Add(routes);

            var centre = Form1.cities.Find(x => x.ID == Form1.ChosenCityID).c;
            Mapper.ShowMap(gMapControl1, centre);

            int vTime = 0;
            foreach (var s in Form1.sol.Points)
            {
                vTime += s.VisitTime;
                var marker = new GMarkerGoogle(s.coordinate, GMarkerGoogleType.lightblue);
                marker.ToolTipText += s.Name;
                markers.Markers.Add(marker);
            }
            var r = new GMapRoute(Form1.sol.Path, "path");
            routes.Routes.Add(r);

            string info = "Время, затраченное на создание маршрута: " + Form1.soltime + " секунд"+ "\n";
            info += "Отношение кол-ва достопримечательностей к длительности маршрута в часах: " + (Form1.sol.Points.Count / ((double)Form1.sol.Time / 60)) + "\n";
            info += "Отношение времени посещения достопримечательностей к общему времени: " + (vTime / (double)Form1.sol.Time) + "\n" + "\n";
            info += "Нить маршрута: " + "\n";
            desc = "";
            for (int i= 0; i < Form1.sol.Points.Count - 1; i++)
                desc += Form1.sol.Points[i].Name + " " + Form1.sol.Points[i].TimeWin + "\n";
            desc += Form1.sol.Points[Form1.sol.Points.Count - 1].Name + " " + Form1.sol.Points[Form1.sol.Points.Count - 1].TimeWin;
            info += desc;
            info += "\n" + "\n";
            if (Form1.leftsights.Count > 0)
            {
                info += "В маршрут не вошли следующие объекты:" + "\n";
                foreach (var s in Form1.leftsights)
                    info += s.Name + "\n";
                info += "\n";
            }
            int h = (int)Form1.sol.Time / 60;
            int m = (int)Form1.sol.Time - h * 60;
            info += "Общая длительность маршрута: " + h + " ч. " + m + " мин. " + "\n";
            info += "Общие затраты на посещение достопримечательностей: " + Form1.sol.Price + " рублей"+ "\n";

            richTextBox1.Text = info;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var s = desc.Split("\n");
            int n = s.Length;
            var doc = new Document();
            Kml k = new Kml();
            if ((Form2.start == Form2.end) && (!Form2.start.IsEmpty) && (!Form2.end.IsEmpty))
            {
                s[0] += " / " + s[n - 1];
                n--;
            }
            for (int i = 0; i < n; i++)
            {
                var pl = new SharpKml.Dom.Placemark();
                pl.Name = s[i];
                var p = new SharpKml.Dom.Point();
                p.Coordinate = new Vector((double)Form1.sol.Points[i].coordinate.Lat, (double)Form1.sol.Points[i].coordinate.Lng);
                pl.Geometry = p;
                doc.AddFeature(pl);
            }
            var line = new SharpKml.Dom.LineString();
            var track = new CoordinateCollection();
            foreach (var c in Form1.sol.Path)
            {
                track.Add(new Vector(c.Lat, c.Lng));
            }
            line.Coordinates = track;
            var tr = new SharpKml.Dom.Placemark();
            tr.Name = "Маршрут";
            tr.Geometry = line;
            doc.AddFeature(tr);
            k.Feature = doc;
            KmlFile kml = KmlFile.Create(k, false);

            SaveFileDialog sv = new SaveFileDialog();
            sv.InitialDirectory = Directory.GetCurrentDirectory() + @"\my routes";
            sv.FileName = "Маршрут" + DateTime.Today.ToString("dd.MM.yyyy") + ".kml";
            sv.DefaultExt = "kml";
            sv.Filter = "kml files (*.kml) | *.kml";
            if (sv.ShowDialog() == DialogResult.OK)
            {
                using (var stream = sv.OpenFile())
                {
                    kml.Save(stream);
                }
                MessageBox.Show("Маршрут успешно сохранен", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }               
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                Sql.InsertRoute(Form1.sol, textBox1.Text, desc, true, Form1.ChosenCityID, Form1.ChosenDay);
                MessageBox.Show("Маршрут успешно добавлен в БД", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Дайте маршруту имя", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
