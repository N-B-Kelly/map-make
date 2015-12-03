using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettlersEngine;

namespace map_explore.Algorithm
{
    public class Defgrid : IPathNode<Object>
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Boolean IsWall { get; set; }

        public bool IsWalkable (Object unused) {
            return !IsWall;
        }

        public Defgrid (int x, int y, bool wall) {
            X = x;
            Y = y;
            IsWall = wall;
        }

        public static Defgrid[,] initGrid (int[,] map, int[,] lightmap, int width, int height) {
            Defgrid[,] grid = new Defgrid[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    Boolean wall = (map[x, y] == 0 || lightmap[x, y] == 0);
                    grid[x, y] = new Defgrid(x, y, wall);
                }
            return grid;
        }
    }
}
