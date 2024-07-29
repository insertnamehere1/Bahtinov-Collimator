using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
}
