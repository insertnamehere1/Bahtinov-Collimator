using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Bahtinov_Collimator.Custom_Components
{
    public partial class CalibrationComponent : UserControl
    {
        private Form1 _parent;
        private float[] offsetValue = new float[3] { 0.0f, 0.0f, 0.0f };

        public CalibrationComponent(Form1 parent)
        {
            InitializeComponent();
            _parent = parent;
            SetupUI();
            SubscribeToEvents();
        }

        private void SetupUI()
        {

            float increasedSize = this.Font.Size + 2.0f;
            Font newFont = new Font(this.Font.FontFamily, increasedSize, this.Font.Style);

            // Adjust fonts
            roundedPanel1.Font = newFont;
            cancelButton.Font = newFont;
            saveButton.Font = newFont;
            titledRoundedRichTextBox1.Font = newFont;
            titledRoundedRichTextBox1.TitleFont = new Font(this.Font.FontFamily, increasedSize + 2.0f, FontStyle.Bold);

            roundedPanel1.BackColor = UITheme.DarkBackground;
            roundedPanel1.ForeColor = UITheme.MenuHighlightBackground;

            // Cancel Button Styling
            cancelButton.BackColor = UITheme.ButtonDarkBackground;
            cancelButton.ForeColor = UITheme.ButtonDarkForeground;
            cancelButton.FlatStyle = FlatStyle.Popup;
            cancelButton.CornerRadius = 6;
            cancelButton.TextOffsetX = 0;
            cancelButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            cancelButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // Save Button Styling
            saveButton.BackColor = UITheme.ButtonDarkBackground;
            saveButton.ForeColor = UITheme.ButtonDarkForeground;
            saveButton.FlatStyle = FlatStyle.Popup;
            saveButton.CornerRadius = 6;
            saveButton.TextOffsetX = 0;
            saveButton.BevelDark = Color.FromArgb(180, 90, 90, 90);
            saveButton.BevelLight = Color.FromArgb(220, 160, 160, 160);

            // RichTextBox Styling
            titledRoundedRichTextBox1.BackColor = UITheme.ButtonDarkBackground; // .TextBoxBackground;
            titledRoundedRichTextBox1.ForeColor = Color.White;
            titledRoundedRichTextBox1.TitleForeColor = Color.White;
            titledRoundedRichTextBox1.TitleBackColor = UITheme.DarkBackground;
            titledRoundedRichTextBox1.TitleText = "Calibration";

            UpdateCalibrationInfo();
        }



        /// <summary>
        /// Subscribes to necessary events for the component.
        /// </summary>
        private void SubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
        }

        /// <summary>
        /// Unsubscribes from events to avoid memory leaks.
        /// </summary>
        private void UnsubscribeToEvents()
        {
            BahtinovProcessing.FocusDataEvent -= FocusDataEvent;
        }


        /// <summary>
        /// Handles the focus data event to update label values.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="FocusDataEventArgs"/> instance containing the event data.</param>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FocusDataEventArgs>(FocusDataEvent), sender, e);
                return;
            }

            if (e.FocusData.Id >= 0)
            {
                if (!e.FocusData.ClearDisplay)
                    offsetValue[e.FocusData.Id] = (float)e.FocusData.BahtinovOffset;
                else
                    offsetValue[e.FocusData.Id] = 0.0f;

                UpdateCalibrationInfo();
            }
        }

        private void roundedButton1_Click(object sender, EventArgs e)
        {
            if (_parent != null)
            {
                UnsubscribeToEvents();
                _parent.StopCalibration();
            }
        }

        private void UpdateCalibrationInfo()
        {
            if(offsetValue[0] == 0.0f || offsetValue[1] == 0.0f || offsetValue[2] == 0.0f)
                titledRoundedRichTextBox1.Text = "Install either Bahtinov focus mask or Tri-Bahtinov collimation mask and capture the star image for display";
            else
                titledRoundedRichTextBox1.Text = "We are reading values.";
        }
    }
}
