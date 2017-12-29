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
    public delegate void OnCheckedChanged(object sender, EventArgs e);

    [DefaultEvent("CheckedChanged")]
    [DefaultProperty("Checked")]
    public partial class Switch : UserControl
    {

        private static int xlim = 22;
        private int offset = 0;
        private int prevx = 0;
        private int prevx1 = 0;
        private int progress = 0;
        private string mText;
        private bool mChecked = false;
        private bool pChecked = false;

        private const int DEFAULT_CONTROL_WIDTH = 50;

        [DefaultValue(false)]
        public bool Checked
        {
            get { return mChecked; }
            set
            {
                mChecked = value;
                progress = mChecked ? xlim : 0;
                if (CheckedChanged != null && mChecked != pChecked)
                    CheckedChanged.Invoke(this, new EventArgs());
                pChecked = mChecked;
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

        public event OnCheckedChanged CheckedChanged;

        public Switch()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int bg_w = CitySynth.Properties.Resources.ctswitch_bg.Width,
                bg_h = CitySynth.Properties.Resources.ctswitch_bg.Height;
            int fg_w = CitySynth.Properties.Resources.ctswitch_dyn.Width,
                fg_h = CitySynth.Properties.Resources.ctswitch_dyn.Height;

            offset = (int)(this.Width - bg_w) / 2;

            e.Graphics.DrawImageUnscaledAndClipped(
                CitySynth.Properties.Resources.ctswitch_bg, new Rectangle(offset, this.Height - bg_h, bg_w, bg_h));
            e.Graphics.DrawImageUnscaledAndClipped(
                CitySynth.Properties.Resources.ctswitch_dyn, new Rectangle(progress + offset, this.Height - bg_h, fg_w, fg_h));

            e.Graphics.DrawString(this.Text.ToUpper(), TextTools.font, new SolidBrush(this.ForeColor),
                new RectangleF(0, -1, this.Width, this.Height - bg_h), TextTools.format);
        }

        private void Switch_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int dx = e.X - prevx;

                progress += dx;

                if (progress <= 0)
                {
                    progress = 0;
                    Checked = progress == xlim;
                }
                else if (progress >= xlim)
                {
                    progress = xlim;
                    Checked = progress == xlim;
                }

                prevx = e.X;
                Invalidate();
            }
        }

        private void Switch_MouseDown(object sender, MouseEventArgs e)
        {
            prevx = prevx1 = e.X;
        }

        private void Switch_MouseUp(object sender, MouseEventArgs e)
        {
            float _pro = (float)progress / xlim;
            if (_pro > 0 && _pro < 0.5) progress = 0;
            else if (_pro >= 0.5 && _pro < 1) progress = xlim;

            Checked = progress == xlim;

            Invalidate();
        }

        private void Switch_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int dx = e.X - prevx1;
                if (Math.Abs(dx) < 1)
                {
                    if (mChecked) progress = 0;
                    else progress = xlim;
                    Invalidate();
                }
            }
        }

        //protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        //{
        //    // EDIT: ADD AN EXTRA HEIGHT VALIDATION TO AVOID INITIALIZATION PROBLEMS
        //    // BITWISE 'AND' OPERATION: IF ZERO THEN HEIGHT IS NOT INVOLVED IN THIS OPERATION
        //    if ((specified & BoundsSpecified.Width) == 0 || width == DEFAULT_CONTROL_WIDTH)
        //    {
        //        base.SetBoundsCore(x, y, DEFAULT_CONTROL_WIDTH, height, specified);
        //    }
        //    else
        //    {
        //        return; // RETURN WITHOUT DOING ANY RESIZING
        //    }
        //}
    }
}
