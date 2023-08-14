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

namespace Route_planner
{
    public partial class Form2 : Form
    {
        // окно для ввода доп данных
        public Form2()
        {
            InitializeComponent();
        }
        public static PointLatLng start;
        public static PointLatLng end;
        public static int earlystart;
        public static int earlyend;
        public static int latestart;
        public static int lateend;
        public static int maxtime;
        public static bool gotdata;
        GMapOverlay markers;
        private void Form2_Load(object sender, EventArgs e)
        {
            markers = new GMapOverlay();
            if (!start.IsEmpty)
            {
                textBox1.Text = start.Lat.ToString();
                textBox2.Text = start.Lng.ToString();
            }
            if (!end.IsEmpty)
            {
                textBox3.Text = end.Lat.ToString();
                textBox4.Text = end.Lng.ToString();
            }
            if (earlystart > 0)
            {
                textBox5.Text = (earlystart / 60).ToString();
                var t = earlystart % 60;
                if (t < 9)
                    textBox6.Text = "0" + (earlystart % 60);
                else
                    textBox6.Text = (earlystart % 60).ToString();
            }
            if (lateend > 0)
            {
                textBox7.Text = (lateend / 60).ToString();
                var t = lateend % 60;
                if (t < 9)
                    textBox8.Text = "0" + t;
                else
                    textBox8.Text = t.ToString();
            }
            if (latestart > 0)
            {
                textBox9.Text = (latestart / 60).ToString();
                var t = latestart % 60;
                if (t < 9)
                    textBox10.Text = "0" + t;
                else
                    textBox10.Text = t.ToString();
            }
            if (earlyend > 0)
            {
                textBox11.Text = (earlyend / 60).ToString();
                var t = earlyend % 60;
                if (t < 9)
                    textBox12.Text = "0" + t;
                else
                    textBox12.Text = t.ToString();
            }
            if (maxtime > 0)
            {
                textBox13.Text = (maxtime / 60).ToString();
                var t = maxtime % 60;
                if (t < 9)
                    textBox14.Text = "0" + t;
                else
                    textBox14.Text = t.ToString();
            }
        }
        int TimeCheck(TextBox hours, TextBox mins)
        {
            int res;
            int h;
            int min;
            if (String.IsNullOrWhiteSpace(hours.Text) && String.IsNullOrWhiteSpace(mins.Text))
                return -2;
            if (Int32.TryParse(hours.Text, out h))
            {
                if (h >= 0 && h < 24)
                    res = h * 60;
                else
                    return -1;
            }
            else
                return -1;
            if (Int32.TryParse(mins.Text, out min))
            {
                if (min >= 0 && min < 60)
                    res += min;
                else
                    return -1;
            }
            else
                return -1;
            return res;
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            var centre = Form1.cities.Find(x => x.ID == Form1.ChosenCityID).c;
            Mapper.ShowMap((GMapControl)sender, centre);
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var point = gMapControl1.FromLocalToLatLng(e.X, e.Y);
                if (markers.Markers.Count > 0) //
                    markers.Markers.Clear();
                else
                    gMapControl1.Overlays.Add(markers);
                var marker = new GMarkerGoogle(new PointLatLng(point.Lat, point.Lng), GMarkerGoogleType.lightblue);
                markers.Markers.Add(marker);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (markers.Markers.Count == 1)
            {
                start = markers.Markers[0].Position;
                textBox1.Text = start.Lat.ToString();
                textBox2.Text = start.Lng.ToString();
                if (checkBox1.Checked)
                {
                    end = markers.Markers[0].Position;
                    textBox3.Text = end.Lat.ToString();
                    textBox4.Text = end.Lng.ToString();
                }
            }
            else
                MessageBox.Show("Выберите точку", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (markers.Markers.Count == 1)
            {
                end = markers.Markers[0].Position;
                textBox3.Text = end.Lat.ToString();
                textBox4.Text = end.Lng.ToString();
                if (checkBox1.Checked)
                {
                    start = markers.Markers[0].Position;
                    textBox1.Text = start.Lat.ToString();
                    textBox2.Text = start.Lng.ToString();
                }
            }
            else
                MessageBox.Show("Выберите точку", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (start.IsEmpty && !end.IsEmpty)
                {
                    start = end;
                    textBox3.Text = start.Lat.ToString();
                    textBox4.Text = start.Lng.ToString();
                }
                else if ((end.IsEmpty && !start.IsEmpty) || (!start.IsEmpty && !end.IsEmpty))
                {
                    end = start;
                    textBox3.Text = end.Lat.ToString();
                    textBox4.Text = end.Lng.ToString();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            start = new PointLatLng();
            end = new PointLatLng();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            earlystart = TimeCheck(textBox5, textBox6);
            lateend = TimeCheck(textBox7, textBox8);
            latestart = TimeCheck(textBox9, textBox10);
            earlyend = TimeCheck(textBox11, textBox12);
            maxtime = TimeCheck(textBox13, textBox14);
            if (earlystart < 0 || lateend < 0)
            {
                MessageBox.Show("Введите корректные необходимые данные", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (latestart == -1 || earlyend == -1 || maxtime == -1)
            {
                MessageBox.Show("Некорректный ввод опциональных данных", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if ((latestart != -2 && (latestart - earlystart) < 0) || (lateend - earlystart) <= 0)
            {
                MessageBox.Show("Самое позднее начало или окончание раньше самого раннего начала", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (earlyend != -2 && latestart != -2 && (earlyend - latestart > maxtime))
            {
                MessageBox.Show("Максимальная длительность маршрута меньше минимального временного окна", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                gotdata = true;
                Close();
            }
        }
    }
}
