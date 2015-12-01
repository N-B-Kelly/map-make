using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using map_explore.Map;
using map_explore.Display;
using map_explore.Display.File;
using map_explore.Map.Explore;
using map_explore.Algorithm;
using SettlersEngine;
using System.Threading;

namespace Display_Map
{
    public partial class Map_Man : Form
    {
        Graphics graph;
        Brush block = new SolidBrush(Color.Purple);
        Brush back = new SolidBrush(Color.LightSalmon);
        Brush player = new SolidBrush(Color.Olive);
        Brush player2 = new SolidBrush(Color.Magenta);
        Brush unreachable = new SolidBrush(Color.Black);

        Brush highlight = new SolidBrush(Color.Red);
        Brush floor_seen = new SolidBrush(Color.LightSteelBlue);
        Brush wall_seen = new SolidBrush(Color.OliveDrab);
        Brush floor_stale = new SolidBrush(Color.SteelBlue);
        Brush wall_stale = new SolidBrush(Color.DarkOliveGreen);

        
        Bitmap btc;
        Bitmap maskmap;

        bool display_score_mode = false;
        int px;
        int py;
        int width = 50;
        int height = 50;
        float pixelWidth = 6;
        float pixelHeight = 6;
        int ca = 9;
        int cb = 8;
        int cc = 2;
        int cs = 8;

        int conservative_radius = 0;

        int visrad = 5;

        int[,] map;
        int[,] explore;
        int[,] open;
        
        int[,] scoreMap;
        int[,] visited;

        int delay = 15;

        public Map_Man () {
            InitializeComponent();
        }

        private EventArgs lastE = new EventArgs();
        private void pictureBox1_Click (object sender, EventArgs e) {
            lastE = e;
            generate(e);
        }

        private void generate(EventArgs e) {
            map_explore.Map.Explore.Machina.dispose();
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            maskmap = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            if ((e as System.Windows.Forms.MouseEventArgs) == null) {
                for (int i = 0; i < 3 + ca + cb + cc; i++) { //non constrained
                    map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                    Display_Image(btc, pictureBox1);
                    Thread.Sleep(delay);
                }
            }
            else if ((e as System.Windows.Forms.MouseEventArgs).Button == MouseButtons.Right) // constrained generation
                for (int i = 0; i <= cs; i++) {
                    map = map_explore.Map.Generation.Master.constrained_stepper(width, height, i, cs, map);
                    Display_Image(btc, pictureBox1);
                    Thread.Sleep(delay);
                }
            else for (int i = 0; i < 3 + ca + cb + cc; i++) { //non constrained
                    map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                    Display_Image(btc, pictureBox1);
                    Thread.Sleep(delay);
                }

            Random playerX = new Random();

            do {
                px = playerX.Next(width);
                py = playerX.Next(height);
            }
            while (map[px, py] != 1);

            Display_Player(btc, player, pictureBox1);

            visited = new int[width, height];
            //visited[px, py] = 1;
            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), new int[width, height]);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap, pictureBoxRight);
            Mask_Player(maskmap, player, px, py, pictureBoxRight);
        }
        private void int_gen() {
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            maskmap = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            //insert
            for (int i = 0; i < 3 + ca + cb + cc; i++) { //non constrained
                map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                Display_Image(btc, pictureBox1);
                Thread.Sleep(delay);
            }

            //insert
            Random playerX = new Random();

            do {
                px = playerX.Next(width);
                py = playerX.Next(height);
            }
            while (map[px, py] != 1);

            Display_Player(btc, player, pictureBox1);

            visited = new int[width, height];
            //visited[px, py] = 1;

            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), new int[width, height]);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap, pictureBoxRight);
            Mask_Player(maskmap, player, px, py, pictureBoxRight);
        }
        private void Display_Image (Bitmap btc, PictureBox input) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(back, 0, 0, (float)width * pixelWidth, (float)height * pixelHeight);
            for (int k = 0; k < width; k++) {
                for (int j = 0; j < height; j++) {
                    if (map[k, j] == 0) {
                        g.FillRectangle(block, k * pixelWidth, j * pixelHeight, pixelWidth, pixelHeight);
                    }
                }
            }
            g.Flush();

            input.Image = btc;
            input.Refresh();
        }
        private void Display_mask (Bitmap maskmap, PictureBox input) {
            Graphics g = Graphics.FromImage(maskmap);
            g.FillRectangle(unreachable, 0, 0, (float)width * pixelWidth, (float)height * pixelHeight);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++) {
                    if(explore[x, y] == 1)
                        if(map[x, y] == 0)
                            g.FillRectangle(wall_seen, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                        else
                            g.FillRectangle(floor_seen, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                    else if(explore[x, y] == 2)
                        if (map[x, y] == 0)
                            g.FillRectangle(wall_stale, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                        else
                            g.FillRectangle(floor_stale, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                    else if (open[x, y] == 1)
                        g.FillRectangle(highlight, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
                }
            g.Flush();
            input.Image = maskmap;
        }
        private void Display_Player (Bitmap btc, Brush pcol, PictureBox input) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(pcol, px * pixelWidth, py * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            input.Image = btc;
            input.Refresh();
        }
        private void Display_Player (Bitmap btc, Brush pcol, int x, int y, PictureBox input) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(pcol, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            input.Image = btc;
            pictureBox1.Refresh();
        }

        private void Display_Score_alt (Bitmap btc, PictureBox input) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, (float)width * pixelWidth, (float)height * pixelHeight);
            for (int k = 0; k < width; k++) {
                for (int j = 0; j < height; j++) {
                    if (scoreMap[k, j] != 0) {
                        int col = (40 + (scoreMap[k, j]) / 5);
                        if (col >= 246)
                            col = 245;
                        g.FillRectangle(new SolidBrush(Color.FromArgb(col, col, col + 10)), k * pixelWidth, j * pixelHeight, pixelWidth, pixelHeight);
                    }
                    else if (open[k, j] == 1)
                        g.FillRectangle(highlight, k * pixelWidth, j * pixelHeight, pixelWidth, pixelHeight);
                }
            }
            g.Flush();

            input.Image = btc;
            input.Refresh();
        }
        private void Mask_Player(Bitmap MaskMap, Brush pcol, int x, int y, PictureBox input) {
            Graphics g = Graphics.FromImage(MaskMap);
            g.FillRectangle(player2, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            input.Image = MaskMap;
            input.Refresh();
        }
        private void Form1_Load (object sender, EventArgs e) {
            pictureBox1.Width = (this.Width - 8)/2;
            pictureBox1.Height = this.Height - 28;
            pictureBoxRight.Height = pictureBox1.Height;
            pictureBoxRight.Width = pictureBox1.Width;
            pictureBoxRight.Location = new Point(pictureBoxRight.Width, pictureBox1.Location.Y);

            graph = pictureBox1.CreateGraphics();
            pictureBoxRight.CreateGraphics();
            map = new int[width, height];
            explore = new int[width, height];
            pixelWidth = pictureBox1.Width / width;
            pixelHeight = pictureBox1.Height / height;

            this.Show();

            int_gen();
        }

        private void OnResize(object sender, EventArgs e) {

            int w = this.Size.Width / 2;
            int h = this.Size.Height;
            if (w == 0 || h == 0)
                return;
            pictureBox1.Width = w;
            pictureBox1.Height = h;
            pictureBoxRight.Height = h;
            pictureBoxRight.Width = w;
            pictureBoxRight.Location = new Point(w, 0);

            pixelWidth = w / width;
            pixelHeight = h / height;

            if (pixelWidth == 0 || pixelHeight == 0)
                return;
            pictureBox1.Refresh();
            pictureBoxRight.Refresh();
            graph = pictureBox1.CreateGraphics();

            //redraw
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            maskmap = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);

            Display_Image(btc, pictureBox1);
            Display_Player(btc, player, pictureBox1);

            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), new int[width, height]);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap, pictureBoxRight);
            Mask_Player(maskmap, player, px, py, pictureBoxRight);
        }
        private void OnKeyDown (object sender, EventArgs e) {
            int pxl = px;
            int pyl = py;
            if ((e as KeyEventArgs).KeyCode == Keys.Left || (e as KeyEventArgs).KeyCode == Keys.NumPad4
                || (e as KeyEventArgs).KeyCode == Keys.NumPad7 || (e as KeyEventArgs).KeyCode == Keys.NumPad1)
                px -= 1;
            else if ((e as KeyEventArgs).KeyCode == Keys.Right || (e as KeyEventArgs).KeyCode == Keys.NumPad6
                || (e as KeyEventArgs).KeyCode == Keys.NumPad9 || (e as KeyEventArgs).KeyCode == Keys.NumPad3)
                px += 1;
            if ((e as KeyEventArgs).KeyCode == Keys.Up || (e as KeyEventArgs).KeyCode == Keys.NumPad8
                || (e as KeyEventArgs).KeyCode == Keys.NumPad7 || (e as KeyEventArgs).KeyCode == Keys.NumPad9)
                py -= 1;
            else if ((e as KeyEventArgs).KeyCode == Keys.Down || (e as KeyEventArgs).KeyCode == Keys.NumPad2
                || (e as KeyEventArgs).KeyCode == Keys.NumPad1 || (e as KeyEventArgs).KeyCode == Keys.NumPad3)
                py += 1;
            if (map[px, py] == 0) {
                px = pxl;
                py = pyl;
            }

            if ((e as KeyEventArgs).KeyCode == Keys.OemCloseBrackets)
                visrad += 1;
            else if ((e as KeyEventArgs).KeyCode == Keys.OemOpenBrackets)
                visrad -= 1;
            if (visrad < 1)
                visrad = 1;

            if ((e as KeyEventArgs).KeyCode == Keys.OemPipe) {
                if (conservative_radius == 1)
                    conservative_radius = 0;
                else
                    conservative_radius = 1;
                visited = new int[width, height];
                //visited[px, py] = 1;
            }

            else if ((e as KeyEventArgs).KeyCode == Keys.C) {
                width -= 10;
                height -= 10;

                if (width < 50 || height < 50)
                    width = height = 50;

                map = new int[width, height];
                explore = new int[width, height];
                pixelWidth = pictureBox1.Width / width;
                pixelHeight = pictureBox1.Height / height;

                generate(lastE);
            }
            else if ((e as KeyEventArgs).KeyCode == Keys.V) {
                width += 10;
                height += 10;

                if (width < 50 || height < 50)
                    width = height = 50;

                map = new int[width, height];
                explore = new int[width, height];
                pixelWidth = pictureBox1.Width / width;
                pixelHeight = pictureBox1.Height / height;

                generate(lastE);
            }
            else if ((e as KeyEventArgs).KeyCode == Keys.B) {
                display_score_mode = !display_score_mode;
                if (!display_score_mode) {
                    Display_Image(btc, pictureBox1);
                    Display_Player(btc, player, pictureBox1);
                }
            }
            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), explore);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            
            scoreMap = map_explore.Map.Explore.Machina.q_scoreboard_2(map, explore, open, width, height, visrad, (conservative_radius == 1), ref visited);

            if (display_score_mode) {
                Display_Score_alt(btc, pictureBox1);
                Mask_Player(btc, player, px, py, pictureBox1);
            }
            else {
                Display_Player(btc, back, pxl, pyl, pictureBox1);
                Display_Player(btc, player, pictureBox1);
            }

            
                Display_mask(maskmap, pictureBoxRight);
                Mask_Player(maskmap, player, px, py, pictureBoxRight);
            


            
            visited[px, py] = 1;

            if ((e as KeyEventArgs).KeyCode == Keys.Space) {
                Solve(0);
            }

            String str = "Map Man {  options: [ ] | C V   - -  W: " + width + ", H: " + height + "   - -  ";
            if (conservative_radius == 1)
                str += "Conservative  ";
            if (display_score_mode)
                str += "Showing Score";
            str += " vision radius: " + visrad + "}";
            this.Text = str;

        }

        bool equal (int[,] a, int[,] b, int width, int height) {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if ((a[x, y] ==0 && b[x, y] != 0) || (a[x, y] !=0 && b[x, y] == 0)) {
                        Console.WriteLine("false: difference at " + x + ", " + y);
                        return false;
                    }
            return true;
        }

        void Solve (int timescale) {
            while (true) {

                Thread.Sleep(timescale);
                int k = nodes_to_find();
                if (k == 0) {
                    Display_Image(btc, pictureBox1);
                    Display_Player(btc, back, px, py, pictureBox1);
                    Display_Player(btc, player, pictureBox1);
                    return;
                }

                List<Point> p = points_to_find();

                map_explore.Algorithm.Defgrid[,] dg = map_explore.Algorithm.Defgrid.initGrid(map, explore, width, height);

                List<int> scores = new List<int>();

                SettlersEngine.SpatialAStar<map_explore.Algorithm.Defgrid, Object> aStar =
                    new SettlersEngine.SpatialAStar<map_explore.Algorithm.Defgrid, Object>(dg);

                int low_ind = 0;
                int lowest = 0;

                for (int i = 0; i < p.Count; i++) {
                    scores.Add(Math.Abs(px - p[i].X) + Math.Abs( p[i].Y - py) - scoreMap[p[i].X, p[i].Y]);
                    if (i == 0)
                        lowest = scores[i];
                    else if (scores[i] < lowest) {
                        low_ind = i;
                        lowest = scores[i];
                    }
                }

                Point destination = p[low_ind];

                LinkedList<map_explore.Algorithm.Defgrid> path = aStar.Search(new Point(px, py), destination, null);

                int del = new Random().Next(12);

                for (int i = 0; i < path.Count; i++) {
                    Point ct = new Point(px, py);
                    Point nex = new Point(path.ElementAt(i).X, path.ElementAt(i).Y);


                    px = nex.X;
                    py = nex.Y;

                    explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), explore);
                    open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
                    scoreMap = map_explore.Map.Explore.Machina.q_scoreboard_2(map, explore, open, width, height, visrad, (conservative_radius == 1), ref visited);

                    Display_Score_alt(btc, pictureBox1);
                    Mask_Player(btc, player, px, py, pictureBox1);

                    Display_mask(maskmap, pictureBoxRight);
                    Mask_Player(maskmap, player, px, py, pictureBoxRight);

                    Thread.Sleep(timescale);

                    if (i == 70 + del)
                        break;
                }
            }

        }

        List<Point> points_to_find () {
            List<Point> p = new List<Point>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (scoreMap[x, y] != 0)
                        p.Add(new Point(x, y));
            return p;
        }

        int nodes_to_find () {
            int ct = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (open[x, y] == 1)
                        ct++;
            return ct;
        }
    }
}
