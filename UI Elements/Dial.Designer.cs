namespace CitySynth.UI_Elements
{
    partial class Dial
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
            // Dial
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(86, 100);
            this.Name = "Dial";
            this.Size = new System.Drawing.Size(86, 100);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Dial_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Dial_MouseDown);
            this.MouseLeave += new System.EventHandler(this.Dial_MouseLeave);
            this.MouseHover += new System.EventHandler(this.Dial_MouseHover);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Dial_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Dial_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
