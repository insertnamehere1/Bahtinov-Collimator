using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Bahtinov_Collimator.Helper;

namespace Bahtinov_Collimator
{
    #region ImageReceivedEvent

    /// <summary>
    /// Provides data for the ImageReceivedEvent. This class contains information about an image that was captured.
    /// </summary>
    public class ImageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the image that was captured.
        /// </summary>
        public Bitmap Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageReceivedEventArgs"/> class with the specified image.
        /// </summary>
        /// <param name="image">The captured image that this event is reporting. This parameter cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/> parameter is null.</exception>
        protected internal ImageReceivedEventArgs(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Image = image;
        }
    }
    #endregion

    #region ChannelSelectEvent

    /// <summary>
    /// Provides data for the ChannelSelectEvent. This class contains information about selected channels and the total number of channels.
    /// </summary>
    public class ChannelSelectEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the array of boolean values indicating which channels are selected.
        /// </summary>
        public bool[] ChannelSelected { get; }

        /// <summary>
        /// Gets the total number of channels.
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSelectEventArgs"/> class with the specified channel selection data and channel count.
        /// </summary>
        /// <param name="data">An array of boolean values where each value indicates whether a corresponding channel is selected. This parameter cannot be null.</param>
        /// <param name="channelCount">The total number of channels. This parameter should match the length of the <paramref name="data"/> array.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the length of <paramref name="data"/> does not match the <paramref name="channelCount"/>.</exception>
        protected internal ChannelSelectEventArgs(bool[] data, int channelCount)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ChannelSelected = data;
            ChannelCount = channelCount;
        }
    }
    #endregion

    #region ImageLostEvent

    /// <summary>
    /// Represents the method that will handle the ImageLost event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ImageLostEventArgs"/> that contains the event data.</param>
    public delegate void ImageLostEventHandler(object sender, ImageLostEventArgs e);

    /// <summary>
    /// Provides data for the ImageLost event. This class contains information about the message to be displayed when an image is lost.
    /// </summary>
    public class ImageLostEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message that describes the image loss event.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the title of the message box to be displayed.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the icon to be displayed in the message box.
        /// </summary>
        public MessageBoxIcon Icon { get; }

        /// <summary>
        /// Gets the buttons to be displayed in the message box.
        /// </summary>
        public MessageBoxButtons Button { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLostEventArgs"/> class with the specified message, title, icon, and buttons.
        /// </summary>
        /// <param name="message">The message to be displayed in the message box. This parameter cannot be null or empty.</param>
        /// <param name="title">The title of the message box. This parameter cannot be null or empty.</param>
        /// <param name="icon">The icon to be displayed in the message box.</param>
        /// <param name="button">The buttons to be displayed in the message box.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="message"/> or <paramref name="title"/> parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> or <paramref name="title"/> parameters are empty.</exception>
        public ImageLostEventArgs(string message, string title, MessageBoxIcon icon, MessageBoxButtons button)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.", nameof(title));

            Message = message;
            Title = title;
            Icon = icon;
            Button = button;
        }
    }

    /// <summary>
    /// Provides methods to raise the ImageLost event and manage event subscriptions.
    /// </summary>
    public static class ImageLostEventProvider
    {
        /// <summary>
        /// Occurs when an image is lost and needs to notify subscribers.
        /// </summary>
        public static event ImageLostEventHandler ImageLostEvent;

        /// <summary>
        /// Raises the ImageLost event with the specified parameters.
        /// </summary>
        /// <param name="message">The message to be displayed in the message box.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="icon">The icon to be displayed in the message box.</param>
        /// <param name="button">The buttons to be displayed in the message box.</param>
        public static void OnImageLost(string message, string title, MessageBoxIcon icon, MessageBoxButtons button)
        {
            // Invoke the event if there are subscribers
            ImageLostEvent?.Invoke(null, new ImageLostEventArgs(message, title, icon, button));
        }
    }
    #endregion

    #region FocusDataEvent

    /// <summary>
    /// Provides data for the FocusData event. This class contains focus-related data.
    /// </summary>
    public class FocusDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the focus data associated with the event.
        /// </summary>
        public FocusData FocusData { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusDataEventArgs"/> class with the specified focus data.
        /// </summary>
        /// <param name="focus">The focus data associated with the event.</param>
        protected internal FocusDataEventArgs(FocusData focus)
        {
            FocusData = focus ?? throw new ArgumentNullException(nameof(focus), "Focus data cannot be null.");
        }
    }

    /// <summary>
    /// Represents focus-related data used in focus-related events.
    /// </summary>
    public class FocusData
    {
        /// <summary>
        /// Gets or sets the Bahtinov offset value.
        /// </summary>
        public double BahtinovOffset { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the focus data.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the display should be cleared.
        /// </summary>
        public bool ClearDisplay { get; set; }
    }
    #endregion

    #region BahtinovLineEvent

    /// <summary>
    /// Provides data for the BahtinovLineData event, including an image and line data.
    /// </summary>
    public class BahtinovLineDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the image associated with the event.
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// Gets the Bahtinov line data associated with the event.
        /// </summary>
        public BahtinovLineData Linedata { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BahtinovLineDataEventArgs"/> class with the specified line data and image.
        /// </summary>
        /// <param name="linedata">The Bahtinov line data.</param>
        /// <param name="image">The image associated with the line data.</param>
        public BahtinovLineDataEventArgs(BahtinovLineData linedata, Bitmap image)
        {
            Linedata = linedata;
            Image = new Bitmap(image);
        }

        /// <summary>
        /// Represents the line data in a Bahtinov mask analysis, including line groups.
        /// </summary>
        public class BahtinovLineData
        {
            /// <summary>
            /// Gets or sets the number of groups allowed in the line data.
            /// </summary>
            public int NumberOfGroups { get; set; } = 0;

            /// <summary>
            /// Gets the list of line groups in the line data.
            /// </summary>
            public List<LineGroup> LineGroups { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="BahtinovLineData"/> class.
            /// </summary>
            public BahtinovLineData()
            {
                LineGroups = new List<LineGroup>();
            }

            /// <summary>
            /// Adds a line group to the line data.
            /// </summary>
            /// <param name="group">The line group to add.</param>
            /// <exception cref="InvalidOperationException">Thrown when attempting to add more than the allowed number of line groups.</exception>
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

        /// <summary>
        /// Represents a group of lines in the Bahtinov mask analysis.
        /// </summary>
        public class LineGroup
        {
            /// <summary>
            /// Gets or sets the unique identifier for the line group.
            /// </summary>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets the list of lines in the group.
            /// </summary>
            public List<Line> Lines { get; set; }

            /// <summary>
            /// Gets or sets the error line associated with the line group.
            /// </summary>
            public ErrorLine ErrorLine { get; set; }

            /// <summary>
            /// Gets or sets the error circle associated with the line group.
            /// </summary>
            public ErrorCircle ErrorCircle { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LineGroup"/> class with the specified group ID.
            /// </summary>
            /// <param name="groupId">The unique identifier for the line group.</param>
            public LineGroup(int groupId)
            {
                GroupId = groupId;
                Lines = new List<Line>();
            }

            /// <summary>
            /// Adds a line to the line group.
            /// </summary>
            /// <param name="line">The line to add.</param>
            /// <exception cref="InvalidOperationException">Thrown when attempting to add more than 3 lines to the group.</exception>
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

        /// <summary>
        /// Represents an error circle in the Bahtinov mask analysis.
        /// </summary>
        public class ErrorCircle
        {
            /// <summary>
            /// Gets or sets the origin point of the error circle.
            /// </summary>
            public Point Origin { get; set; }

            /// <summary>
            /// Gets or sets the height of the error circle.
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Gets or sets the width of the error circle.
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            /// Gets or sets the error value associated with the error circle.
            /// </summary>
            public string ErrorValue { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ErrorCircle"/> class with the specified parameters.
            /// </summary>
            /// <param name="origin">The origin point of the error circle.</param>
            /// <param name="height">The height of the error circle.</param>
            /// <param name="width">The width of the error circle.</param>
            /// <param name="errorValue">The error value associated with the error circle.</param>
            public ErrorCircle(Point origin, int height, int width, string errorValue)
            {
                Origin = origin;
                Height = height;
                Width = width;
                ErrorValue = errorValue;
            }
        }

        /// <summary>
        /// Represents a line in the Bahtinov mask analysis.
        /// </summary>
        public class Line
        {
            /// <summary>
            /// Gets or sets the starting point of the line.
            /// </summary>
            public Point Start { get; set; }

            /// <summary>
            /// Gets or sets the ending point of the line.
            /// </summary>
            public Point End { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier for the line.
            /// </summary>
            public int LineId { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Line"/> class with the specified parameters.
            /// </summary>
            /// <param name="start">The starting point of the line.</param>
            /// <param name="end">The ending point of the line.</param>
            /// <param name="lineId">The unique identifier for the line.</param>
            public Line(Point start, Point end, int lineId)
            {
                Start = start;
                End = end;
                LineId = lineId;
            }
        }

        /// <summary>
        /// Represents an error line in the Bahtinov mask analysis.
        /// </summary>
        public class ErrorLine
        {
            /// <summary>
            /// Gets or sets the starting point of the error line.
            /// </summary>
            public Point Start { get; set; }

            /// <summary>
            /// Gets or sets the ending point of the error line.
            /// </summary>
            public Point End { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ErrorLine"/> class with the specified parameters.
            /// </summary>
            /// <param name="start">The starting point of the error line.</param>
            /// <param name="end">The ending point of the error line.</param>
            public ErrorLine(Point start, Point end)
            {
                Start = start;
                End = end;
            }
        }

        /// <summary>
        /// Provides data for the DefocusCircle event, including details of the inner and outer circles.
        /// </summary>
        public class DefocusCircleEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the image associated with the defocus circle event.
            /// </summary>
            public Bitmap Image { get; set; }

            /// <summary>
            /// Gets the center point of the inner circle.
            /// </summary>
            public PointD InnerCircleCentre { get; private set; }

            /// <summary>
            /// Gets the center point of the outer circle.
            /// </summary>
            public PointD OuterCircleCentre { get; private set; }

            /// <summary>
            /// Gets the radius of the inner circle.
            /// </summary>
            public double InnerCircleRadius { get; private set; }

            /// <summary>
            /// Gets the radius of the outer circle.
            /// </summary>
            public double OuterCircleRadius { get; private set; }

            /// <summary>
            /// Gets the direction of the defocus circle.
            /// </summary>
            public double Direction { get; private set; }

            /// <summary>
            /// Gets the distance from the center of the defocus circle.
            /// </summary>
            public double Distance { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DefocusCircleEventArgs"/> class with the specified parameters.
            /// </summary>
            /// <param name="image">The image associated with the defocus circle event.</param>
            /// <param name="inner">The center point of the inner circle.</param>
            /// <param name="innerRadius">The radius of the inner circle.</param>
            /// <param name="outer">The center point of the outer circle.</param>
            /// <param name="outerRadius">The radius of the outer circle.</param>
            /// <param name="distance">The distance from the center of the defocus circle.</param>
            /// <param name="direction">The direction of the defocus circle.</param>
            public DefocusCircleEventArgs(Bitmap image, PointD inner, double innerRadius, PointD outer, double outerRadius, double distance, double direction)
            {
                Image = new Bitmap(image);
                InnerCircleCentre = inner;
                OuterCircleCentre = outer;
                InnerCircleRadius = innerRadius;
                OuterCircleRadius = outerRadius;
                Distance = distance;
                Direction = direction;
            }
        }
    }
    #endregion
}
