using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using map_explore.Map.Generation;

namespace map_explore.Map
{
    public class Map
    {
        /// <summary>
        /// Width of the map, in integers
        /// </summary>
        public int Width {
            get { return width; }
        }

        /// <summary>
        /// Height of the map, in integers
        /// </summary>
        public int Height {
            get { return height; }
        }

        protected int width;
        protected int height;

        public int[,] map;

        public Map (int width, int height, int[,] mod) {
            if (width < 1 || height < 1)
                throw new Exception("Dim <= 0");
            this.map = mod;
            this.width = width;
            this.height = height;
        }
        public Map (int width, int height) {
            if (width < 1 || height < 1)
                throw new Exception("Dim <= 0");
            this.map = map_explore.Map.Generation.Master.genBase(width, height);
            this.width = width;
            this.height = height;
        }
    }
}
