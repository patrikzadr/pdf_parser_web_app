using SautinSoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp4
{
    internal class PdfToObjectClass
    {
        public string ConnectionString { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }

        public ObjectToDbClass PdfToObject()
        {
            PdfFocus f = new PdfFocus();
            f.OpenPdf(Path);
            string text = f.ToText();
            string[] content = text.Split();
            string[] words = content.Where(word => !String.IsNullOrEmpty(word)).ToArray();

            List<Tuple<StringBuilder, int, int>> stations = new List<Tuple<StringBuilder, int, int>>();
            int zone = 0;
            List<List<int>> times = new List<List<int>>();
            int nextStation = -1;
            int currentTime = -1;
            int lastDelay = -1;
            bool numberRead = false;
            bool lastStation = false;

            for (int index = 0; index < words.Length; index++)
            {
                bool parsed = Int32.TryParse(words[index], out int parsedValue);

                if (words[index] == "PLATÍ" || words[index] == "NEPLATÍ" || words[index] == "Ú" ||
                    words[index] == "SOBOTA" || words[index] == "NEDĚLE" || words[index] == "M" ||
                    words[index] == "TAKÉ" || words[index] == "VE" || words[index] == "DNECH" ||
                    words[index] == "STÁTEM" || words[index] == "UZNANÝCH" || words[index] == "SVÁTKŮ" ||
                    words[index] == "o" || words[index] == "zastávka" || words[index] == "od" ||
                    words[index] == "do" || words[index] == "na" || words[index] == "znamení" ||
                    words[index] == "hodin" || words[index] == "z" || words[index] == "w" ||
                    words[index] == "Trial" || words[index] == "S" || words[index] == "a" ||
                    words[index] == "přibližně" || words[index] == "o" || words[index] == "jede" ||
                    words[index] == "jen" || words[index] == "V" || words[index] == "noci" ||
                    words[index] == "až" || words[index] == "m" || words[index] == "H" || words[index] == "s" ||
                    words[index] == "noci," || words[index] == "kratší." || words[index] == "spoj" ||
                    words[index] == "bezbariérově" || words[index] == "přístupným" || words[index] == "vozidlem" ||
                    words[index] == "DPMB" || words[index] == "a.s.," || words[index] == "odjezd" ||
                    words[index] == "zastávky")
                {
                    continue;
                }
                else if (words[index] == "Zóna")
                {
                    index++;
                    zone = Int32.Parse(words[index]);
                }
                else if (!lastStation && (words[index] == "ê" || words[index][0] == 61674))
                {
                    stations.Add(new Tuple<StringBuilder, int, int>(new StringBuilder(), 0, zone));
                    nextStation = 0;
                }
                else if (parsed && !numberRead)
                {
                    numberRead = true;
                }
                else if (words[index].Any(char.IsDigit) && !words[index].Any(c => c == '.'))
                {
                    if (parsed && currentTime != 23 && parsedValue == currentTime + 1)
                    {
                        times.Add(new List<int>());
                        currentTime++;
                    }
                    else if (!lastStation && parsed && parsedValue > lastDelay && parsedValue - lastDelay < 4)
                    {
                        stations.Add(new Tuple<StringBuilder, int, int>(new StringBuilder(), parsedValue, zone));
                        lastDelay = parsedValue;
                    }
                    else if (currentTime != -1 && currentTime != 23)
                    {
                        times[currentTime].Add(Convert(words[index]));
                    }
                }
                else if (!lastStation && words[index].All(c => Char.IsLetter(c) || c == '.' || c == ',') &&
                    nextStation != -1 && nextStation != stations.Count)
                {
                    //Console.WriteLine("další zastávka");
                    for (; !words[index].Any(char.IsDigit) && words[index] != "NEPLATÍ" && words[index] != "v" &&
                        words[index] != "S" && words[index] != "a:"; index++)
                    {
                        string test = words[index];
                        stations[nextStation].Item1.Append(" " + words[index]);
                        if (words[index] == "(o)")
                        {
                            index++;
                            break;
                        }
                    }
                    nextStation++;
                    index--;
                    int lastIndex = stations.Count - 1;
                    for (; lastIndex > 0 && stations[lastIndex].Item1.Length == 0; lastIndex--) ;
                    if (stations[lastIndex].Item2 != 0 && stations[lastIndex].Item1.Length != 0 &&
                        stations[lastIndex].Item1.ToString().All(c => Char.IsUpper(c) || c == ' ' || c == ',' || c == '-'))
                    {
                        lastStation = true;
                        while (stations.Last().Item1.Length == 0)
                        {
                            stations.RemoveAt(stations.Count - 1);
                        }
                    }
                }
            }

            Console.WriteLine($"Converted line {Line}");

            return new ObjectToDbClass
            {
                ConnectionString = ConnectionString,
                Line = Line,
                Stations = stations,
                Times = times
            };
        }

        public int Convert(string input)
        {
            // I read on StackOverflow, that String.Concat is innefective
            StringBuilder result = new StringBuilder();
            foreach (char c in input.Where(char.IsDigit))
            {
                result.Append(c);
            }
            return int.Parse(result.ToString());
        }
    }
}