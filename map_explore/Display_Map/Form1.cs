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
    public partial class Form1 : Form
    {
        Graphics graph;
        Brush block = new SolidBrush(Color.Purple);
        Brush back = new SolidBrush(Color.LightSalmon);
        Image im_wall = Image.FromFile(@"Resources/Wall_Seen.png");
        Image im_floor = Image.FromFile(@"Resources/Blank_Seen.png");
        int width = 300;
        int height = 150;
        int pixel = 6;

        int ca = 9;
        int cb = 8;
        int cc = 2;
        int cs = 8;

        int delay = 15;

        public Form1 () {
            InitializeComponent();
        }
        int[,] map;

        /* 
        private void pictureBox1_Click (object sender, EventArgs e) {
            if ((e as System.Windows.Forms.MouseEventArgs).Button == MouseButtons.Right)
                map = map_explore.Map.Generation.Master.continue_gen(width, height, map);
            else
                map = map_explore.Map.Generation.Master.genBase(width, height);
            pictureBox1.Refresh();
            graph.FillRectangle(back, new Rectangle(0, 0, width*pixel, height*pixel));
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    Point p = new Point(i * pixel, j * pixel);
                    if (map[i, j] == 0) {
                        graph.FillRectangle(block, new Rectangle(p, new Size(pixel, pixel)));
                    }
                }
            }
        }
         */

        private void pictureBox1_Click (object sender, EventArgs e) {
            if ((e as System.Windows.Forms.MouseEventArgs).Button == MouseButtons.Right) {
                Bitmap btc = new Bitmap(width * pixel, height * pixel);
                for (int i = 0; i <= cs; i++) {
                    map = map_explore.Map.Generation.Master.constrained_stepper(width, height, i, cs, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }
            }
            else {
                Bitmap btc = new Bitmap(width * pixel, height * pixel);
                for (int i = 0; i < 3 + ca + cb + cc; i++) {
                    map = map_explore.Map.Generation.Master.stepper(width, height, i, 2 + ca, 2 + ca + cb, 2 + ca + cb + cc, map);
                    Display_Image(btc);
                    Thread.Sleep(delay);
                }
            }
        }

        private void Display_Image (Bitmap btc) {
            Graphics g = Graphics.FromImage(btc);
            g.FillRectangle(back, new Rectangle(0, 0, width * pixel, height * pixel));
            for (int k = 0; k < width; k++) {
                for (int j = 0; j < height; j++) {
                    Point p = new Point(k * pixel, j * pixel);
                    if (map[k, j] == 0) {
                        g.FillRectangle(block, new Rectangle(p, new Size(pixel, pixel)));
                    }
                }

            }
            g.Flush();

            pictureBox1.Image = btc;
            pictureBox1.Refresh();
        }

        private void Form1_Load (object sender, EventArgs e) {
            graph = pictureBox1.CreateGraphics();
            map = new int[width, height];
            pixel = 900 / width;
        }
    }
}
