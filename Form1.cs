// Sections of the following code are covered by and existing copyright
//
//MIT License

//Copyright (c) 2017 Wytse Jan Posthumus

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bahtinov_Collimator.AdjustAssistant;
using static Bahtinov_Collimator.BahtinovLineDataEventArgs;

namespace Bahtinov_Collimator
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 19;

        // Focus Group Boxes
        FocusChannelComponent groupBoxRed;
        FocusChannelComponent groupBoxGreen;
        FocusChannelComponent groupBoxBlue;

        // new image capture, first pass
        private bool firstPassCompleted = false;

        // image Processing Class
        private ImageProcessing imageProcessing;

        // Bahtinove line data
        private BahtinovData bahtinovLineData;

        // Adjust Assistant
        private AdjustAssist adjustAssistDialog;

        public Form1()
        {
            SetProcessDPIAware();
            InitializeComponent();
            InitializeRedFocusBox();
            SetFormUI();
            SetColorScheme();
            SetEvents();

            imageProcessing = new ImageProcessing();
            menuStrip1.Renderer = new CustomToolStripRenderer();
        }

        private void InitializeRedFocusBox()
        {
            groupBoxRed = new FocusChannelComponent(0)
            {
                Location = new Point(10, 30)
            };
            this.Controls.Add(groupBoxRed);
        }

        private void InitializeGreenFocusBox()
        {
            groupBoxGreen = new FocusChannelComponent(1)
            {
                Location = new Point(10, 150)
            };
            this.Controls.Add(groupBoxGreen);
        }

        private void InitializeBlueFocusBox()
        {
            groupBoxBlue = new FocusChannelComponent(2)
            {
                Location = new Point(10, 270)
            };
            this.Controls.Add(groupBoxBlue);
        }

        private void SetFormUI()
        {
            AdjustAssistToolStripMenuItem.Visible = false;
            SetWindowTitleWithVersion();
        }

        private void SetEvents()
        {
            ImageCapture.ImageReceivedEvent += OnImageReceived;
        }

        private void SetColorScheme()
        {
            // main form
            this.ForeColor = UITheme.DarkForeground;
            this.BackColor = UITheme.DarkBackground;

            // menu strip
            menuStrip1.BackColor = UITheme.DarkBackground;
            menuStrip1.ForeColor = UITheme.MenuStripForeground;
            SetMenuItemsColor(menuStrip1.Items, UITheme.MenuDarkBackground, UITheme.MenuDarkForeground);

            // start button
            StartButton.BackColor = UITheme.ButtonDarkBackground;
            StartButton.ForeColor = UITheme.ButtonDarkForeground;
            StartButton.FlatStyle = FlatStyle.Popup;

            // Titlebar
            var color = UITheme.DarkBackground;
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref colorValue, sizeof(int));
        }

        private void SetMenuItemsColor(ToolStripItemCollection items, Color backColor, Color foreColor)
        {
            foreach (ToolStripMenuItem item in items)
            {
                item.BackColor = backColor;
                item.ForeColor = foreColor;
                if (item.HasDropDownItems)
                {
                    SetMenuItemsColor(item.DropDownItems, backColor, foreColor);
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            ImageCapture.StopImageCapture();

            if (ImageCapture.SelectStar())
            {
                ImageCapture.StartImageCapture();
                firstPassCompleted = false;
                ImageCapture.TrackingEnabled = true;
            }
        }

        private void OnImageReceived(object sender, ImageReceivedEventArgs e)
        {
            // Run the processing on a background thread
            Task.Run(() => ProcessAndDisplay(e.Image));
        }

        private void ProcessAndDisplay(Bitmap original)
        {
            Bitmap image = new Bitmap(original);

            // Find the Bahtinov lines
            if (!firstPassCompleted)
            {
                bahtinovLineData = imageProcessing.FindBrightestLines(image);
                bahtinovLineData.Sort();
            }

            // Second pass to find the fine details
            bahtinovLineData = imageProcessing.FindSubpixelLines(image, bahtinovLineData.LineValue.Length, bahtinovLineData);

            // Update UI based on the number of lines detected
            int numberOfLines = bahtinovLineData.LineAngles.Length;

            if (!firstPassCompleted)
            {
                UpdateFocusGroup(numberOfLines);
            }

            imageProcessing.DisplayLines(bahtinovLineData, image);
        }

        private void ShowWarningMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowWarningMessage(message)));
            }
            else
            {
                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ImageCapture.StopImageCapture();
            }
        }

        private void UpdateFocusGroup(int numberOfLines)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateFocusGroup(numberOfLines)));
            }
            else
            {
                RemoveAndDisposeControls(groupBoxGreen, groupBoxBlue);

                if (numberOfLines == 9)
                {
                    InitializeRedFocusBox();
                    InitializeGreenFocusBox();
                    InitializeBlueFocusBox();
                    AdjustAssistToolStripMenuItem.Visible = true;
                }
                else
                {
                    AdjustAssistToolStripMenuItem.Visible = false;
                }

                firstPassCompleted = true;
            }
        }

        private void QuitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Activate();
        }

        private void AboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();

        }

        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Donate donate = new Donate();
            donate.ShowDialog();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo();
        }

        private void InstallUpdateSyncWithInfo()
        {
            Helper.GetLatestUpdate();
        }

        private void cheatSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (adjustAssistDialog == null || adjustAssistDialog.IsDisposed)
            {
                adjustAssistDialog = new AdjustAssistant.AdjustAssist(this);
                adjustAssistDialog.Show(this);
                adjustAssistDialog.FormClosed += CheatSheet_FormClosed;
            }
        }

        private void CheatSheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Set adjustAssistDialog instance to null when the form is closed
            adjustAssistDialog = null;
        }

        private void RemoveAndDisposeControls(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (Controls.Contains(control))
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
            }
        }

        private void SetWindowTitleWithVersion()
        {
            // Get the version number of the assembly
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Create the version string
            string versionString = $"V {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            // Set the form's title
            this.Text = $"Bahtinov Collimator - {versionString}";
        }

        private void SettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Settings Dialog
            Settings settingsDialog = new Settings();

            DialogResult result = settingsDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                imageProcessing.LoadSettings();
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        // Custom renderer to change the color of selected items
        private class CustomToolStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // Handle the background rendering for the menu items
                if (e.Item.Selected || e.Item.Pressed)
                {
                    SolidBrush menuBackgroundBrush = new SolidBrush(UITheme.MenuHighlightBackground);

                    e.Graphics.FillRectangle(menuBackgroundBrush, e.Item.ContentRectangle);
                    e.Item.ForeColor = UITheme.MenuDarkForeground;
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = UITheme.MenuDarkForeground;
                base.OnRenderItemText(e);
            }
        }

    }
}
