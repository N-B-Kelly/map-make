using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace map_explore.Map.Explore
{


    public class Ex
    {
        static int floor = 1;

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
        public static int[,] oldmap;
        static bool conservative;

        public static int[,] region;
        public static int[,] region_last;

        static int[,] highlit;

        static int width_last;
        static int height_last;
        //Checks if region is within square/circle bounds
        static bool inBounds (int dx, int dy, bool conserv, int rad) {
            if (conserv)
                return (dx * dx < rad * rad && dy * dy < rad * rad);
            else
                return (Math.Sqrt(Math.Abs(dx * dx + dy * dy)) < rad);

        }

        static bool israd (int x, int y, int width, int height) {
            return (y >= 0 && y < width && x >= 0 && x < width);
        }

        //Calculates field of view of map, and compares it with old map
        public static int[,] calcFOV (int[,] map, int width, int height, int radius, int startX, int startY, bool conserv, int[,] lastMap) {


            if (region == null || width != width_last || height != height_last) {
                highlit = new int[width, height];
                region = new int[width, height];
                Machina.tally = new int[width, height];
            }

            if (Machina.tally == null)
                Machina.tally = new int[width, height];

            region_last = region;
            width_last = width;
            height_last = height;

            region = new int[width, height];
            if (lightMap != null)
                oldmap = lightMap;
            else
                oldmap = new int[width, height];

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
                    if (inBounds(deltaX, deltaY, conservative, radius)) {
                        lightMap[currentX, currentY] = lit;
                        region[currentX, currentY] = 1;
                        if (highlit != null)
                            if (highlit[currentX, currentY] == 1 || Machina.tally[currentX, currentY] != 0) {
                                for (int x = currentX - radius - 1; x <= currentX + radius + 1; x++)
                                    for (int y = currentY - radius - 1; y <= currentY + radius + 1; y++)
                                        if (israd(x, y, width, height))
                                            if (Machina.tally[currentX, currentY] != 0)
                                                region[x, y] = 1;
                            }
                    }


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
            for (int x = 1; x < width; x++)
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
            highlit = highlight;
            return highlight;
        }

        static int adjacent_lit_tiles (int width, int height, int[,] lightmap, int[,] map, int x, int y, int token) {
            int ct = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    if (i == 0 && j == 0)
                        continue;
                    else
                        if (lightmap[x + i, y + j] == lit || lightmap[x + i, y + j] == seen)
                            if (map[x + i, y + j] == token)
                                ct++;
            return ct;
        }
    }

    public class Machina
    {
        static int unlit = 0;
        static int lit = 1;

        public static int[,] scoreboard (int[,] map, int[,] lit, int[,] highlight, int width, int height, int radius, bool conserv, int[,] visited) {
            scoremap = new int[width, height];
            lights = lit;
            Machina.highlight = highlight;
            Machina.visited = visited;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    if (highlight[x, y] == 1)
                        calcFOV(map, width, height, radius, x, y, conserv);
                }

            return scoremap;
        }

        static int[,] highlight;
        static int[,] lights;
        static int[,] scoremap;
        static int[,] resMap;
        static int[,] visited;
        static int[,] visited_2;
        static int rad;
        static int startX;
        static int startY;
        static int width;
        static int height;
        static bool conservative;

        //Calculates field of view of map, and compares it with old map
        public static int[,] calcFOV (int[,] map, int width, int height, int radius, int startX, int startY, bool conserv) {
            //scoremap[startX, startY] = force;   //light the starting cell
            resMap = map;
            Machina.rad = radius;
            Machina.startX = startX;
            Machina.startY = startY;
            Machina.width = width;
            Machina.height = height;
            Machina.conservative = conserv;
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

            return scoremap;
        }

        public static int[,] q_scoreboard (int[,] map, int[,] lit, int[,] highlight, int width, int height, int radius, bool conserv, ref int[,] visited) {
            int checks = 0;
            scoremap = new int[width, height];
            lights = lit;
            Machina.highlight = highlight;
            Machina.visited = visited;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    if (visited[x, y] == 0 && lit[x, y] != 0 && map[x, y] == 1) {
                        q_calcFOV(map, width, height, radius, x, y, conserv);
                        checks++;
                        if (scoremap[x, y] == 0)
                            visited[x, y] = 1;
                    }
                }
            Console.WriteLine("baseline mode - Checks: " + checks);
            return scoremap;
        }

        public static void dispose () {
            tally = null;
        }
        public static int[,] q_calcFOV (int[,] map, int width, int height, int radius, int startX, int startY, bool conserv) {
            //scoremap[startX, startY] = force;   //light the starting cell
            resMap = map;
            Machina.rad = radius;
            Machina.startX = startX;
            Machina.startY = startY;
            Machina.width = width;
            Machina.height = height;
            Machina.conservative = conserv;
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

            if (scoremap[startX, startY] == 0)
                visited[startX, startY] = 1;

            return scoremap;
        }
        //Checks if region is within square/circle bounds
        static bool inBounds (int dx, int dy, bool conserv, int rad) {
            if (conserv)
                return (dx * dx < rad * rad && dy * dy < rad * rad);
            else
                return (Math.Sqrt(Math.Abs(dx * dx + dy * dy)) < rad);

        }

        //calculates field of view in a single quadrant
        private static void castLight (int row, float start, float end, int xx, int xy, int yx, int yy) {
            float newStart = 0;
            if (start < end)
                return;
            bool blocked = false;
            for (int distance = row; distance <= rad && !blocked; distance++) {
                int deltaY = -distance;
                for (int deltaX = -distance; deltaX <= 0; deltaX++) {
                    //get our current cells
                    int currentX = startX + deltaX * xx + deltaY * xy;
                    int currentY = startY + deltaX * yx + deltaY * yy;

                    //get out current slopes
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    //make sure we're within bounds
                    if (!(currentX >= 0 && currentY >= 0 && currentX < width && currentY < height) || start < rightSlope)
                        continue;
                    //if we are, check if we need to stop
                    else if (end > leftSlope)
                        break;

                    //check we're within the target area, and if we're a highlight, then mark as lit if true
                    if (inBounds(deltaX, deltaY, conservative, rad) && highlight[currentX, currentY] == lit)
                        scoremap[startX, startY] += 2 * lit;


                    if (blocked) //our previous cell was a blocking one
                        if (resMap[currentX, currentY] == 0 || lights[currentX, currentY] == unlit || visited[currentX, currentY] == 1) {//we hit a wall, or we can't see there!
                            newStart = rightSlope;
                            continue;
                        }
                        else {
                            blocked = false;
                            start = newStart;
                        }
                    else
                        if ((resMap[currentX, currentY] == 0 || lights[currentX, currentY] == unlit || visited[currentX, currentY] == 1) && distance < rad) {//hit a wall within sight line
                            blocked = true;
                            castLight(distance + 1, start, leftSlope, xx, xy, yx, yy);
                            newStart = rightSlope;
                        }
                }
            }
        }

        public static int[,] q_scoreboard_2 (int[,] map, int[,] lit, int[,] highlight, int width, int height, int radius, bool conserv, ref int[,] visited) {

            int checks = 0;
            scoremap = new int[width, height];
            if (tally == null)
                tally = scoremap;
            scoremap = tally;

            lights = lit;
            Machina.highlight = highlight;
            Machina.visited_2 = Machina.visited = visited;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    if ((visited[x, y] == 0 && lit[x, y] != unlit && map[x, y] == 1) && (Ex.region[x, y] == 1 || Ex.region_last[x, y] == 1)) {
                        scoremap[x, y] = 0;
                        calcFOV(map, width, height, radius, x, y, conserv);
                        checks++;
                        if (scoremap[x, y] == 0)
                            visited[x, y] = 1;
                        else {
                            int k = adjacent_highlit_tiles(width, height, map, x, y);
                            int k_2 = second_adjacent_highlit_tiles(width, height, map, x, y);
                            if (k < 4 && k > 0) {
                                scoremap[x, y] *= 3 * (4 - k / 2);
                                if (k == 1) {
                                    if (k_2 == 0) {
                                        scoremap[x, y] *= 10;
                                    }
                                }
                            }
                            else if (k_2 == 2 || k_2 == 1 || k_2 == 5)
                                scoremap[x, y] *= 10;
                        }
                    }
                }
            return scoremap;
        }

        static int adjacent_highlit_tiles (int width, int height, int[,] map, int x, int y) {
            int ct = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    if (i == 0 && j == 0)
                        continue;
                    else
                        if (highlight[x + i, y + j] != 0)
                            ct++;
            return ct;
        }
        static int second_adjacent_highlit_tiles (int width, int height, int[,] map, int x, int y) {
            int ct = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    if (i == 0 && j == 0)
                        continue;
                    else
                        if (highlight[x + i, y + j] != 0)
                            return adjacent_highlit_tiles(width, height, map, x + i, y + j);
            return ct;
        }
        public static int[,] tally;
        public static int[,] q_calcFOV_2 (int[,] map, int width, int height, int radius, int startX, int startY, bool conserv) {
            //scoremap[startX, startY] = force;   //light the starting cell
            resMap = map;
            Machina.rad = radius;
            Machina.startX = startX;
            Machina.startY = startY;
            Machina.width = width;
            Machina.height = height;
            Machina.conservative = conserv;

            // TO DO
            //
            castLight_2(1, 1, 0, 0, 1, 1, 0);
            castLight_2(1, 1, 0, 1, 0, 0, 1);

            castLight_2(1, 1, 0, 0, -1, 1, 0);
            castLight_2(1, 1, 0, -1, 0, 0, 1);

            castLight_2(1, 1, 0, 0, -1, -1, 0);
            castLight_2(1, 1, 0, -1, 0, 0, -1);

            castLight_2(1, 1, 0, 0, 1, -1, 0);
            castLight_2(1, 1, 0, 1, 0, 0, -1);

            if (scoremap[startX, startY] == 0)
                visited[startX, startY] = 1;

            return scoremap;
        }
        //Checks if region is within square/circle bounds

        //calculates field of view in a single quadrant
        private static void castLight_2 (int row, float start, float end, int xx, int xy, int yx, int yy) {
            float newStart = 0;
            if (start < end)
                return;
            bool blocked = false;
            for (int distance = row; distance <= rad && !blocked; distance++) {
                int deltaY = -distance;
                for (int deltaX = -distance; deltaX <= 0; deltaX++) {
                    //get our current cells
                    int currentX = startX + deltaX * xx + deltaY * xy;
                    int currentY = startY + deltaX * yx + deltaY * yy;

                    //get out current slopes
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    //make sure we're within bounds
                    if (!(currentX >= 0 && currentY >= 0 && currentX < width && currentY < height) || start < rightSlope)
                        continue;
                    //if we are, check if we need to stop
                    else if (end > leftSlope)
                        break;

                    //check we're within the target area, and if we're a highlight, then mark as lit if true
                    if (inBounds(deltaX, deltaY, conservative, rad) && highlight[currentX, currentY] == lit)
                        scoremap[startX, startY] += 2 * lit;


                    if (blocked) //our previous cell was a blocking one
                        if (resMap[currentX, currentY] == 0 || lights[currentX, currentY] == unlit/* || visited[currentX, currentY] == 1*/) {//we hit a wall, or we can't see there!
                            newStart = rightSlope;
                            continue;
                        }
                        else {
                            blocked = false;
                            start = newStart;
                        }
                    else
                        if ((resMap[currentX, currentY] == 0 || lights[currentX, currentY] == unlit/* || visited[currentX, currentY] == 1*/) && distance < rad) {//hit a wall within sight line
                            blocked = true;
                            castLight_2(distance + 1, start, leftSlope, xx, xy, yx, yy);
                            newStart = rightSlope;
                        }
                }
            }
        }
    }

    public class dist_map
    {
        static int[,] dmap;
        static int[,] lmap;
        static int[,] map;
        static int w;
        static int h;
        static Queue<container> expandables;

        public static int[,] getdistmap (int[,] Map, int[,] lit, int x, int y, int width, int height) {
            dmap = new int[width, height];
            lmap = lit;
            map = Map;
            w = width;
            h = height;

            expandables = new Queue<container>();

            dmap[x, y] = -1;
            expandables.Enqueue(new container(x, y, 0));
            while(expandables.Count != 0) {
                container c = expandables.Dequeue();
                expand(c.X, c.Y, c.Count);
                Console.WriteLine(expandables.Count);
            }
            dmap[x, y] = 0;

            return dmap;
        }
        class container {
            public int X {get; set;}
            public int Y {get; set; }
            public int Count {get; set;}

            public container(int x, int y, int ct) {
                X = x;
                Y = y;
                Count = ct;
            }
        }
        public static void expand (int x, int y, int count) {
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    if ((Math.Abs(i - j) != 1))
                        continue;
                    else
                        if (lmap[x + i, y + j] != 0) //we need to have seen the place
                                if (map[x + i, y + j] == 1) //it also needs to be a floor
                                    if ((dmap[x + i, y + j] != -1 && dmap[x + i, y + j] > count) || dmap[x + i, y + j] == 0) {
                                        dmap[x + i, y + j] = count;
                                        expandables.Enqueue(new container(x + i, y + j, count + 2));
                                    }
        }
    }
}
        
