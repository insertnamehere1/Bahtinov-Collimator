using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class ImageDisplayComponent : UserControl
    {

        private List<Bitmap> layers;
        private int selectedGroup = 0;

        public ImageDisplayComponent()
        {
            InitializeComponent();
            SetupPictureBox();

            this.layers = new List<Bitmap>();
            AddNewLayers(4);

            SetupSubscriptions();
        }

        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
            ImageProcessing.BahtinovLineDrawEvent += OnBahtinovLineReceive;
            this.pictureBox1.Paint += new PaintEventHandler(this.pictureBoxPaint);
        }

        private void SetupPictureBox()
        {
            //            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.BackColor = UITheme.DisplayBackgroundColor;
        }

        private void AddNewLayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var layer = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                layers.Add(layer);
            }
        }

        private void pictureBoxPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(layers[0], 0, 0);

            if (selectedGroup == 0)
            {
                foreach (var layer in layers)
                {
                    e.Graphics.DrawImage(layer, 0, 0);
                }
            }
            else if (selectedGroup > 0 && selectedGroup < layers.Count)
            {
                e.Graphics.DrawImage(layers[selectedGroup], 0, 0);
            }
        }

        private void OnBahtinovLineReceive(object sender, BahtinovLineDataEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image, e.Linedata)));
            else
                UpdatePictureBox(image, e.Linedata);
        }

        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            selectedGroup = 0; // Default value

            for (int i = 0; i < e.ChannelSelected.Length; i++)
            {
                if (e.ChannelSelected[i])
                {
                    selectedGroup = i + 1;
                    break;
                }
            }

            pictureBox1.Invalidate();
        }

        private void UpdatePictureBox(Bitmap image, BahtinovLineDataEventArgs.BahtinovLineData data)
        {
            // Clear all layers
            foreach (var layer in layers)
            {
                using (Graphics g = Graphics.FromImage(layer))
                {
                    g.Clear(Color.Transparent);
                }
            }

            // Draw the image on the first layer
            using (Graphics g = Graphics.FromImage(layers[0]))
            {
                g.DrawImage(image, new Point(0, 0));
            }

            // Define the center and radius of the circle
            int canvasWidth = 600;
            int canvasHeight = 600;
            int centerX = canvasWidth / 2;
            int centerY = canvasHeight / 2;
            int radius = 280;

            // Create a circular clipping region
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);

                foreach (var group in data.LineGroups)
                {
                    using (Graphics g = Graphics.FromImage(layers[group.GroupId + 1]))
                    {
                        // Apply the clipping region
                        g.SetClip(path);

                        foreach (var line in group.Lines)
                        {
                            Pen pen = UITheme.GetDisplayLinePen(group.GroupId, line.LineId);
                            g.DrawLine(pen, line.Start, line.End);
                        }

                        // Reset the clipping region
                        g.ResetClip();
                    }
                }
            }

            pictureBox1.Invalidate();
        }

        public void ClearDisplay()
        {
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
        }

        public Bitmap InvertBitmapVertically(Bitmap original)
        {
            Bitmap inverted = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(inverted))
            {
                g.TranslateTransform(0, original.Height);
                g.ScaleTransform(1, -1);
                g.DrawImage(original, new Point(0, 0));
            }

            return inverted;
        }
    }
}



//private void LoadImageLost()
//{
//    Bitmap imageLost = new Bitmap(Properties.Resources.imageLost);
//            pictureBox.Image = imageLost;
//}



// display the error value in the pictureBox
//Font font2 = new Font("Arial", 16f);
//SolidBrush solidBrush = new SolidBrush(Color.White);

//Point textStartPoint = new Point((int)lineStart_X1, (int)((float)height - lineStart_Y1 + (float)yOffset));
//Point textEndPoint = new Point((int)lineEnd_X1, (int)((float)height - lineEnd_Y1 + (float)yOffset));

//            if (textStartPoint.X > pictureBox.Width - 50 || textStartPoint.Y > pictureBox.Height - 50)
//           {
//                textStartPoint.X = Math.Min(textStartPoint.X, pictureBox.Width - 50);
//                textStartPoint.Y = Math.Min(textStartPoint.Y, pictureBox.Height - 50);
//            }

//            if (textEndPoint.X > pictureBox.Width - 50 || textEndPoint.Y > pictureBox.Height - 50)
//            {
//                textEndPoint.X = Math.Min(textEndPoint.X, pictureBox.Width - 50);
//                textEndPoint.Y = Math.Min(textEndPoint.Y, pictureBox.Height - 50);
//            }

//            Point textLocation = group == 1 ? textEndPoint : textStartPoint;

//            if (showFocus[group] == true)
//                graphics.DrawString((errorSign * line2ToIntersectDistance).ToString("F1"), font2, (Brush)solidBrush, (PointF)textLocation);



