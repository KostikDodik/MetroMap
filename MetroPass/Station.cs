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
        public string Name{get; set;}
        public string Line { get; set; }
        public Point coord;

        public Station(string inp)
        {
            this.Line = inp.Substring(0, inp.IndexOf(' '));
            this.Name = inp.Substring(inp.IndexOf(' ') + 1, inp.IndexOf('\t') - inp.IndexOf(' ') - 1);
            string t = inp.Substring(inp.IndexOf('\t') + 1);
            this.coord.X = Int32.Parse(t.Substring(0, t.IndexOf(';')));
            coord.Y = Int32.Parse(t.Substring(t.IndexOf(';') + 1));
        }
        public Station(string Line, string Name, Point p)
        {
            this.Line = Line;
            this.Name = Name;
            coord = p;
        }
        public override string ToString()
        {
            return string.Format("{0} {1}\t{2};{3}", Line, Name, coord.X, coord.Y);
        }
    }
}
