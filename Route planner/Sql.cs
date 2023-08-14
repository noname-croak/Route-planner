using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace Route_planner
{
    public class Sql
    {
        // класс для взаимодействия с БД
        public static string connectionString = "Server=localhost\\SQLEXPRESS;Database=tourism;Trusted_Connection=True;";
        public static DataTable GetData(string q)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(q, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
        public static void InsertPictures()
        {
            for (int i = 18; i < 36; i++)
            {
                Image img = Image.FromFile("дост\\sight" + i + ".jpg");
                double r = (double)(img.Height) / (double)(img.Width);
                img = ResizeImage(img, 200, (int)Math.Round(200 * r));
                byte[] kek = ImgToByte(img);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    SqlCommand command = new SqlCommand("UPDATE Sights SET Picture = @Picture where SightID=" + i, connection);
                    command.Parameters.AddWithValue("@Picture", kek);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void InsertTimeTable(List<Sight> sights, int id)
        {
            List<TimeBetween> t = new List<TimeBetween>();
            for (int i = 1; i <= sights.Count; i++)
            {
                for (int j = i + 1; j <= sights.Count; j++)
                {
                    var from = sights.Find(x => x.ID == i + 17); //!
                    var to = sights.Find(x => x.ID == j + 17);//!
                    var b = Mapper.CountTimePath(from.ID, to.ID, from.coordinate, to.coordinate);
                    t.Add(b);
                }
            }
            foreach (var r in t)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO SightTimeTravel VALUES (@cityid, @fromid, @toid, @time, @path)", connection);
                    command.Parameters.AddWithValue("@cityid", id);
                    command.Parameters.AddWithValue("@fromid", r.FromID);
                    command.Parameters.AddWithValue("@toid", r.ToID);
                    command.Parameters.AddWithValue("@time", r.Time);
                    command.Parameters.AddWithValue("@path", Mapper.PointsToString(r.Path));
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void InsertRoute(solRoute r, string name, string desc, bool isuser, int id, string day)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                List<PointLatLng> p = new List<PointLatLng>();
                foreach (var s in r.Points)
                    p.Add(s.coordinate);

                SqlCommand command = new SqlCommand("INSERT INTO Routes VALUES (@cityid, @name, @desc, @time, @pr, @path, @points, @user, @day)", connection);
                command.Parameters.AddWithValue("@cityid", id);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@desc", desc);
                command.Parameters.AddWithValue("@time", (int)r.Time);
                command.Parameters.AddWithValue("@pr", r.Price);
                command.Parameters.AddWithValue("@path", Mapper.PointsToString(r.Path));
                command.Parameters.AddWithValue("@points", Mapper.PointsToString(p));
                if (isuser)
                    command.Parameters.AddWithValue("@user", 1);
                else
                    command.Parameters.AddWithValue("@user", 0);
                command.Parameters.AddWithValue("@day", day);
                command.ExecuteNonQuery();
            }
        }
        public static void DeleteRoute(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                SqlCommand command = new SqlCommand("DELETE FROM Routes where RouteID=" + id, connection);
                command.ExecuteNonQuery();
            }
        }
        public static byte[] ImgToByte(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                img.Dispose();
                return ms.ToArray();
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}
