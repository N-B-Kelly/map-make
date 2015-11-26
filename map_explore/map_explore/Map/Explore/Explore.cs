using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace map_explore.Map.Explore {
    

    public class Ex
    {
        static int floor = 1;

        static int unlit = 0;
        static int lit = 1;
        static int seen = 2;

        static int force = 1; //this is just a flag
        static int slope (int x1, int y1, int x2, int y2) {
            return (x1 - x2) / (y1 - y2);
        }

        static int radius;
        static int startx;
        static int starty;
        static int width;
        static int height;
        static int[,] lightMap;
        static int[,] resMap;
        static bool conservative;


        //Checks if region is within square/circle bounds
        static bool inBounds (int dx, int dy, bool conserv, int rad) {
            if (conserv)
                return (dx * dx < rad * rad && dy * dy < rad * rad);
            else
                return (Math.Sqrt(Math.Abs(dx*dx + dy*dy)) < rad);

        }

        //Calculates field of view of map, and compares it with old map
        public static int[,] calcFOV(int[,] map, int width, int height, int radius, int startX, int startY, bool conserv, int[,] lastMap) {
            
            lightMap = new int[width, height];
            lightMap[startX, startY] = force;   //light the starting cell
            resMap = map;
            Ex.radius = radius;
            Ex.startx = startX;
            Ex.starty = startY;
            Ex.width = width;
            Ex.height = height;
            Ex.conservative = conserv;
            // TO DO
            //
            castLight(1, 1, 0, 0, 1, 1, 0);
            castLight(1, 1, 0, 1, 0, 0, 1);

            castLight(1, 1, 0, 0, -1, 1, 0);
            castLight(1, 1, 0, -1, 0, 0, 1);

            castLight(1, 1, 0, 0, -1, -1, 0);
            castLight(1, 1, 0, -1, 0, 0, -1);

            castLight(1, 1, 0, 0, 1, -1, 0);
            castLight(1, 1, 0, 1, 0, 0, -1);

            mask(width, height, lastMap);
            return lightMap;
        }

        //calculates field of view in a single quadrant
        private static void castLight (int row, float start, float end, int xx, int xy, int yx, int yy) {
            float newStart = 0;
            if (start < end)
                return;
            bool blocked = false;
            for (int distance = row; distance <= radius && !blocked; distance++) {
                int deltaY = -distance;
                for (int deltaX = -distance; deltaX <= 0; deltaX++) {
                    //get our current cells
                    int currentX = startx + deltaX * xx + deltaY * xy;
                    int currentY = starty + deltaX * yx + deltaY * yy;

                    //get out current slopes
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    //make sure we're within bounds
                    if (!(currentX >= 0 && currentY >= 0 && currentX < Ex.width && currentY < Ex.height) || start < rightSlope)
                        continue;
                    //if we are, check if we need to stop
                    else if (end > leftSlope)
                        break;

                    //check we're within the lightable area, then light if needed
                    if (inBounds(deltaX, deltaY, conservative, radius))
                        lightMap[currentX, currentY] = lit;


                    if (blocked) //our previous cell was a blocking one
                        if (resMap[currentX, currentY] == 0) {//we hit a wall
                            newStart = rightSlope;
                            continue;
                        }
                        else {
                            blocked = false;
                            start = newStart;
                        }
                    else
                        if (resMap[currentX, currentY] == 0 && distance < radius) {//hit a wall within sight line
                            blocked = true;
                            castLight(distance + 1, start, leftSlope, xx, xy, yx, yy);
                            newStart = rightSlope;
                        }
                }
            }
        }

        //calculates field of view around a single area
        static void mask (int width, int height, int[,] lastMap) {
            for(int x = 1; x < width; x++)
                for (int y = 1; y < height; y++) 
                    if (lightMap[x, y] == lit)
                        continue;
                    else if (lastMap[x, y] == lit || lastMap[x, y] == seen)
                        lightMap[x, y] = seen;
        }

        public static int[,] highlight (int width, int height, int[,] lightmap, int[,] map) {
            int[,] highlight = new int[width, height];
            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < height - 1; y++)
                    if (lightmap[x, y] == 0)
                        if (adjacent_lit_tiles(width, height, lightmap, map, x, y, floor) >= 1)
                            highlight[x, y] = 1;

            return highlight;
        }

        static int adjacent_lit_tiles (int width, int height, int[,] lightmap, int[,] map, int x, int y, int token) {
            int ct = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    if (i == 0 && j == 0)
                        continue;
                    else
                        if(lightmap[x + i, y + j] == lit || lightmap[x + i, y + j] == seen)
                            if(map[x + i, y + j] == token)
                                ct++;
            return ct;
        }
    }
}
