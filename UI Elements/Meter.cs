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
    public partial class Meter : UserControl
    {
        private float mLeftProgress = 0;
        private float mRightProgress = 0;
        private float pLeftProgress = 0;
        private float pRightProgress = 0;
        private string mText;
        private Brush brush = Brushes.White;

        public float BothProgress
        {
            set
            {
                float res;
                if (value < 0) res = 0;
                else if (value > 1) res = 1;
                else res = value;

                mLeftProgress = mRightProgress = res;
                if (mLeftProgress != pLeftProgress)
                {
                    //Refresh();
                    Invalidate();
                }
                pLeftProgress = pRightProgress = res;
            }
        }

        public float LeftProgress
        {
            get { return mLeftProgress; }
            set
            {
                if (value < 0) mLeftProgress = 0;
                else if (value > 1) mLeftProgress = 1;
                else mLeftProgress = value;
                Invalidate();
            }
        }

        public float RightProgress
        {
            get { return mRightProgress; }
            set
            {
                if (value < 0) mRightProgress = 0;
                else if (value > 1) mRightProgress = 1;
                else mRightProgress = value;
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

        public Meter()
        {
            InitializeComponent();
            this.ForeColorChanged += Meter_ForeColorChanged;
        }

        void Meter_ForeColorChanged(object sender, EventArgs e)
        {
            brush = new SolidBrush(Color.FromArgb(255, this.ForeColor));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int img_w = CitySynth.Properties.Resources.ctdialmeters.Width,
                img_h = CitySynth.Properties.Resources.ctdialmeters.Height;
            int x1 = this.Width / 2 - img_w / 2,
                y1 = this.Height - img_h;
            int x2 = this.Width / 2;

            RectangleF txtLayoutRec = new RectangleF(0, 0, this.Width, this.Height - img_h);

            float orrh = img_h-1;
            float y_l = (int)(y1 + ((1 - mLeftProgress) * orrh));
            float y_r = (int)(y1 + ((1 - mRightProgress) * orrh));
            //e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctdialmeters,
                new Rectangle(x1, y1, img_w, img_h));
            e.Graphics.DrawString(this.Text.ToUpper(), TextTools.font, brush,
                txtLayoutRec, TextTools.format);

            e.Graphics.TranslateTransform(this.Width / 2, y1 + orrh);
            e.Graphics.RotateTransform(180);
            e.Graphics.FillRectangle(brush,
                + 1, 0, 
                img_w / 2 - 2, orrh * mLeftProgress);
            e.Graphics.FillRectangle(brush,
                4 - img_w / 2 - 3, 0, 
                img_w / 2 - 2, orrh * mRightProgress);
        }
    }
}
