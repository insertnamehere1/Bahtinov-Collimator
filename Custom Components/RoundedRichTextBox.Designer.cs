using Bahtinov_Collimator;
using System.Drawing;
using System.Windows.Forms;

namespace SkyCal.Custom_Components
{
    sealed partial class TitledRoundedRichTextBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support — do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = UITheme.White;
            this.ForeColor = UITheme.TextBoxForeground;
            this.Name = "TitledRoundedRichTextBox";
            this.Padding = new Padding(10);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
