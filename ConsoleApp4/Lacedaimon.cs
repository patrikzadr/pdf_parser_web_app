using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    internal class Lacedaimon
    {
        public string ConnectionString { get; private set; }
        public string Directory { get; private set; }

        // Constructor is private, because object should be constructed via Factory method
        private Lacedaimon() { }

        public static async Task<Lacedaimon> LacedaimonFactoryAsync()
        {
            Lacedaimon lacedaimon = new Lacedaimon()
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString,
                Directory = ConfigurationManager.AppSettings["directory"]
            };

            await ObjectToDbClass.PrepareDbAsync(lacedaimon.ConnectionString);

            return lacedaimon;
        }

        public void ProcessFiles()
        {
            string[] files = ConfigurationManager.AppSettings["files"].Split(';');
            Thread[] threads = new Thread[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                // Lambdas in C# capture by reference, so I have to copy value myself
                int copy = i;
                Console.WriteLine($"Thread {i} started");

                threads[i] = new Thread(new ThreadStart(
                    async () => await ProcessSingleFile(ConnectionString, Path.Combine(Directory, files[copy]), copy + 1)));
                threads[i].Start();
            }

            for (int i = 0; i < files.Length; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("Done! Press Enter to continue...");

            Console.ReadLine();
        }

        private async static Task ProcessSingleFile(string connectionString, string path, int line)
        {
            PdfToObjectClass item = new PdfToObjectClass
            {
                ConnectionString = connectionString,
                Path = path,
                Line = line
            };

            await item.PdfToObject()
                .ObjectToDbAsync();
        }
    }
}