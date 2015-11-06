using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using map_explore.Map;
using map_explore.Display;
using map_explore.Display.File;

namespace map_explore
{
    class Program
    {
        static void Main (string[] args) {
            Map.Map map = null;

            //map = new Map.Map(75, 75);
            map = new Map.Map(235, 57);

            Display.File.WriteFile.write_human(map, "output.txt");

            Process.Start(@"output.txt");
        }
    }
}
