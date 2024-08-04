using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            public ErrorCircle(Point origin, int height, int width)
            {
                Origin = origin;
                Height = height;
                Width = width;
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
    }
}
