using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace map_explore.Map.Explore {
    
    
    public class Explore {
        static int unseen = 0;
        static int seen = 1;
        static int seen_wall = 2;
        static int old_seen = 3;
        static int old_seen_wall = 4;
        static int open_space = 5;

        public static int[,] genMask(int width, int height, int[,] map, int playerX, int playerY, int rad, int conserve) {
            int[,] output = new int[width, height];
            for (int x = playerX - rad; x <= playerX + rad; x++) {
                for(int y = playerY - rad; y <= playerY + rad; y++)
                    if(Math.Sqrt(Math.Pow(playerX - x, 2) + Math.Pow(playerY - y, 2)) <= rad + conserve)
                        line(width, height, map, output, playerX, playerY, x, y);
            }

            return output;
        }
        public static int[,] intersect(int width, int height, int[,] mask_old, int[,] mask) {
            int[,] output = new int[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    if (mask[i, j] != 0)
                        output[i, j] = mask[i, j];
                    else if (mask_old[i, j] >= 1 && mask_old[i, j] < 3)
                        output[i, j] = mask_old[i, j] + 2;
                    else
                        output[i, j] = mask_old[i, j];
            return output;
        }
        public static int[,] highlight(int width, int height, int[,] mask) {
            int[,] output = new int[width, height];

            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < width - 1; y++)
                    if (mask[x, y] == 0) {
                        if (neighbors(x, y, mask) != 0)
                            output[x, y] = open_space;
                    }
                    else output[x, y] = mask[x, y];
            return output;
        }
        static int neighbors(int x, int y, int[,] mask) {
            int n = 0;
            for(int i = x - 1; i <= x + 1; i++)
                for (int j = y - 1; j <= y + 1; j++) {
                    if (i == x && j == y)
                        continue;
                    if (mask[i, j] == 1 || mask[i, j] == 3)
                        n++;
                }
            return n;
        }
        public static void line(int width, int height, int[,] map, int[,] mask, int x, int y, int px, int py) {
            if (x < 0)
                x = 0;
            else if (x >= width)
                x = width - 1;

            if (y < 0)
                y = 0;
            else if (y >= height)
                y = height - 1;

            int lwidth = Math.Abs(x - px);
            int lHeight = Math.Abs(y - py);
            int sig1 = -Math.Sign(x - px);
            int sig2 = -Math.Sign(y - py);

            for (int i = 0; i < max(lHeight, lwidth); i++ ) {
                int f1 = i;
                int f2 = (int)((float)min(lwidth, lHeight) * (float)i / (float)max(lwidth, lHeight));
                if (lHeight > lwidth) {
                    if (map[x + sig1 * f2, y + sig2 * f1] == 1)
                        mask[x + sig1 * f2, y + sig2 * f1] = seen;
                    else {
                        mask[x + sig1 * f2, y + sig2 * f1] = seen_wall;
                        break;
                    }
                }
                else {
                    if (map[x + sig1 * f1, y + sig2 * f2] == 1)
                        mask[x + sig1 * f1, y + sig2 * f2] = seen;
                    else {
                        mask[x + sig1 * f1, y + sig2 * f2] = seen_wall;
                        break;
                    }
                }
            }

        }



        //MIN OR MAX OF PAIR OF NUMBERS
        static int min(int a, int b) {
            if (a < b)
                return a;
            else return b;
        }
        static int max(int a, int b) {
            if (a > b)
                return a;
            else return b;
        }

    }
}
