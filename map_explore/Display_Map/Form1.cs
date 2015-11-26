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

        int px;
        int py;
        int width = 100;
        int height = 100;
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
        int delay = 15;

        public Map_Man () {
            InitializeComponent();
        }

        private void pictureBox1_Click (object sender, EventArgs e) {
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            maskmap = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            if ((e as System.Windows.Forms.MouseEventArgs).Button == MouseButtons.Right) // constrained generation
                for (int i = 0; i <= cs; i++) {
                    map = map_explore.Map.Generation.Master.constrained_stepper(width, height, i, cs, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }
            else for (int i = 0; i < 3 + ca + cb + cc; i++) { //non constrained
                    map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }

            Random playerX = new Random();

            do {
                px = playerX.Next(width);
                py = playerX.Next(height);
            }
            while (map[px, py] != 1);

            Display_Player(btc, player);

            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), new int[width, height]);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap);
            Mask_Player(maskmap, player, px, py);
        }

        private void Display_Image (Bitmap btc) {
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

            pictureBox1.Image = btc;
            pictureBox1.Refresh();
        }
        private void Display_mask (Bitmap maskmap) {
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
            pictureBoxRight.Image = maskmap;
        }
        private void Display_Player(Bitmap btc, Brush pcol) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(pcol, px * pixelWidth, py * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            pictureBox1.Image = btc;
            pictureBox1.Refresh();
        }
        private void Display_Player(Bitmap btc, Brush pcol, int x, int y) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(pcol, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            pictureBox1.Image = btc;
            pictureBox1.Refresh();
        }
        private void Mask_Player(Bitmap MaskMap, Brush pcol, int x, int y) {
            Graphics g = Graphics.FromImage(MaskMap);
            g.FillRectangle(player2, x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight);
            g.Flush();

            pictureBoxRight.Image = MaskMap;
            pictureBoxRight.Refresh();
        }
        private void Form1_Load (object sender, EventArgs e) {
            pictureBox1.Width = (this.Width - 8)/2;
            pictureBox1.Height = this.Height - 28;
            pictureBoxRight.Height = pictureBox1.Height;
            pictureBoxRight.Width = pictureBox1.Width;
            pictureBoxRight.Location = new Point(pictureBoxRight.Width, pictureBox1.Location.Y);

            graph = pictureBox1.CreateGraphics();
            map = new int[width, height];
            explore = new int[width, height];
            pixelWidth = pictureBox1.Width / width;
            pixelHeight = pictureBox1.Height / height;
        }

        private void OnResize(object sender, EventArgs e) {

            int w = this.Size.Width / 2;
            int h = this.Size.Height;
            pictureBox1.Width = w;
            pictureBox1.Height = h;
            pictureBoxRight.Height = h;
            pictureBoxRight.Width = w;
            pictureBoxRight.Location = new Point(w, 0);

            pixelWidth = w / width;
            pixelHeight = h / height;

            pictureBox1.Refresh();
            pictureBoxRight.Refresh();
            graph = pictureBox1.CreateGraphics();

            //redraw
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            maskmap = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);

            Display_Image(btc);
            Display_Player(btc, player);

            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), new int[width, height]);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap);
            Mask_Player(maskmap, player, px, py);
        }
        private void OnKeyDown(object sender, EventArgs e) {
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
            else if(px != pxl || py != pyl){
                Display_Player(btc, player);
                Display_Player(btc, back, pxl, pyl);
            }

            if ((e as KeyEventArgs).KeyCode == Keys.OemCloseBrackets)
                visrad += 1;
            else if ((e as KeyEventArgs).KeyCode == Keys.OemOpenBrackets)
                visrad -= 1;
            if (visrad < 1)
                visrad = 1;

            if ((e as KeyEventArgs).KeyCode == Keys.OemPipe)
                if (conservative_radius == 1)
                    conservative_radius = 0;
                else
                    conservative_radius = 1;

            explore = map_explore.Map.Explore.Ex.calcFOV(map, width, height, visrad, px, py, (conservative_radius == 1), explore);
            open = map_explore.Map.Explore.Ex.highlight(width, height, explore, map);
            Display_mask(maskmap);
            Mask_Player(maskmap, player, px, py);

            String str = "Map Man {conservative(\\): ";
            if (conservative_radius == 1)
                str += "yes";
            else
                str += "no";
            str += ", vision radius([,]): " + visrad + "}";
            this.Text = str;

        }
    }
}
