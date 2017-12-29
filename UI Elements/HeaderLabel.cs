using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CitySynth.UI_Elements
{
    [DefaultProperty("Text")]
    public partial class HeaderLabel : UserControl
    {
        private string mText;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Bindable(true)]
        [Description("The text associated with the control")]
        public override string Text
        {
            get { return mText; }
            set
            {
                mText = value;
                Invalidate();
            }
        }

        public HeaderLabel()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.FromArgb(238, 129, 12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.DrawString(mText, TextTools.font, new SolidBrush(this.ForeColor),
                new RectangleF(0, 0, this.Width, this.Height), TextTools.format);
        }
    }
}
