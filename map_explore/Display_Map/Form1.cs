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
using System.Threading;

namespace Display_Map
{
    public partial class Map_Man : Form
    {
        Graphics graph;
        Brush block = new SolidBrush(Color.Purple);
        Brush back = new SolidBrush(Color.LightSalmon);
        Brush player = new SolidBrush(Color.Olive);
        Bitmap btc;
        int px;
        int py;
        int width = 45;
        int height = 45;
        float pixelWidth = 6;
        float pixelHeight = 6;
        int ca = 9;
        int cb = 8;
        int cc = 2;
        int cs = 8;

        int[,] map;
        int delay = 15;

        public Map_Man () {
            InitializeComponent();
        }

        private void pictureBox1_Click (object sender, EventArgs e) {
            btc = new Bitmap(width * (int)pixelWidth, height * (int)pixelHeight);
            if ((e as System.Windows.Forms.MouseEventArgs).Button == MouseButtons.Right) {
                for (int i = 0; i <= cs; i++) {
                    map = map_explore.Map.Generation.Master.constrained_stepper(width, height, i, cs, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }
            }
            else {
                for (int i = 0; i < 3 + ca + cb + cc; i++) {
                    map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }
            }
            Random playerX = new Random();

            do {
                px = playerX.Next(width);
                py = playerX.Next(height);
            }
            while (map[px, py] != 1);

            Display_Player(btc, player);
        }

        private void Display_Image (Bitmap btc) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(back, 0, 0, width * pixelWidth, height * pixelHeight);
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
        private void Form1_Load (object sender, EventArgs e) {
            pictureBox1.Width = this.Width - 8;
            pictureBox1.Height = this.Height - 28;
            graph = pictureBox1.CreateGraphics();
            map = new int[width, height];
            pixelWidth = pictureBox1.Width / width;
            pixelHeight = pictureBox1.Height / height;
            
        }

        private void OnResize(object sender, EventArgs e) {
            pictureBox1.Width = this.Width - 8;
            pictureBox1.Height = this.Height - 28;
            graph = pictureBox1.CreateGraphics();
            map = new int[width, height];
            pixelWidth = (float)pictureBox1.Width / (float)width;
            pixelHeight = (float)pictureBox1.Height / (float)height;
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
        }
    }
}
