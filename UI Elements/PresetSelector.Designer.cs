namespace CitySynth.UI_Elements
{
    partial class PresetSelector
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
            // PresetSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(329, 27);
            this.Name = "PresetSelector";
            this.Size = new System.Drawing.Size(329, 27);
            this.Leave += new System.EventHandler(this.PresetSelector_Leave);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PresetSelector_MouseClick);
            this.MouseEnter += new System.EventHandler(this.PresetSelector_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.PresetSelector_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PresetSelector_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
