using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using map_explore.Map;
using System.Timers;

namespace map_explore.Map.Generation {
    public class Master //TO DO: HALF THIS SHIT ISN'T EVEN USED ANYMORE, CLEAN IT THE FUCK UP
    {
        static Random rand = new Random();
        static bool Rand() {
            return (rand.Next(2) == 0);
        }
        static int ca = 7; //7;     //not really needed - these are just here as defaults
        static int cb = 12; //12;

        /// <summary>
        /// Generates an open map of size W x H
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static int[,] genBase(int width, int height) {
            int[,] mod = perlin(width, height, 8, 5, new int[width, height]);
            mod = expand_2(width, height, mod);
            mod = cellular(width, height, 11, 9, 13, 5, 4, mod);
            borderize(width, height, mod);


            int[,] crass = (int[,])mod.Clone();
            for (int i = 0; i < ca; i++) {
                Display.File.WriteFile.write_human(new Map(width, height, crass), "output_" + i + ".txt");
                mult_within(width, height, 2, crass);
                crass = perlin(width, height, 8, 7, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 9, 9, 15, 4, 5, crass);
                borderize(width, height, crass);
            }

            for (int i = ca; i < cb; i++) {
                Display.File.WriteFile.write_human(new Map(width, height, crass), "output_" + i + ".txt");
                mult_within(width, height, 1, crass);
                crass = perlin(width, height, 8, 6, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 11, 11, 16, 4, 5, crass);
                borderize(width, height, crass);
            }


            q_get_islands(width, height, crass);
            return crass;
        }   //generate open map from scratch
        public static int[,] continue_gen(int width, int height, int[,] crass) {
            for (int i = ca; i < cb; i++) {
                Display.File.WriteFile.write_human(new Map(width, height, crass), "output_" + i + ".txt");
                mult_within(width, height, 1, crass);
                crass = perlin(width, height, 8, 6, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 19, 11, 16, 4, 4, crass);
                borderize(width, height, crass);
            }


            q_get_islands(width, height, crass);

            Console.WriteLine("*** Continuation complete ***");
            return crass;
        } //generate closed map based on template   NOTE: you can use a blank or a filled array for an empty template

        /// <summary>
        /// Uses a perlin noise generation technique, specifying an amount of passes, (pass), and a cuttoff, (thresh)
        /// </summary>
        /// <param name="width">width of map</param>
        /// <param name="height">height of map</param>
        /// <param name="pass">amount of passes through map</param>
        /// <param name="thresh">threshold for cutoff</param>
        /// /// <param name="mod">map to use as template 
        /// (use new map[width, height] if unsure)</param>
        /// <returns>integet map array</returns>
        static int[,] perlin(int width, int height, int pass, int thresh, int[,] mod) {
            for (int z = 0; z < pass; z++)
                for (int i = 1; i < width - 1; i++)
                    for (int j = 1; j < height - 1; j++) {
                        if (Rand())
                            mod[i, j] = mod[i, j] + 1;
                    }

            for (int i = 1; i < width - 1; i++)
                for (int j = 1; j < height - 1; j++)
                    if (mod[i, j] < thresh)
                        mod[i, j] = 0;
                    else mod[i, j] = mod[i, j] - thresh + 1;
            return mod;
        }

        /// <summary>
        /// Expand each room to perlin generated size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        static int[,] expand_2(int width, int height, int[,] input) {
            int[,] output = new int[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    if (input[x, y] > 1 || (Rand() && input[x, y] == 1))
                        addBox(x, y, width, height, input[x, y] - 1, output);
                }

            return output;
        }
        /// <summary>
        /// Add box of size N x N
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="size"></param>
        /// <param name="mod"></param>
        static void addBox(int x, int y, int width, int height, int size, int[,] mod) {
            for (int X = x - size; X < x + size; X++)
                for (int Y = y - size; Y < y + size; Y++) {
                    if (X > 0 && Y > 0 && X < width - 1 && Y < height - 1)
                        mod[X, Y] = 1;
                }
        }
        static int[,] cellular(int width, int height, int pass, int del_min, int del_max, int close, int open, int[,] mod) {
            int[,] temp = mod;
            int[,] output = new int[width, height];
            for (int p = 0; p < pass; p++) {
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        if (mod[x, y] == 1) { //grow floors
                            if (neighbors(x, y, mod, width, height, 1) < close && rand_within(del_min, del_max))
                                output[x, y] = 0;
                            else
                                output[x, y] = 1;
                        }
                        else if (mod[x, y] == 0) { //grow walls
                            if (neighbors(x, y, mod, width, height, 0) < open && rand_within(del_min, del_max))
                                output[x, y] = 1;
                            else
                                output[x, y] = 0;
                        }
                    }
                }
                mod = output;
                output = new int[width, height];
            }
            output = mod;
            mod = temp;
            return output;
        }
        static int neighbors(int x, int y, int[,] map, int width, int height, int token) {
            int sum = 0;
            for (int X = x - 1; X < x + 2; X++) {
                for (int Y = y - 1; Y < y + 2; Y++) {
                    if (X == x && Y == y)
                        continue;
                    else if (X < 1 || X >= width || Y < 1 || Y >= height)
                        continue;
                    if (map[X, Y] == token)
                        sum += 1;
                }
            }
            return sum;
        }

        static bool rand_within(int min, int max) {
            return (rand.Next(max) >= min);
        }
        static void borderize(int width, int height, int[,] mod) {
            for (int i = 0; i < width; i++) {
                mod[i, 0] = 0;
                mod[i, 1] = 0;
                mod[i, height - 1] = 0;
                mod[i, height - 2] = 0;
            }

            for (int i = 0; i < height; i++) {
                mod[0, i] = 0;
                mod[1, i] = 0;
                mod[width - 1, i] = 0;
                mod[width - 2, i] = 0;
            }
        }
        static void mult_within(int width, int height, int mult, int[,] mod) {
            for (int i = 0; i < width; i++)
                for (int y = 0; y < height; y++)
                    mod[i, y] = mod[i, y] * mult;
        }

        class pair {
            public int x;
            public int y;
            public pair(int x, int y) {
                this.x = x;
                this.y = y;
            }
            public override string ToString() {
                return ("(" + x + ", " + y + ")");
            }
            public override bool Equals(object obj) {
                return (this.x == (obj as pair).x && this.y == (obj as pair).y);
            }
            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }

        static void Hallway_2(int xStart, int yStart, int xDest, int yDest, int[,] mod) {
            if (Rand()) {
                for (int i = min(xStart, xDest); i <= max(xStart, xDest); i++)
                    mod[i, yStart] = 1;
                for (int j = min(yStart, yDest); j <= max(yStart, yDest); j++)
                    mod[xDest, j] = 1;
            }
            else {
                for (int i = min(yStart, yDest); i <= max(yStart, yDest); i++)
                    mod[xStart, i] = 1;
                for (int j = min(xStart, xDest); j <= max(xStart, xDest); j++)
                    mod[j, yDest] = 1;
            }
        } //TODO

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

        public static int[,] stepper(int width, int height, int num, int ca, int cb, int cc, int[,] crass) {
            if (num == 0) {
                crass = perlin(width, height, 8, 5, new int[width, height]);
            }
            else if (num == 1) {
                crass = expand_2(width, height, crass);
            }
            else if (num == 2) {
                crass = cellular(width, height, 11, 9, 13, 5, 4, crass);
                borderize(width, height, crass);
            }
            else if (num < ca) {
                mult_within(width, height, 2, crass);
                crass = perlin(width, height, 8, 7, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 9, 9, 15, 4, 5, crass);
                borderize(width, height, crass);
            }
            else if (num < cb) {
                mult_within(width, height, 1, crass);
                crass = perlin(width, height, 8, 6, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 11, 11, 16, 4, 5, crass);
                borderize(width, height, crass);
            }
            else if (num < cc) {
                mult_within(width, height, 2, crass);
                crass = perlin(width, height, 8, 7, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 9, 9, 15, 4, 5, crass);
                borderize(width, height, crass);
            }
            else
                q_get_islands_5(width, height, crass);

            return crass;
        }
        public static int[,] constrained_stepper(int width, int height, int num, int cb, int[,] crass) {
            if (num < cb) {
                mult_within(width, height, 1, crass);
                crass = perlin(width, height, 8, 6, crass);
                crass = expand_2(width, height, crass);
                crass = cellular(width, height, 19, 11, 16, 4, 4, crass);
                borderize(width, height, crass);
            }

            else {
                crass = cellular(width, height, 9, 9, 15, 4, 5, crass);
                borderize(width, height, crass);
                q_get_islands_5(width, height, crass);
            }

            //Console.WriteLine("*** Continuation " + num + " complete ***");
            return crass;
        }

        static ArrayList q_islandGet(int[,] map, int width, int height) {
            ArrayList islands = new ArrayList();

            Timer t = new Timer(50);
            t.Elapsed += OnTime;
            t.AutoReset = true;
            t.Enabled = true;

            for (int x = 2; x < width; x++)
                for (int y = 2; y < height; y++)
                    if (map[x, y] == 1 && !q_tileInIsland(x, y, islands))
                        islands.Add(q_scanIsland_3(width, height, map, x, y));


            t.Stop();
            Console.WriteLine("*** Fill finished in: " + time + " ms ***");
            time = 0;
            return islands;
        }
        static bool q_tileInIsland(int x, int y, ArrayList islands) {
            foreach (ArrayList arr in islands)
                if (arr.Contains(new pair(x, y)))
                    return true;
            return false;
        }

        static int time = 0;
        static void OnTime(Object source, ElapsedEventArgs e) {
            time += 50;
        }

        /// <summary>
        /// uses scanfill to run through an island identifying it's exact bounds !WOAH
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="map"></param>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <returns></returns>
        static ArrayList q_scanIsland_2(int width, int height, int[,] map, int xs, int ys) {
            Queue<pair> q = new Queue<pair>();
            ArrayList beacon = new ArrayList();

            pair pr = new pair(xs, ys);
            q.Enqueue(pr);
            while (q.Count != 0) {
                pair p = q.Dequeue();
                int x = p.x;
                int y = p.y;
                bool topClosed = true;
                bool botClosed = true;
                if (!beacon.Contains(p) && map[x, y] == 1) {
                    //Console.WriteLine(q.Count);
                    beacon.Add(p);
                    int ct = 1;
                    if (y - 1 >= 0)
                        q.Enqueue(new pair(x, y - 1));
                    if (y + 1 >= 0)
                        q.Enqueue(new pair(x, y + 1));

                    while (x + ct < width && map[x + ct, y] == 1) {
                        beacon.Add(new pair(x + ct, y));
                        if (map[x + ct, y + 1] == 0)
                            topClosed = true;
                        else if (topClosed) {
                            q.Enqueue(new pair(x + ct, y + 1));
                            topClosed = false;
                        }
                        if (map[x + ct, y - 1] == 0)
                            botClosed = true;
                        else if (botClosed) {
                            q.Enqueue(new pair(x + ct, y - 1));
                            botClosed = false;
                        }
                        ct++;
                    }
                    ct = -1;
                    while (x + ct < width && map[x + ct, y] == 1) {
                        beacon.Add(new pair(x + ct, y));
                        if (map[x + ct, y + 1] == 0)
                            topClosed = true;
                        else if (topClosed) {
                            q.Enqueue(new pair(x + ct, y + 1));
                            topClosed = false;
                        }
                        if (map[x + ct, y - 1] == 0)
                            botClosed = true;
                        else if (botClosed) {
                            q.Enqueue(new pair(x + ct, y - 1));
                            botClosed = false;
                        }
                        ct--;
                    }
                }
            }
            return beacon;
        }
        /// <summary>
        /// uses scanfill to run through an island identifying it's exact bounds !WOAH  -this one seems to have variable speeds - sometimes faster, sometimes
        /// slower than q_scanIsland_3
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="map"></param>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <returns></returns>
        static ArrayList q_scanIsland_3(int width, int height, int[,] map, int xs, int ys) {
            Queue<pair> q = new Queue<pair>();
            ArrayList beacon = new ArrayList();

            pair pr = new pair(xs, ys);
            q.Enqueue(pr);
            while (q.Count != 0) {
                pair p = q.Dequeue();
                int x = p.x;
                int y = p.y;
                bool topClosed = true;
                bool botClosed = true;
                if (!beacon.Contains(p) && map[x, y] == 1) {
                    //Console.WriteLine(q.Count);
                    beacon.Add(p);
                    int ct = 0;
                    while (map[x + ct, y] == 1)
                        ct--;
                    ct++;
                    if (y - 1 >= 0)
                        q.Enqueue(new pair(x, y - 1));
                    if (y + 1 >= 0)
                        q.Enqueue(new pair(x, y + 1));

                    while (x + ct < width && map[x + ct, y] == 1) {
                        beacon.Add(new pair(x + ct, y));
                        if (map[x + ct, y + 1] == 0)
                            topClosed = true;
                        else if (topClosed) {
                            q.Enqueue(new pair(x + ct, y + 1));
                            topClosed = false;
                        }
                        if (map[x + ct, y - 1] == 0)
                            botClosed = true;
                        else if (botClosed) {
                            q.Enqueue(new pair(x + ct, y - 1));
                            botClosed = false;
                        }
                        ct++;
                    }

                }
            }
            return beacon;
        }
        /// <summary>
        /// Concatenates all individual islands on the map into a single tree
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        static int[,] q_get_islands(int width, int height, int[,] mod) {

            Console.WriteLine("Making islands");
            ArrayList islandList = q_islandGet(mod, width, height);

            Console.WriteLine(islandList.Count + " islands found");

            List<pair> centers = new List<pair>();

            for (int i = 0; i < islandList.Count; i++) {
                centers.Add(q_center(islandList[i] as ArrayList));
            }

            Console.WriteLine("Centers determined for all islands");

            for (int i = 1; i < islandList.Count; i++) {//(islandList.Count > 1) {
                pair centerA = centers[0];
                pair centerB = centers[i];

                pair AClose = q_closest(centerB, islandList[0] as ArrayList);
                pair BClose = q_closest(AClose, islandList[i] as ArrayList);

                Hallway_2(AClose.x, AClose.y, BClose.x, BClose.y, mod);

                (islandList[0] as ArrayList).AddRange(islandList[i] as ArrayList);//(islandList[0] as ArrayList).AddRange(islandList[i] as ArrayList);

            }

            Console.WriteLine("Connected all islands");

            return mod;
        }
        /// <summary>
        /// Finds the center of an arraylist of nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        static pair q_center(ArrayList nodes) {
            int x = 0;
            int y = 0;
            int ct = 0;
            foreach (pair p in nodes) { //sum everything
                x += p.x;
                y += p.y;
                ct += 1;
            }
            x = x / ct;
            y = y / ct; //div ct

            return new pair(x, y);
        }
        /// <summary>
        /// finds the node within an island that has the shortest manhattan distance to another node
        /// </summary>
        /// <param name="target"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        static pair q_closest(pair target, ArrayList nodes) {
            pair closest = (nodes[0] as pair);
            int dist = Math.Abs(closest.x - target.x) + Math.Abs(closest.y - target.y);
            for (int i = 1; i < nodes.Count; i += 5) {
                int dist2 = Math.Abs((nodes[i] as pair).x - target.x) + Math.Abs((nodes[i] as pair).y - target.y);
                if (dist2 < dist) {
                    dist = dist2;
                    closest = (nodes[i] as pair);
                }
            }
            return closest;
        }


        static int mod_rythm = 0; //this is probably the (talentless hack)iest workaround in this document
        /// <summary>
        /// Concatenates all individual islands on the map into a single tree
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        static int[,] q_get_islands_2(int width, int height, int[,] mod) { //NEXT ITERATION: USE A LIST OF CENTERS PER ISLAND, AND SELECT FROM THE CLOSEST IN THAT LIST!
            Console.WriteLine("Making islands");
            ArrayList islandList = q_islandGet(mod, width, height);

            Console.WriteLine(islandList.Count + " islands found");

            List<pair> centers = new List<pair>();
            List<int> sizes = new List<int>();
            for (int i = 0; i < islandList.Count; i++) {
                centers.Add(q_center(islandList[i] as ArrayList));
                sizes.Add((islandList[i] as ArrayList).Count);
            }

            Console.WriteLine("Centers determined for all islands");

            while (islandList.Count > 3) {
                //declare four islands, pick one
                int[] islandstouse = new int[] { rand.Next(islandList.Count), -1, -1, -1 }; //array of places in list to use

                //pick the three closest islands
                for (int i = 0; i < islandList.Count; i++)
                    if (islandstouse.Contains(i))
                        continue;
                    else {
                        mod_rythm++;
                        for (int change = 0; change < 3; change++)
                            if (islandstouse[(change + mod_rythm) % 3 + 1] == -1) {
                                islandstouse[(change + mod_rythm) % 3 + 1] = i;
                                break;
                            }
                            else if (Math.Abs(centers[i].x - centers[islandstouse[0]].x) + Math.Abs(centers[i].y - centers[islandstouse[0]].y) <
                                Math.Abs(centers[(change + mod_rythm) % 3 + 1].x - centers[islandstouse[0]].x) + Math.Abs(centers[(change + mod_rythm) % 3 + 1].y - centers[islandstouse[0]].y)) {
                                islandstouse[(change + mod_rythm) % 3 + 1] = i;
                                break;
                            }
                    }

#region r
                //select the center of each of these islands
                pair centerA = centers[islandstouse[0]];
                pair centerB = centers[islandstouse[1]];
                pair centerC = centers[islandstouse[2]];
                pair centerD = centers[islandstouse[3]];

                //determine shortest path between each of these islands and the target island
                pair ACloseB = q_closest(centerB, islandList[islandstouse[0]] as ArrayList);
                pair BCloseA = q_closest(ACloseB, islandList[islandstouse[1]] as ArrayList);
                int length_AB = Math.Abs(ACloseB.x - BCloseA.x) + Math.Abs(ACloseB.y - BCloseA.y) + (int)Math.Sqrt(Math.Pow(ACloseB.x - BCloseA.x, 2) + Math.Pow(ACloseB.y - BCloseA.y, 2));

                pair ACloseC = q_closest(centerC, islandList[islandstouse[0]] as ArrayList);
                pair CCloseA = q_closest(ACloseC, islandList[islandstouse[2]] as ArrayList);
                int length_AC = Math.Abs(ACloseC.x - CCloseA.x) + Math.Abs(ACloseC.y - CCloseA.y) + (int)Math.Sqrt(Math.Pow(ACloseC.x - CCloseA.x, 2) + Math.Pow(ACloseC.y - CCloseA.y, 2));

                pair ACloseD = q_closest(centerD, islandList[islandstouse[0]] as ArrayList);
                pair DCloseA = q_closest(ACloseD, islandList[islandstouse[3]] as ArrayList);
                int length_AD = Math.Abs(ACloseD.x - DCloseA.x) + Math.Abs(ACloseD.y - DCloseA.y) + (int)Math.Sqrt(Math.Pow(ACloseD.x - DCloseA.x, 2) + Math.Pow(ACloseD.y - DCloseA.y, 2));

                List<int> sel = new List<int> { length_AB, length_AC, length_AD };
                int shortest = Shortest(sel);

                if (shortest == 0) {
                    //join the hallways
                    Hallway_2(ACloseB.x, ACloseB.y, BCloseA.x, BCloseA.y, mod);

                    //add item B to our base island
                    (islandList[islandstouse[0]] as ArrayList).AddRange(islandList[islandstouse[1]] as ArrayList);

                    //adjust center points - this isn't perfect, it just averages the centers
                    //maybe later I will make it take size into account - would mean I'd have to actually list size again :)
                    centers[islandstouse[0]] = new pair((centers[islandstouse[0]].x * sizes[islandstouse[0]] + centers[islandstouse[1]].x * sizes[islandstouse[1]]) / 2 * (sizes[islandstouse[1]] + sizes[islandstouse[0]]),
                        (centers[islandstouse[0]].y * sizes[islandstouse[0]] + centers[islandstouse[1]].y * sizes[islandstouse[1]]) / 2 * (sizes[islandstouse[1]] + sizes[islandstouse[0]]));

                    //remove items from lists
                    islandList.RemoveAt(islandstouse[1]);
                    centers.RemoveAt(islandstouse[1]);
                    sizes.RemoveAt(1);
                }
                else if (shortest == 1) {
                    //join the hallways
                    Hallway_2(ACloseC.x, ACloseC.y, CCloseA.x, CCloseA.y, mod);

                    //add item C to our base island
                    (islandList[islandstouse[0]] as ArrayList).AddRange(islandList[islandstouse[2]] as ArrayList);

                    //adjust center points - this isn't perfect, it just averages the centers
                    //maybe later I will make it take size into account - would mean I'd have to actually list size again :)
                    centers[islandstouse[0]] = new pair((centers[islandstouse[0]].x * sizes[islandstouse[0]] + centers[islandstouse[2]].x * sizes[islandstouse[2]]) / 2 * (sizes[islandstouse[2]] + sizes[islandstouse[0]]),
                        (centers[islandstouse[0]].y * sizes[islandstouse[0]] + centers[islandstouse[2]].y * sizes[islandstouse[2]]) / 2 * (sizes[islandstouse[2]] + sizes[islandstouse[0]]));

                    //remove items from lists
                    islandList.RemoveAt(islandstouse[2]);
                    centers.RemoveAt(islandstouse[2]);
                    sizes.RemoveAt(2);
                }
                else if (shortest == 2) {
                    //join the hallways
                    Hallway_2(ACloseD.x, ACloseD.y, DCloseA.x, DCloseA.y, mod);

                    //add item D to our base island
                    (islandList[islandstouse[0]] as ArrayList).AddRange(islandList[islandstouse[3]] as ArrayList);

                    //adjust center points - this isn't perfect, it just averages the centers
                    //maybe later I will make it take size into account - would mean I'd have to actually list size again :)
                    centers[islandstouse[0]] = new pair((centers[islandstouse[0]].x * sizes[islandstouse[0]] + centers[islandstouse[3]].x * sizes[islandstouse[3]]) / 2 * (sizes[islandstouse[3]] + sizes[islandstouse[0]]),
                        (centers[islandstouse[0]].y * sizes[islandstouse[0]] + centers[islandstouse[3]].y * sizes[islandstouse[3]]) / 2 * (sizes[islandstouse[3]] + sizes[islandstouse[0]]));

                    //remove items from lists
                    islandList.RemoveAt(islandstouse[3]);
                    centers.RemoveAt(islandstouse[3]);
                    sizes.RemoveAt(3);
                }
            }


            for (int i = 1; i < islandList.Count; i++) {//(islandList.Count > 1) {
                pair centerA = centers[0];
                pair centerB = centers[i];

                pair AClose = q_closest(centerB, islandList[0] as ArrayList);
                pair BClose = q_closest(AClose, islandList[i] as ArrayList);

                Hallway_2(AClose.x, AClose.y, BClose.x, BClose.y, mod);

                (islandList[0] as ArrayList).AddRange(islandList[i] as ArrayList);//(islandList[0] as ArrayList).AddRange(islandList[i] as ArrayList);

            }
            Console.WriteLine("Connected all islands");
#endregion
            return mod;
        }


        static int[,] q_get_islands_4(int width, int height, int[,] mod) {
            Console.WriteLine("Making islands");
            ArrayList islandList = q_islandGet(mod, width, height);

            Console.WriteLine(islandList.Count + " islands found");

            List<List<pair>> centers = new List<List<pair>>();
            List<int> sizes = new List<int>();
            for (int i = 0; i < islandList.Count; i++) {
                centers.Add(new List<pair> {q_center(islandList[i] as ArrayList)});
                sizes.Add((islandList[i] as ArrayList).Count);
            }

            
            Console.WriteLine("Centers determined for all islands");

            while (islandList.Count > 1) {
                //pick an islands
                int island = rand.Next(islandList.Count);
                int island_ind = -1;
                int compar = -1;
                int compar_ind = -1;
                int mindist = -1;
                for (int i = 0; i < islandList.Count; i++) {
                    if (i == island) //our matching case
                        continue;
                    pair set = closest(centers[island], centers[i]);
                    int dis = select_dist(centers[island][set.x], centers[i][set.y]);
                    if (compar == -1 || dis < mindist) {
                        compar = i;
                        island_ind = set.x;
                        compar_ind = set.y;
                        mindist = dis;
                    }
                }

                pair centerA = centers[island][island_ind];
                pair centerB = centers[compar][compar_ind];

                pair AClose = q_closest(centerB, islandList[island] as ArrayList);
                pair BClose = q_closest(AClose, islandList[compar] as ArrayList);

                Hallway_2(AClose.x, AClose.y, BClose.x, BClose.y, mod);

                if (rand.Next(10) < 7) {
                    centers[island].AddRange(centers[compar]);
                    (islandList[island] as ArrayList).AddRange(islandList[compar] as ArrayList);

                    centers.RemoveAt(compar);
                    islandList.RemoveAt(compar);
                }
            }

            return mod;
        }
        static int[,] q_get_islands_5(int width, int height, int[,] mod) {
            Console.WriteLine("Making islands");
            ArrayList islandList = q_islandGet(mod, width, height);

            Console.WriteLine(islandList.Count + " islands found");

            List<List<pair>> centers = new List<List<pair>>();
            List<int> sizes = new List<int>();
            for (int i = 0; i < islandList.Count; i++) {
                centers.Add(new List<pair> { q_center(islandList[i] as ArrayList) });
                sizes.Add((islandList[i] as ArrayList).Count);
            }


            Console.WriteLine("Centers determined for all islands");

            while (islandList.Count > 1) {
                //pick an islands
                int island = rand.Next(islandList.Count);
                int island_ind = -1;
                int compar = -1;
                int compar_ind = -1;
                int mindist = -1;
                int last = -1;
                int last_ind = -1;
                for (int i = 0; i < islandList.Count; i++) {
                    if (i == island) //our matching case
                        continue;
                    pair set = closest(centers[island], centers[i]);
                    int dis = select_dist(centers[island][set.x], centers[i][set.y]);
                    if (compar == -1 || dis < mindist) {
                        if (dis < mindist && rand_within(5, 10)) {
                            last = compar;
                            last_ind = compar_ind;
                        }
                        compar = i;
                        island_ind = set.x;
                        compar_ind = set.y;
                        mindist = dis;
                    }
                }

                pair centerA = centers[island][island_ind];
                pair centerB = centers[compar][compar_ind];
                

                pair AClose = q_closest(centerB, islandList[island] as ArrayList); //target, dest
                pair BClose = q_closest(AClose, islandList[compar] as ArrayList); //dest, target

                if (last != -1) {
                    pair centerL = centers[last][last_ind];

                    pair ACloseL = q_closest(centerL, islandList[island] as ArrayList);
                    pair LCloseA = q_closest(ACloseL, islandList[last] as ArrayList);

                    pair BCloseL = q_closest(centerL, islandList[compar] as ArrayList);
                    pair LCloseB = q_closest(BCloseL, islandList[last] as ArrayList);

                    if (rand.Next(10) < 4) {
                        Hallway_3(ACloseL.x, ACloseL.y, LCloseA.x, LCloseA.y, mod, islandList, compar);
                    }
                    if (rand.Next(10) < 4) {
                        Hallway_3(BCloseL.x, BCloseL.y, LCloseB.x, LCloseB.y, mod, islandList, compar);
                    }
                }
                Hallway_3(AClose.x, AClose.y, BClose.x, BClose.y, mod, islandList, compar);

                
                if (true) {//rand.Next(10) < 7) {
                    centers[island].AddRange(centers[compar]);
                    (islandList[island] as ArrayList).AddRange(islandList[compar] as ArrayList);

                    centers.RemoveAt(compar);
                    islandList.RemoveAt(compar);
                }
            }

            return mod;
        }
        static int[,] q_get_islands_3(int width, int height, int[,] mod) {
            Console.WriteLine("Making islands");
            ArrayList islandList = q_islandGet(mod, width, height);

            Console.WriteLine(islandList.Count + " islands found");

            List<pair> centers = new List<pair>();
            List<int> sizes = new List<int>();
            for (int i = 0; i < islandList.Count; i++) {
                centers.Add(q_center(islandList[i] as ArrayList));
                sizes.Add((islandList[i] as ArrayList).Count);
            }

            Console.WriteLine("Centers determined for all islands");

            while (islandList.Count > 1) {
                //pick an islands
                int island = rand.Next(islandList.Count);
                int compar = -1;
                for (int i = 0; i < islandList.Count; i++) {
                    if (i == island) //match case
                        continue;
                    if (compar == -1) { //base case
                        compar = i;
                        //now we need to select indices
                        continue;
                    }

                    //check to see if lower distance:
                    if (select_dist(centers[compar], centers[island]) > select_dist(centers[island], centers[i]))
                        compar = i;
                }

                pair centerA = centers[island];
                pair centerB = centers[compar];

                pair AClose = q_closest(centerB, islandList[island] as ArrayList);
                pair BClose = q_closest(AClose, islandList[compar] as ArrayList);

                Hallway_2(AClose.x, AClose.y, BClose.x, BClose.y, mod);
                centers[island] = new pair((centers[island].x + centers[compar].x) / 2, (centers[island].y + centers[compar].y) / 2);
                (islandList[island] as ArrayList).AddRange(islandList[compar] as ArrayList);

                centers.RemoveAt(compar);
                islandList.RemoveAt(compar);

            }
            return mod;

        }


        static void Hallway_3(int xStart, int yStart, int xDest, int yDest, int[,] mod, ArrayList host, int host_index) {
            ArrayList arr = new ArrayList();

            if (Rand()) {
                for (int i = min(xStart, xDest); i <= max(xStart, xDest); i++) {
                    mod[i, yStart] = 1;
                    arr.Add(new pair(i, yStart));
                }
                for (int j = min(yStart, yDest); j <= max(yStart, yDest); j++) {
                    mod[xDest, j] = 1;
                    arr.Add(new pair(xDest, j));
                }
            }
            else {
                for (int i = min(yStart, yDest); i <= max(yStart, yDest); i++) {
                    mod[xStart, i] = 1;
                    arr.Add(new pair(xStart, i));
                }
                for (int j = min(xStart, xDest); j <= max(xStart, xDest); j++) {
                    mod[j, yDest] = 1;
                    arr.Add(new pair(j, yDest ));
                }
            }

            (host[host_index] as ArrayList).AddRange(arr);
        } //TODO

        /// <summary>
        /// returns closest pair of indices
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static pair closest(List<pair> a, List<pair> b) {
            int short_num = 99999;
            int short_a = -1;
            int short_b = -1;

            for(int i = 0; i < a.Count; i++)
                for (int j = 0; j < b.Count; j++) {
                    int dis = select_dist(a[i], b[j]);
                    if (dis < short_num) {
                        short_a = i;
                        short_b = j;
                        short_num = dis;
                    }
                }

            return new pair(short_a, short_b);
        }
        /// <summary>
        /// returns the item in the list closest to the selected pair
        /// </summary>
        /// <param name="pairlist"></param>
        /// <param name="_pair"></param>
        /// <returns></returns>
        static int closest_less(List<pair> pairlist, pair _pair) {
            int shortest = 0;
            int dist = select_dist(pairlist[0], _pair);
            for (int i = 1; i < pairlist.Count; i++) {
                int k = select_dist(pairlist[i], _pair);
                if (k < dist) {
                    dist = k;
                    shortest = i;
                }
            }
            return shortest;
        }

        //WE NEED MAJOR CHANGES HERE!!!!
        /// <summary>
        /// returns an object containing:
        /// the element in list I closest to ANY item in list j
        /// the shortest attained distance between I and J
        /// </summary>
        /// <param name="pairlist"></param>
        /// <param name="competingList"></param>
        /// <param name="shortest"></param>
        /// <returns></returns>
        static pair closest_more(List<pair> pairlist, List<pair> competingList) { //NOTE: WE ONLY NEED KEEP TRACK OF ONE SIDE SHORTEST DIST
            int shortest = -1;
            int low_i = -1;
            int low_j = -1;
            for (int i = 0; i < pairlist.Count; i++)
                for (int j = 0; j < competingList.Count; j++) {
                    int k = select_dist(pairlist[i], competingList[j]);
                    if (k < shortest || shortest == -1) {
                        shortest = k;
                        low_i = i;
                        low_j = j;
                    }
                }
            if (low_i != -1)
                return new pair(low_i, closest_less(pairlist, competingList[low_j])); //pack this for ease of use
            else return new pair(-1, -1);
        }

        //applies distance calculation to a pair
        static int select_dist(pair x, pair y) {
            int dist = Math.Abs(x.x - y.x) + Math.Abs(x.y - y.y);
            dist += (int)Math.Sqrt(Math.Pow(x.x - y.x, 2) + Math.Pow(x.y - y.y, 2));
            return dist / 2;

        }
        static int Shortest(List<int> sel) {
            int sh = 0;
            for (int i = 1; i < sel.Count; i++)
                if (sel[i] < sel[sh])
                    sh = i;
            return sh;
        }
    }
}
