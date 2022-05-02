using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace WebApplication3
{
    public partial class WebForm1 : Page
    {
        private LoadDataFromDb DBHandler { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            WebForm1 form = (WebForm1)sender;

            form.DBHandler = new LoadDataFromDb();

            if (ViewState["line"] == null)
            {
                ViewState["line"] = 1;
            }
            if (ViewState["start"] == null)
            {
                ViewState["start"] = 0;
            }
            if (ViewState["direction"] == null)
            {
                ViewState["direction"] = false;
            }

            foreach (int line in DBHandler.LoadLines())
            {
                Button button = new Button
                {
                    Text = line.ToString(),
                    ID = $"Line{line}"
                };
                button.Click += (_, __) =>
                {
                    SwitchAtribut("line", line);
                    SwitchAtribut("start", 0);
                    Update("direction", false);
                };
                form1.Controls.Add(button);
            }

            Button changeDir = new Button
            {
                Text = "Změnit směr"
            };
            changeDir.Click += (_, __) =>
            {
                Update("direction", !(bool)ViewState["direction"]);
            };
            form1.Controls.Add(changeDir);

            LiteralControl twoSpaces = new LiteralControl()
            {
                Text = "<br><br>"
            };
            form1.Controls.Add(twoSpaces);

            Table stations = new Table()
            {
                ID = "stations"
            };
            form1.Controls.Add(stations);

            LiteralControl space = new LiteralControl()
            {
                Text = "<br>"
            };
            form1.Controls.Add(space);

            Table times = new Table()
            {
                ID = "times"
            };
            form1.Controls.Add(times);

            form.ShowStations();
            form.ShowTimes();
        }

        protected void ShowStations()
        {
            int line = (int)ViewState["line"];
            int start = (int)ViewState["start"];
            bool direction = (bool)ViewState["direction"];
            Table table = form1.Controls.OfType<Table>().Where(item => item.ID == "stations").First();

            table.BorderColor = Color.Black;
            table.BorderWidth = 1;
            table.CellPadding = 3;
            table.CellSpacing = 0;
            table.Rows.Clear();

            TableCell delayHeader = new TableCell()
            {
                Text = "čas"
            };
            delayHeader.Style.Add("border-bottom", "1px solid black");
            TableCell stationHeader = new TableCell()
            {
                Text = "zastávky"
            };
            stationHeader.Style.Add("border-bottom", "1px solid black");
            TableCell zoneHeader = new TableCell()
            {
                Text = "zóna"
            };
            zoneHeader.Style.Add("border-bottom", "1px solid black");

            TableRow header = new TableRow()
            {
                Cells = { delayHeader, stationHeader, zoneHeader }
            };
            table.Rows.Add(header);

            List<Tuple<string, int, int>> stations = DBHandler.LoadStations(line);

            if (direction)
            {
                stations.Reverse();
                start = stations.Count - start - 1;
            }

            for (int i = 0; i < stations.Count; i++)
            {
                Tuple<string, int, int> station = stations[i];
                int newDelay = i < start ? -1 : Math.Abs(station.Item2 - stations[start].Item2);

                TableCell delayCell = new TableCell()
                {
                    Text = $"<div style=\"text-align:right\">{(newDelay < 0 ? "" : newDelay.ToString())}</div>"
                };
                TableCell nameCell = new TableCell();
                TableCell zoneCell = new TableCell()
                {
                    Text = $"<div style=\"text-align:right;color:green\">{station.Item3}</div>"
                };

                LinkButton button = new LinkButton
                {
                    ID = $"Station{i}",
                    Text = station.Item1
                };

                if (i == 0 || i == start || i == stations.Count - 1)
                {
                    button.Text = $"<b>{button.Text}</b>";
                }
                if (i == stations.Count - 1)
                {
                    button.Enabled = false;
                }

                int copy = i;
                button.Click += (_, __) =>
                {
                    Update("start", direction ? stations.Count - copy - 1 : copy);
                };
                
                nameCell.Controls.Add(button);

                TableRow row = new TableRow()
                {
                    Cells = { delayCell, nameCell, zoneCell }
                };

                if (i % 2 == 0)
                {
                    row.BackColor = Color.LightGray;
                }

                table.Rows.Add(row);
            }

        }

        protected void ShowTimes()
        {
            int line = (int)ViewState["line"];
            int start = (int)ViewState["start"];
            bool direction = (bool)ViewState["direction"];
            int delay = DBHandler.LoadDelay(line, start);
            Table table = form1.Controls.OfType<Table>().Where(item => item.ID == "times").First();
            table.Rows.Clear();

            table.BorderColor = Color.Black;
            table.BorderWidth = 1;
            table.CellPadding = 3;
            table.CellSpacing = 0;

            List<Tuple<int, List<int>>> newTimes = DBHandler.MyInit();
            foreach (Tuple<int, List<int>> hour in DBHandler.LoadTimes(line, direction))
            {
                foreach (int minute in hour.Item2)
                {
                    if (minute + delay < 60)
                    {
                        newTimes[hour.Item1].Item2.Add(minute + delay);
                    }
                    else
                    {
                        newTimes[hour.Item1 + 1].Item2.Add(minute + delay - 60);
                    }
                }
            }

            int maxTimes = newTimes.Select(time => time.Item2.Count).Max();

            for (int i = 0; i < newTimes.Count; i++)
            {
                Tuple<int, List<int>> time = newTimes[i];
                int hour = time.Item1;
                TableCell cell = new TableCell
                {
                    Text = $"<div style=\"text-align:right\"><b>{hour}:</b></div>",
                };
                cell.Style.Add("border-right", "1px solid black");

                TableRow row = new TableRow();
                if (i % 2 == 0)
                {
                    row.BackColor = Color.LightGray;
                }
                row.Cells.Add(cell);

                row.Cells.AddRange(
                    time
                    .Item2
                    .OrderBy(number => number)
                    .Select(minute =>
                    {
                        TableCell minuteCell = new TableCell
                        {
                            Text = minute.ToString()
                        };
                        return minuteCell;
                    })
                    .ToArray());

                for (int j = row.Cells.Count; j <= maxTimes; j++)
                {
                    row.Cells.Add(new TableCell());
                }

                table.Rows.Add(row);
            }
        }

        protected void SwitchAtribut<T>(string key, T value)
        {
            ViewState[key] = value;
        }

        protected void Update<T>(string key, T value)
        {
            // Switch atribut and then recompute table content
            SwitchAtribut(key, value);
            this.ShowStations();
            this.ShowTimes();
        }
    }
}