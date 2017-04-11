using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroPass
{
    class Station
    {
        public string Name,Line;
        public Point coord;

        public Station(string inp)
        {
            this.Line = inp.Substring(0, inp.IndexOf(' '));
            this.Name = inp.Substring(inp.IndexOf(' ')+1, inp.IndexOf('\t')-inp.IndexOf(' ')-1);
            string t = inp.Substring(inp.IndexOf('\t') + 1);
            coord.X = Int32.Parse(t.Substring(0, t.IndexOf(';')));
            coord.Y = Int32.Parse(t.Substring(t.IndexOf(';') + 1));
        }
    }
}
