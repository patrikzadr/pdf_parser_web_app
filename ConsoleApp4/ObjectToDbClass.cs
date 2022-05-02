using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    internal class ObjectToDbClass
    {
        public string ConnectionString { get; set; }
        public int Line { get; set; }
        public List<Tuple<StringBuilder, int, int>> Stations { get; set; }
        public List<List<int>> Times { get; set; }

        public async Task ObjectToDbAsync()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                foreach (Tuple<StringBuilder, int, int> station in Stations)
                {
                    string name = station.Item1.ToString();
                    int delay = station.Item2;
                    int zone = station.Item3;

                    await connection.ExecuteAsync("INSERT INTO Stations (Line, Name, Delay, Zone) VALUES (@Line, @Name, @Delay, @Zone)",
                        new { Line, Name = name.Truncate(29), Delay = delay, Zone = zone });
                }

                for (int i = 0; i < Times.Count; i++)
                {
                    foreach (int time in Times[i])
                    {
                        await connection.ExecuteAsync("INSERT INTO Times (Line, Hour, Minute) VALUES (@Line, @Hour, @Minute)",
                            new { Line, Hour = i, Minute = time });
                    }
                }
            }

            Console.WriteLine($"Line {Line} saved to database");
        }

        public async static Task PrepareDbAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                await connection.ExecuteAsync("DROP TABLE IF EXISTS Stations");
                await connection.ExecuteAsync("CREATE TABLE Stations (Line int, Name nvarchar(30), Delay int, Zone int)");

                await connection.ExecuteAsync("DROP TABLE IF EXISTS Times");
                await connection.ExecuteAsync("CREATE TABLE Times (Line int, Hour int, Minute int)");
            }
        }
    }
}