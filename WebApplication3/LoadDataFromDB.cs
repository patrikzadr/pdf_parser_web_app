using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;

namespace WebApplication3
{
    public class LoadDataFromDb
    {
        private string ConnectionString { get; }
        public LoadDataFromDb()
        {
            // It would be cool to have relative path here, but program is launched from
            // IIS directory, not from working directory
            ConnectionString =
                File.ReadAllText(@"C:\Users\Yunani\source\repos\WebApplication3\WebApplication3\.ConnectionString.txt");

        }
        public List<Tuple<string, int, int>> LoadStations(int line)
        {
            List<Tuple<string, int, int>> stations = new List<Tuple<string, int, int>>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT * FROM Stations WHERE Line={line}", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(1);
                    int delay = reader.GetInt32(2);
                    int zone = reader.GetInt32(3);

                    stations.Add(new Tuple<string, int, int>(name, delay, zone));
                }
            }

            return stations;
        }

        public List<int> LoadLines()
        {
            List<int> lines = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT DISTINCT Line FROM Stations", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lines.Add(reader.GetInt32(0));
                }
            }

            return lines;
        }

        public List<Tuple<int, List<int>>> LoadTimes(int line, bool direction)
        {
            List<Tuple<int, List<int>>> times = MyInit();
            
            for (int i = 0; i < 24; i++)
            {
                times.Add(new Tuple<int, List<int>>(i, new List<int>()));
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT Hour, Minute FROM Times WHERE Line={line}", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int hour = reader.GetInt32(0);
                    int minute = reader.GetInt32(1);
                    times[hour].Item2.Add(minute);
                }
            }

            return times;
        }

        public List<Tuple<int, List<int>>> MyInit()
        {
            // I didn't found any appropriate constructor
            List<Tuple<int, List<int>>> list = new List<Tuple<int, List<int>>>(24);
            for (int i = 0; i < 24; i++)
            {
                list.Add(new Tuple<int, List<int>>(i, new List<int>()));
            }
            return list;
        }

        public int LoadDelay(int line, int station)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT Delay FROM Stations WHERE Line={line}", connection);
                SqlDataReader reader = command.ExecuteReader();

                List<int> delays = new List<int>();

                while (reader.Read())
                {
                    delays.Add(reader.GetInt32(0));
                }

                return delays[station];
            }
        }
    }
}