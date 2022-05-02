using System.Threading.Tasks;

namespace ConsoleApp4
{
    internal class Program
    {
        public async static Task Main()
        {
            // Coz greek mythology is gut
            Lacedaimon lacedaimon = await Lacedaimon.LacedaimonFactoryAsync(); // Constructor cannot be async, so I use factory instead
            lacedaimon.ProcessFiles();
        }
    }
}