﻿namespace Sil.VS2012
{
    partial class SilViewHostWrapper
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
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.silViewHost1 = new SilUI.SilViewHost();
            this.SuspendLayout();
            // 
            // silViewHost1
            // 
            this.silViewHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.silViewHost1.Location = new System.Drawing.Point(0, 0);
            this.silViewHost1.Name = "silViewHost1";
            this.silViewHost1.Size = new System.Drawing.Size(150, 150);
            this.silViewHost1.TabIndex = 0;
            // 
            // SilViewHostWrapper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.silViewHost1);
            this.Name = "SilViewHostWrapper";
            this.ResumeLayout(false);

        }

        #endregion

        private SilUI.SilViewHost silViewHost1;
    }
}