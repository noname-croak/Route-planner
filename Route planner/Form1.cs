using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using OsmSharp.IO;
using Itinero;
using Itinero.Data;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Itinero.Geo;
using Google.OrTools.LinearSolver;
using Google.OrTools.ConstraintSolver;
using Google.OrTools;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;
using System.Linq.Expressions;
using OperationsResearch;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace Route_planner
{
    public partial class Form1 : Form
    {
        // главное меню
        public Form1()
        {
            InitializeComponent();
        }
        public static List<City> cities;
        public static int ChosenCityID;
        public static solRoute sol;
        public static double soltime;
        public static List<Sight> leftsights;
        public static string ChosenDay;
        List<Sight> sights;
        List<TimeBetween> times;
        private void Form1_Load(object sender, EventArgs e)
        {
            ChosenCityID = 0;
            loadCities();
            sights = new List<Sight>();
            Mapper.Initialize();
            //Sql.InsertPictures(); использовалось при заполнении БД
        }
        public void loadCities()
        {
            cities = new List<City>();
            var d = Sql.GetData("Select * from Cities");
            foreach (DataRow r in d.Rows)
            {
                cities.Add(new City(Convert.ToInt32(r["CityID"]), r["Name"].ToString(), r["Description"].ToString(), Convert.ToDouble(r["Latitude"]), Convert.ToDouble(r["Longitude"])));
                r.Delete();
            }
            d.Dispose();
            foreach (var c in cities)
            {
                comboBox1.Items.Add(c.Name);
            }
        }
        public void LoadSights()
        {
            if ((comboBox1.SelectedIndex != -1) && (comboBox2.SelectedIndex != -1))
            {
                dataGridView1.DataSource = Sql.GetData("SELECT * from Sights WHERE CityID=" + cities.Find(x => x.Name == comboBox1.Text).ID);
                dataGridView1.Columns["SightID"].Visible = false;
                dataGridView1.Columns["CityID"].Visible = false;
                dataGridView1.Columns["Latitude"].Visible = false;
                dataGridView1.Columns["Longitude"].Visible = false;
                dataGridView1.Columns["Name"].HeaderText = "Название";
                dataGridView1.Columns["Picture"].HeaderText = "Фото";
                dataGridView1.Columns["Description"].HeaderText = "Описание";
                dataGridView1.Columns["Price"].HeaderText = "Цена (руб)";
                dataGridView1.Columns["VisitTimeMin"].HeaderText = "~Время посещения (мин)";
                dataGridView1.Columns["Rate"].HeaderText = "Оценка (из 5)";
                for (int i = 11; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].Visible = false;
                    dataGridView1.Columns[i].HeaderText = "Время работы";
                }
                int vday = 11 + comboBox2.SelectedIndex;
                ChosenDay = comboBox2.Text;
                dataGridView1.Columns[vday].Visible = true;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1[vday, i].Value == DBNull.Value)
                        dataGridView1.Rows[i].Visible = false;
                }
                for (int i = 1; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].ReadOnly = true;
                }
            }
            else
                MessageBox.Show("Должны быть выбраны город и день посещения", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public void LoadRoutes()
        {
            if (comboBox1.SelectedIndex != -1)
            {
                dataGridView2.DataSource = Sql.GetData("SELECT * from Routes WHERE CityID=" + cities.Find(x => x.Name == comboBox1.Text).ID);
                dataGridView2.Columns["RouteID"].Visible = false;
                dataGridView2.Columns["CityID"].Visible = false;
                dataGridView2.Columns["Path"].Visible = false;
                dataGridView2.Columns["Points"].Visible = false;
                dataGridView2.Columns["IsUser"].Visible = false;
                dataGridView2.Columns["Name"].HeaderText = "Название";
                dataGridView2.Columns["Description"].HeaderText = "Нить маршрута";
                dataGridView2.Columns["TimeMin"].HeaderText = "Затраты времени (мин)";
                dataGridView2.Columns["WeekDay"].HeaderText = "Дни недели";
                dataGridView2.Columns["Price"].HeaderText = "Финансовые затраты на достопримечательности (руб)";
                for (int i = 2; i < dataGridView2.Columns.Count; i++)
                {
                    dataGridView2.Columns[i].ReadOnly = true;
                }
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if ((int)dataGridView2[10, i].Value != 1)
                    {
                        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
                        dataGridViewCellStyle2.Padding = new Padding(0, 0, 1000, 0);
                        dataGridView2[1, i].Style = dataGridViewCellStyle2;
                    }
                }
            }
            else
                MessageBox.Show("Должен быть выбран город", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public void SightToList()
        {
            sights = new List<Sight>();
            int count = 0;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    if ((bool)r.Cells[0].Value)
                    {
                        count++;
                        int id = Convert.ToInt32(r.Cells[1].Value);
                        string name = r.Cells[3].Value.ToString();
                        PointLatLng p = new PointLatLng(Convert.ToDouble(r.Cells[6].Value), Convert.ToDouble(r.Cells[7].Value));
                        string time = r.Cells[11 + comboBox2.SelectedIndex].Value.ToString();
                        int visit = Convert.ToInt32(r.Cells[9].Value);
                        int price = Convert.ToInt32(r.Cells[8].Value);
                        double rate = Convert.ToDouble(r.Cells[10].Value);
                        sights.Add(new Sight(id, name, p, time, visit, price, rate, count));
                    }
                }
            }
            if (count == 0)
                MessageBox.Show("Не выбрана ни одна достопримечательность", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);           
        }
        public void FindRoute()
        {
            var watch = new Stopwatch();
            watch.Start();

            long[,] tm = new long[sights.Count + 2, sights.Count + 2];
            long[,] tw = new long[sights.Count + 2, 2];
            long[] penalties = new long[sights.Count + 2];
            //добавление старта и финиша в матрицу времени
            times = new List<TimeBetween>();
            if (!Form2.start.IsEmpty)
            {
                foreach (var s in sights)
                    times.Add(Mapper.CountTimePath(0, s.RelativeID, Form2.start, s.coordinate));
            }
            else
            {
                foreach (var s in sights)
                    times.Add(new TimeBetween(0, s.RelativeID, 0, new List<PointLatLng>()));
            }
            if (!Form2.end.IsEmpty)
            {
                foreach (var s in sights)
                    times.Add(Mapper.CountTimePath(s.RelativeID, sights.Count + 1, s.coordinate, Form2.end));
            }
            else
            {
                foreach (var s in sights)
                    times.Add(new TimeBetween(s.RelativeID, sights.Count + 1, 0, new List<PointLatLng>()));
            }
            if ((!Form2.start.IsEmpty) && (!Form2.end.IsEmpty) && (Form2.start != Form2.end))
                times.Add(Mapper.CountTimePath(0, sights.Count + 1, Form2.start, Form2.end));
            else
                times.Add(new TimeBetween(0, sights.Count + 1, 0, new List<PointLatLng>()));
            //считывание матрицы времени из БД
            sights = sights.OrderBy(x => x.ID).ToList();
            for (int i = 0; i < sights.Count; i++)
            {
                for (int j = i + 1; j < sights.Count; j++)
                {
                    var dt = Sql.GetData("SELECT * FROM SightTimeTravel WHERE FromID=" + sights[i].ID + " AND ToID=" + sights[j].ID);
                    TimeBetween t = new TimeBetween(sights[i].RelativeID, sights[j].RelativeID, Convert.ToInt32(dt.Rows[0][3]), Mapper.StringToPoints(dt.Rows[0][4].ToString()));
                    times.Add(t);
                }
            }
            //
            sights = sights.OrderBy(x => x.RelativeID).ToList();
            times = times.OrderBy(x => x.FromID).ThenBy(x => x.ToID).ToList();
            /*
            // проверка
            string ext = "";
            foreach (var t in times)
            {
                ext += t.FromID + " " + t.ToID + " " + t.Time + "\n";
            }
            MessageBox.Show(ext);
            */
            int c = 0;
            for (int i = 0; i < sights.Count + 1; i++)
            {
                for (int j = i + 1; j < sights.Count + 2; j++)
                {
                    tm[i, j] = times[c].Time;
                    tm[j, i] = times[c].Time;
                    c++;
                }
            }
            for (int i = 0; i < sights.Count + 2; i++)
            {
                for (int j = 1; j < sights.Count + 1; j++)
                {
                    if (i != j)
                        tm[i, j] += sights[j - 1].VisitTime;
                }
            }
            /*
            // проверка
            string text = "";
            for (int i = 0; i < sights.Count + 2; i++)
            {
                for (int j = 0; j < sights.Count + 2; j++)
                {
                    text += tm[i, j] + " ";
                }
                text += "\n";
            }
            MessageBox.Show(text);
            */
            tw[0, 0] = Form2.earlystart;
            tw[0, 1] = Form2.lateend;
            tw[sights.Count + 1, 0] = Form2.earlystart;
            tw[sights.Count + 1, 1] = Form2.lateend;
            if (Form2.latestart != -2)
                tw[0, 1] = Form2.latestart;
            if (Form2.earlyend != -2)
                tw[sights.Count + 1, 0] = Form2.earlyend;
            for (int i = 1; i < sights.Count + 1; i++)
            {               
                var v = sights[i - 1].TimeWin.Split('—');
                var v1 = v[0].Split(':');
                int start = Int32.Parse(v1[0]) * 60 + Int32.Parse(v1[1]);
                var v2 = v[1].Split(':');
                int end = Int32.Parse(v2[0]) * 60 + Int32.Parse(v2[1]);
                tw[i, 0] = start + sights[i - 1].VisitTime;
                tw[i, 1] = end;                
            }
            /*
            // проверка 
            string text = "";
            for (int i = 0; i < sights.Count + 2; i++)
                text += tw[i, 0] + " " + tw[i, 1] + "\n";
            MessageBox.Show(text);
            */
            foreach (var s in sights)
                penalties[s.RelativeID] = Convert.ToInt32(s.Rate * s.Rate * 50);
            /*
            string text = "";
            for (int i = 0; i < sights.Count + 2; i++)
                text += penalties[i] + "\n";
            MessageBox.Show(text);
            */

            VrpTimeWindows.DataModel data = new VrpTimeWindows.DataModel(tm, tw, 1, new int[] { 0 }, new int[] { sights.Count + 1 });
            long mt = 0;
            if (Form2.maxtime != -2)
                mt = Form2.maxtime;
            else
                mt = 1440;
            List<long[]> solution = new List<long[]>();
            try
            {
                leftsights = new List<Sight>();
                leftsights.AddRange(sights);
                solution = VrpTimeWindows.FindSolution(data, penalties, mt);
                long totatime = -1;
                int totalprice = 0;
                List<Sight> points = new List<Sight>();
                List<PointLatLng> path = new List<PointLatLng>();
                if (!Form2.start.IsEmpty)
                {
                    points.Add(new Sight(0, "Старт", Form2.start, TimeToString(solution[0][1]), 0, 0, 0, (int)solution[0][0]));
                    totatime = solution[solution.Count - 1][1] - solution[0][1];
                }
                for (int i = 1; i < solution.Count - 1; i++)
                {
                    var s = leftsights.Find(x => x.RelativeID == solution[i][0]);
                    leftsights.Remove(s);                    
                    Sight t = new Sight(s.ID, s.Name, s.coordinate, TimeToString(solution[i][1] - s.VisitTime) + "-" + TimeToString(solution[i][1]), s.VisitTime, s.Price, s.Rate, s.RelativeID);
                    points.Add(t);
                    totalprice += s.Price;
                    if (totatime == -1)
                        totatime = solution[solution.Count - 1][1] - (solution[i][1] - s.VisitTime);
                }
                if (!Form2.end.IsEmpty)
                    points.Add(new Sight(0, "Финиш", Form2.end, TimeToString(solution[solution.Count - 1][1]), 0, 0, 0, (int)solution[solution.Count - 1][0]));

                for (int i = 1; i < points.Count; i++)
                {
                    if (points[i].RelativeID > points[i - 1].RelativeID)
                        path.AddRange(times.Find(x => (x.FromID == points[i - 1].RelativeID) && (x.ToID == points[i].RelativeID)).Path);
                    else
                    {
                        var p = times.Find(x => (x.FromID == points[i].RelativeID) && (x.ToID == points[i - 1].RelativeID)).Path;
                        p.Reverse();
                        path.AddRange(p);
                    }
                }

                sol = new solRoute(totatime, totalprice, path, points);

                watch.Stop();
                soltime = watch.Elapsed.TotalSeconds;

                Form3 f3 = new Form3();
                f3.ShowDialog();                              
            }
            catch (Exception ex)
            {
                MessageBox.Show("Для введенных ограничений по времени выбрано слишком мало посещаемых объектов. Выберите больше достопримечательностей либо измените или уберите опциональные ограничения", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }                                  
        }
        public string TimeToString(long time)
        {
            long h = time / 60;
            long m = time - h * 60;
            if (m.ToString().ToCharArray().Length <= 1)
                return h + ":0" + m;
            else
                return h + ":" + m;
        }
        private void gMapControl1_Load(object sender, EventArgs e)
        {
            //Mapper.ShowMap((GMapControl)sender, new PointLatLng(55.751999, 37.617734)); использовался при проверке карты
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                ChosenCityID = cities.Find(x => x.Name == comboBox1.Text).ID;
                richTextBox1.Text = cities.Find(x => x.ID == ChosenCityID).Description;
            }          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // загрузка достопримечательностей из БД
            Form4 wait = new Form4();
            wait.Show();
            wait.Refresh();
            LoadSights();
            wait.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //загрузка маршрутов из БД
            Form4 wait = new Form4();
            wait.Show();
            wait.Refresh();
            LoadRoutes();
            wait.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //открытие окна ввода доп данных
            Form2 f2 = new Form2();
            if (comboBox1.SelectedIndex != -1)
                f2.ShowDialog();
            else
                MessageBox.Show("Выберите город", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // сохранение достопримечательностей на посещение
            SightToList();
            //Sql.InsertTimeTable(sights, ChosenCityID); использовалось при заполнении БД
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // очистка списка посещаемых достопримечательностей
            sights.Clear();
            foreach (DataGridViewRow r in dataGridView1.Rows)
                r.Cells[0].Value = null;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // запуск поиска маршрута
            if (Form2.gotdata == false)
                MessageBox.Show("Введите доп. данные", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (sights.Count < 2)
                MessageBox.Show("Всего необходимо минимум 2 достопримечательности в списке", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                FindRoute();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //чтение маршрута из файла
            gMapControl1.Overlays.Clear();
            var markers = new GMapOverlay();
            gMapControl1.Overlays.Add(markers);
            var routes = new GMapOverlay();
            gMapControl1.Overlays.Add(routes);
            
            Kml kml = new Kml();
            List<PointLatLng> route = new List<PointLatLng>();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Directory.GetCurrentDirectory() + @"\my routes";
            ofd.Filter = "kml files (*.kml) | *.kml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (var stream = ofd.OpenFile())
                {
                    KmlFile file = KmlFile.Load(stream);
                    kml = file.Root as Kml;
                }
            }
            foreach (var placemark in kml.Flatten().OfType<SharpKml.Dom.Placemark>())
            {
                if (placemark.Name != "Маршрут") 
                {
                    var p = placemark.Geometry as SharpKml.Dom.Point;
                    var marker = new GMarkerGoogle(new PointLatLng(p.Coordinate.Latitude, p.Coordinate.Longitude), GMarkerGoogleType.lightblue);
                    marker.ToolTipText += placemark.Name;
                    markers.Markers.Add(marker);
                }
                else
                {
                    var l = placemark.Geometry as LineString;
                    foreach (var c in l.Coordinates)
                    {
                        route.Add(new PointLatLng(c.Latitude, c.Longitude));
                    }
                }
            }
            var centre = route[0];
            var r = new GMapRoute(route, "path");
            routes.Routes.Add(r);
            Mapper.ShowMap(gMapControl1, centre);

        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {         
            // отображение маршрутов на карте и их удаление из БД
            if ((dataGridView2.Columns[e.ColumnIndex].Index == 0) && (e.RowIndex >= 0))
            {
                gMapControl1.Overlays.Clear();
                var markers = new GMapOverlay();
                gMapControl1.Overlays.Add(markers);
                var routes = new GMapOverlay();
                gMapControl1.Overlays.Add(routes);

                var centre = cities.Find(x => x.ID == ChosenCityID).c;
                Mapper.ShowMap(gMapControl1, centre);

                var path = Mapper.StringToPoints((string)dataGridView2[8, e.RowIndex].Value);
                var points = Mapper.StringToPoints((string)dataGridView2[9, e.RowIndex].Value);
                var s = dataGridView2[5, e.RowIndex].Value.ToString().Split("\n");
                for (int i = 0; i < points.Count; i++)
                {
                    var marker = new GMarkerGoogle(points[i], GMarkerGoogleType.lightblue);
                    marker.ToolTipText = s[i];
                    markers.Markers.Add(marker);
                }
                var r = new GMapRoute(path, "path");
                routes.Routes.Add(r);
            }
            else if ((dataGridView2.Columns[e.ColumnIndex].Index == 1) && (e.RowIndex >= 0))
            {
                Sql.DeleteRoute((int)dataGridView2[2, e.RowIndex].Value);
                MessageBox.Show("Удаление прошло успешно, обновите таблицу, чтобы увидеть результат", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // сброс
            sol = null;
            soltime = 0;
            sights = new List<Sight>();
            leftsights = new List<Sight>();
            times = new List<TimeBetween>();
            cities = new List<City>();
            ChosenCityID = 0;
            ChosenDay = null;            
            sights = new List<Sight>();
            comboBox1.Items.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            richTextBox1.Text = "";
            dataGridView1.DataSource = null;
            dataGridView1.Refresh();
            dataGridView2.DataSource = null;
            dataGridView2.Refresh();
            gMapControl1.Overlays.Clear();
            gMapControl1.Refresh();
            Form2.start = new PointLatLng();
            Form2.end = new PointLatLng();
            Form2.earlystart = -2;
            Form2.lateend = -2;
            Form2.earlyend = -2;
            Form2.latestart = -2;
            Form2.maxtime = -2;
            Form2.gotdata = false;

            loadCities();
        }
    }   
}