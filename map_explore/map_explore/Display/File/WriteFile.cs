using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace map_explore.Display.File
{
    class WriteFile
    {
        public static void write (map_explore.Map.Map map) {
            StreamWriter writer = new StreamWriter(@"output.txt");
            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++)
                    writer.Write(map.map[x, y]);
                writer.WriteLine();
            }
            writer.Close();
        }
        public static void write_human (map_explore.Map.Map map, string fname) {
            StreamWriter writer = new StreamWriter(@fname);
            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int i = map.map[x, y];
                    char c;
                    if (i == 0)
                        c = '#';
                    else
                        c = '.';
                    writer.Write(c);
                }
                writer.WriteLine();
            }
            writer.Close();
        }
    }
}
