using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Bahtinov_Collimator
{
    /// <summary>
    /// Provides data for the ImageReceivedEvent.
    /// </summary>
    public class ImageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the image that was captured.
        /// </summary>
        public Bitmap Image { get; }

        /// <summary>
        /// Initializes a new instance of the ImageReceivedEventArgs class with the specified image.
        /// </summary>
        /// <param name="image">The captured image.</param>
        protected internal ImageReceivedEventArgs(Bitmap image)
        {
            Image = image;
        }
    }

    public class ChannelSelectEventArgs : EventArgs
    {
        public bool[] ChannelSelected;
        public int ChannelCount;

        protected internal ChannelSelectEventArgs(bool[] data, int channelCount)
        {
            ChannelSelected = data;
            ChannelCount = channelCount;
        }
    }

    public delegate void ImageLostEventHandler(object sender, ImageLostEventArgs e);

    public class ImageLostEventArgs : EventArgs
    {
        public string Message { get; }
        public string Title { get; }
        public MessageBoxIcon Icon { get; }
        public MessageBoxButtons Button { get; }


        public ImageLostEventArgs(string message, string title, MessageBoxIcon icon, MessageBoxButtons button)
        {
            Message = message;
            Title = title;
            Icon = icon;
            Button = button;
        }
    }

    public static class ImageLostEventProvider
    {
        // Define the event
        public static event ImageLostEventHandler ImageLostEvent;

        // Method to raise the event
        public static void OnImageLost(string message, string title, MessageBoxIcon icon, MessageBoxButtons button)
        {
            ImageLostEvent?.Invoke(null, new ImageLostEventArgs(message, title, icon, button));
        }
    }

    public class FocusDataEventArgs : EventArgs
    {
        public FocusData focusData;

        protected internal FocusDataEventArgs(FocusData focus)
        {
            focusData = focus;
        }
    }

    public class FocusData
    {
        public double BahtinovOffset { get; set; }
        public double DefocusError { get; set; }
        public bool InsideFocus { get; set; }
        public int Id { get; set; }
        public bool ClearDisplay { get; set; }
    }

    public class BahtinovLineDataEventArgs : EventArgs
    {
        public Bitmap Image { get; set; }
        public BahtinovLineData Linedata { get; private set; }
        public BahtinovLineDataEventArgs(BahtinovLineData linedata, Bitmap image)
        {
            Linedata = linedata;
            this.Image = new Bitmap(image);
        }

        public class BahtinovLineData
        {
            public int NumberOfGroups { get; set; } = 0;
            public List<LineGroup> LineGroups { get; private set; }

            public BahtinovLineData()
            {
                LineGroups = new List<LineGroup>();
            }

            public void AddLineGroup(LineGroup group)
            {
                if (LineGroups.Count < NumberOfGroups)
                {
                    LineGroups.Add(group);
                }
                else
                {
                    throw new InvalidOperationException("Cannot add more than 3 line groups.");
                }
            }
        }

        public class LineGroup
        {
            public int GroupId { get; set; }
            public List<Line> Lines { get; set; }
            public ErrorLine ErrorLine { get; set; }
            public ErrorCircle ErrorCircle { get; set; }

            public LineGroup(int groupId)
            {
                GroupId = groupId;
                Lines = new List<Line>();
            }

            public void AddLine(Line line)
            {
                if (Lines.Count < 3)
                {
                    Lines.Add(line);
                }
                else
                {
                    throw new InvalidOperationException("Cannot add more than 3 lines in a group.");
                }
            }
        }

        public class ErrorCircle
        {
            public Point Origin { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public bool InsideFocus { get; set; }
            public string ErrorValue { get; set; }

            public ErrorCircle(Point origin, int height, int width, bool insideFocus, string errorValue)
            {
                Origin = origin;
                Height = height;
                Width = width;
                InsideFocus = insideFocus;
                ErrorValue = errorValue;
            }
        }

        public class Line
        {
            public Point Start { get; set; }
            public Point End { get; set; }

            public int LineId { get; set; }

            public Line(Point start, Point end, int lineId)
            {
                Start = start;
                End = end;
                LineId = lineId;
            }
        }

        public class ErrorLine
        {
            public Point Start { get; set; }
            public Point End { get; set; }

            public ErrorLine(Point start, Point end)
            {
                Start = start;
                End = end;
            }
        }

        public class DefocusCircleEventArgs : EventArgs
        {
            public Bitmap Image { get; set; }
            public Point InnerCircleCentre { get; private set; }
            public Point OuterCircleCentre { get; private set; }
            public int InnerCircleRadius { get; private set; }
            public int OuterCircleRadius { get; private set; }

            public DefocusCircleEventArgs(Bitmap image, Point inner, int innerRadius, Point outer, int outerRadius)
            {
                this.Image = new Bitmap(image);
                this.InnerCircleCentre = inner;
                this.OuterCircleCentre = outer;
                this.InnerCircleRadius = innerRadius;
                this.OuterCircleRadius = outerRadius;
            }

        }
    }
}
