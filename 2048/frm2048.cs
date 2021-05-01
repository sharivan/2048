using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using logic;

namespace _2048
{
    public partial class frm2048 : Form
    {
        const int PADDING = 14;
        const int SIZE = 120;
        const int ANIMATION_INTERVAL = 100;

        private static Color RGBtoColor(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xff, (rgb >> 8) & 0xff, rgb & 0xff);
        }

                                                                            //2                     4                      8                    16                      32                      64                          128                     256                         512                     1024                    2048                        4096
        private static readonly Color[] COLORS_FRONT = new Color[] { RGBtoColor(0x776E65), RGBtoColor(0x776E65), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF9F6F2), RGBtoColor(0xF8F5F2) };
        private static readonly Color[] COLORS_BACK = new Color[] { RGBtoColor(0xEEE4DA), RGBtoColor(0xEEE1C9), RGBtoColor(0xF3B27A), RGBtoColor(0xF69664), RGBtoColor(0xF77C5F), RGBtoColor(0xF7603B), RGBtoColor(0xEDD073), RGBtoColor(0xEDCC63), RGBtoColor(0xEDC950), RGBtoColor(0xEDC53F), RGBtoColor(0xEDC53F), RGBtoColor(0x3C3A32) };

        private class MovingLabel
        {
            public Label label;
            public int x0;
            public int y0;
            public int x;
            public int y;
            public float vx;
            public float vy;
            public int value;
            public Label unparentLabel;

            public MovingLabel(Label label, Point src, Point dst, int value, Label unparentLabel)
            {
                this.label = label;
                x0 = src.X;
                y0 = src.Y;
                x = dst.X;
                y = dst.Y;
                this.value = value;
                this.unparentLabel = unparentLabel;

                int deltaX = x - x0;
                int deltaY = y - y0;

                vx = (float) deltaX / ANIMATION_INTERVAL;
                vy = (float) deltaY / ANIMATION_INTERVAL;
            }
        }

        private bool animating;
        private int tickCounter;
        private Label[,] labels;
        private Logic2048 logic;

        private List<MovingLabel> movingLabels;

        public frm2048()
        {
            InitializeComponent();

            labels = new Label[4, 4];
            logic = new Logic2048(4, 4);

            movingLabels = new List<MovingLabel>();

            logic.OnReset += LogicReset;
            logic.OnSpawn += LogicSpawn;
            logic.OnMove += LogicMove;
            logic.OnFusion += LogicFusion;
            logic.OnMoveEnd += LogicMoveEnd;
            logic.OnGameOver += LogicGameOver;
        }

        private void LogicReset()
        {       
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    Label label = labels[row, col];
                    if (label != null)
                        label.Parent = null;

                    labels[row, col] = null;
                }
        }

        private void LogicSpawn(Entry entry, int row, int col)
        {
            CreateLabel(entry.Number, row, col);
        }

        private void LogicMove(Entry entry, int srcRow, int srcCol, int dstRow, int dstCol)
        {
            Label label = labels[srcRow, srcCol];
            movingLabels.Add(new MovingLabel(label, RowColToXY(srcRow, srcCol), RowColToXY(dstRow, dstCol), entry.Number, null));
            labels[dstRow, dstCol] = label;
            labels[srcRow, srcCol] = null;          
        }

        private void LogicFusion(Entry entry, int srcRow, int srcCol, int dstRow, int dstCol)
        {
            Label label = labels[srcRow, srcCol];
            movingLabels.Add(new MovingLabel(label, RowColToXY(srcRow, srcCol), RowColToXY(dstRow, dstCol), entry.Number, labels[dstRow, dstCol]));
            labels[dstRow, dstCol] = label;
            labels[srcRow, srcCol] = null;
        }

        private void LogicMoveEnd(bool valid)
        {
            if (valid)
            {
                tickCounter = 0;
                tmrAnimation.Enabled = true;
            }
            else
                animating = false;
        }

        private void LogicGameOver()
        {
            
        }

        private Point RowColToXY(int row, int col)
        {
            return new Point(PADDING + (SIZE + PADDING) * col, PADDING + (SIZE + PADDING) * row);
        }

        private int Log2(int x)
        {
            if (x == 0)
                throw new ArithmeticException("Log(0)");

            int counter = 0;
            while ((x & 1) == 0)
            {
                x >>= 1;
                counter++;
            }

            return counter;
        }

        private Color NumberToForecolor(int number)
        {
            int index = number > 4096 ? COLORS_FRONT.Length - 1 : Log2(number) - 1;
            return COLORS_FRONT[index];
        }

        public Color NumberToBackcolor(int number)
        {
            int index = number > 4096 ? COLORS_BACK.Length - 1 : Log2(number) - 1;
            return COLORS_BACK[index];
        }

        private Label CreateLabel(int value, int row, int col)
        {
            Label label = new Label();
            label.Text = value.ToString();
            label.Font = new Font("Arial", 21.75F, FontStyle.Bold);
            label.Size = new Size(SIZE, SIZE);
            label.ForeColor = NumberToForecolor(value);
            label.BackColor = NumberToBackcolor(value);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Location = RowColToXY(row, col);
            label.Parent = this;

            labels[row, col] = label;

            return label;
        }

        private void frm2048_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(PADDING + (SIZE + PADDING) * 4, PADDING + (SIZE + PADDING) * 4);

            logic.Reset();
        }

        private void frm2048_KeyUp(object sender, KeyEventArgs e)
        {
            if (animating)
                return;

            switch (e.KeyCode)
            {
                case Keys.Left:                    
                    animating = true;
                    logic.MoveLeft();
                    break;

                case Keys.Up:
                    animating = true;
                    logic.MoveUp();
                    break;

                case Keys.Right:                  
                    animating = true;
                    logic.MoveRight();
                    break;


                case Keys.Down:
                    animating = true;
                    logic.MoveDown();
                    break;
            }
        }

        private void RefreshPositions()
        {
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    Label label = labels[row, col];
                    if (label != null)
                        label.Location = RowColToXY(row, col);
                }
        }

        private void tmrAnimation_Tick(object sender, EventArgs e)
        {
            foreach (MovingLabel ml in movingLabels)
            {
                Label label = ml.label;
                Point location = label.Location;

                label.Location = new Point(location.X + (int) (ml.vx * tmrAnimation.Interval), location.Y + (int) (ml.vy * tmrAnimation.Interval));
            }

            tickCounter += tmrAnimation.Interval;
            if (tickCounter >= ANIMATION_INTERVAL)
            {
                foreach (MovingLabel ml in movingLabels)
                {
                    Label label = ml.label;
                    label.Text = ml.value.ToString();
                    label.ForeColor = NumberToForecolor(ml.value);
                    label.BackColor = NumberToBackcolor(ml.value);

                    if (ml.unparentLabel != null)
                        ml.unparentLabel.Parent = null;
                }

                animating = false;
                tmrAnimation.Enabled = false;
                movingLabels.Clear();

                RefreshPositions();
            }
        }
    }
}
