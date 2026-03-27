using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    /// <summary>
    /// Extension helpers for appending text and inline images to a <see cref="RichTextBox"/>.
    /// </summary>
    public static class RichTextBoxExtensions
    {
        #region Text Helpers

        /// <summary>
        /// Appends a line of text and a trailing newline at the end of the target rich text box.
        /// </summary>
        /// <param name="rtb">The rich text box receiving the appended content.</param>
        /// <param name="text">The text content to append.</param>
        public static void AppendTextLine(this RichTextBox rtb, string text)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.AppendText(text + Environment.NewLine);
        }

        #endregion

        #region Image Helpers

        /// <summary>
        /// Appends an image at the current end of the rich text box by temporarily using the clipboard.
        /// </summary>
        /// <param name="rtb">The rich text box receiving the image.</param>
        /// <param name="image">The image to paste inline. If null, no change is made.</param>
        public static void AppendImageInline(this RichTextBox rtb, Image image)
        {
            if (image == null)
                return;

            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;

            IDataObject backup = Clipboard.GetDataObject();
            try
            {
                Clipboard.SetImage(image);
                rtb.ReadOnly = false;
                rtb.Paste();
                rtb.ReadOnly = true;
            }
            finally
            {
                if (backup != null)
                    Clipboard.SetDataObject(backup);
            }
        }

        #endregion
    }

}
