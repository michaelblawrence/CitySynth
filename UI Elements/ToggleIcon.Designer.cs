namespace CitySynth.UI_Elements
{
    partial class ToggleIcon
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
            this.SuspendLayout();
            // 
            // ToggleIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true;
            this.Name = "ToggleIcon";
            this.Size = new System.Drawing.Size(44, 53);
            this.DoubleClick += new System.EventHandler(this.ToggleIcon_DoubleClick);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ToggleIcon_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToggleIcon_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ToggleIcon_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToggleIcon_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
