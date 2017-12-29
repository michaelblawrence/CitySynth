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
    public delegate void ValueChanged(object sender, EventArgs e);

    [DefaultProperty("Value")]
    [DefaultEvent("OnValueChanged")]
    public partial class Indicator : UserControl
    {
        private bool mValue = false;
        private bool pValue = false;
        private string mText;

        [DefaultValue(false)]
        public bool Value
        {
            get { return mValue; }
            set
            {
                mValue = value;
                if (OnValueChanged != null && mValue != pValue) OnValueChanged.Invoke(this, new EventArgs());
                pValue = mValue;

                Invalidate();
            }
        }
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

        public event ValueChanged OnValueChanged;

        public Indicator()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int img_w = CitySynth.Properties.Resources.ctledoff.Width,
                img_h = CitySynth.Properties.Resources.ctledoff.Height;
            int img1_w = CitySynth.Properties.Resources.ctledon1.Width,
                img1_h = CitySynth.Properties.Resources.ctledon1.Height;

            Rectangle txtLayoutRec = new Rectangle(0, 0, this.Width, this.Height - 12);
            Rectangle imgLayoutRec = new Rectangle(this.Width / 2 - img_w / 2, this.Height - img_h / 2 - 6 - this.Padding.Bottom, img_w, img_h);
            Rectangle img1LayoutRec = new Rectangle(this.Width / 2 - img1_w / 2, this.Height - img1_h / 2 - 6 - this.Padding.Bottom, img1_w, img1_h);

            if (!Value)
                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctledoff, imgLayoutRec);
            else
                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctledon1, img1LayoutRec);

            e.Graphics.DrawString(this.Text.ToUpper(), TextTools.font, new SolidBrush(this.ForeColor), txtLayoutRec, TextTools.format);

        }
    }
}
