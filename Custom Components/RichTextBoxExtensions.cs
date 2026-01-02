using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Custom_Components
{
    public static class RichTextBoxExtensions
    {
        public static void AppendTextLine(this RichTextBox rtb, string text)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.AppendText(text + Environment.NewLine);
        }

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
    }

}
