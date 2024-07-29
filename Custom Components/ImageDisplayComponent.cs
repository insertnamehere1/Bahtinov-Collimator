using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    public partial class ImageDisplayComponent : UserControl
    {
        public ImageDisplayComponent()
        {
            InitializeComponent();
            SetupPictureBox();
            SetupSubscriptions();
        }

        private void SetupSubscriptions()
        {
            // Subscribe to the ImageReceivedEvent
            ImageCapture.ImageReceivedEvent += OnImageReceived;
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;
        }

        private void SetupPictureBox()
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.BackColor = UITheme.DisplayBackgroundColor;
        }

        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            Bitmap image = new Bitmap(e.Image);

            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() => UpdatePictureBox(image)));
            else
                UpdatePictureBox(image);
        }

        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            if (e.ChannelSelected[0] == true)
                Console.WriteLine("Channel 0 selected");
            else if (e.ChannelSelected[1] == true)
                Console.WriteLine("Channel 1 selected");
            else if (e.ChannelSelected[2] == true)
                Console.WriteLine("Channel 2 selected");
            else
                Console.WriteLine("No Channel Selected");
        }

        private void UpdatePictureBox(Bitmap image)
        {
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = image;
        }

        public void ClearDisplay()
        {
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
        }
    }
}






// draw line on image
//if (showFocus[group] == true)
//{
//                   if (index == 0 || index == 2)
//                        graphics.DrawLine(dashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
//                    else
//                        graphics.DrawLine(largeDashPen, lineStart_X, (float)height - lineStart_Y + (float)yOffset, lineEnd_X, (float)height - lineEnd_Y + (float)yOffset);
//}



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
