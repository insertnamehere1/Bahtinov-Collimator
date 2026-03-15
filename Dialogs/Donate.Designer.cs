using Bahtinov_Collimator.Custom_Components;

namespace Bahtinov_Collimator
{
    partial class Donate
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Donate));
            this.cancelButton = new Bahtinov_Collimator.Custom_Components.RoundedButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.BevelDark = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cancelButton.BevelLight = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.BevelThickness = 2;
            this.cancelButton.CornerRadius = 12;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.HoverOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.ImageHeight = 32;
            this.cancelButton.ImageOffsetX = 60;
            this.cancelButton.ImageOffsetY = 0;
            this.cancelButton.ImageWidth = 32;
            this.cancelButton.Location = new System.Drawing.Point(421, 321);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.PressedOverlay = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cancelButton.Size = new System.Drawing.Size(106, 42);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Close";
            this.cancelButton.TextOffsetX = 10;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.Button2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackColor = System.Drawing.Color.LightGray;
            this.pictureBox1.Image = global::Bahtinov_Collimator.Properties.Resources.SkyCal_logo;
            this.pictureBox1.Location = new System.Drawing.Point(0, 45);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(171, 349);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.LightGray;
            this.pictureBox2.Image = global::Bahtinov_Collimator.Properties.Resources.paypal;
            this.pictureBox2.Location = new System.Drawing.Point(17, 64);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(135, 54);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // richTextBox
            // 
            this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox.Location = new System.Drawing.Point(195, 71);
            this.richTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.Size = new System.Drawing.Size(632, 227);
            this.richTextBox.TabIndex = 6;
            this.richTextBox.Text = "";
            // 
            // Donate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(854, 390);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Donate";
            this.Text = "Donate";
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.pictureBox2, 0);
            this.Controls.SetChildIndex(this.richTextBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private RoundedButton cancelButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.RichTextBox richTextBox;
    }
}