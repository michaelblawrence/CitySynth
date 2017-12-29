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
    public delegate void OnToggleChanged(object sender, ToggleEventArgs e);

    [DefaultEvent("ToggleChanged")]
    [DefaultProperty("Text")]
    public partial class SquareButton : UserControl
    {
        string mText;
        string pText;

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
                mText = pText = value;
            }
        }

        public event OnToggleChanged ToggleChanged;

        bool mInactive = false;

        [DefaultValue(false)]
        [Description("The apperance of the Control as either acented or normal")]
        public bool Inactive
        {
            get { return mInactive; }
            set
            {
                mInactive = value;
                Invalidate();
            }
        }


        bool mTextChangeOnSwitch = false;

        [DefaultValue(false)]
        [Description("The Text of the Control changes when either acented or normal")]
        public bool TextChangeOnSwitch
        {
            get { return mTextChangeOnSwitch; }
            set
            {
                mTextChangeOnSwitch = value;
                Invalidate();
            }
        }
        string mInactiveText;
        [Description("The text associated with the control when Inactive is true")]
        public string InactiveText
        {
            get { return mInactiveText; }
            set
            {
                mInactiveText = value;
            }
        }
        private bool mouseDown = false;

        public SquareButton()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int rbtn_w = CitySynth.Properties.Resources.ct_sqbtn.Width;
            int rbtn_h = CitySynth.Properties.Resources.ct_sqbtn.Height;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            RectangleF layoutRec = new RectangleF(0, 0, this.Width, 30);

            if (!mInactive)
                e.Graphics.DrawImage(
                mouseDown ? CitySynth.Properties.Resources.ct_sqbtn_dwn : CitySynth.Properties.Resources.ct_sqbtn,
                new Rectangle(0, 0, this.Width, this.Height));
            else
                e.Graphics.DrawImage(
                mouseDown ? CitySynth.Properties.Resources.ct_sqbtn_inactive_dwn : CitySynth.Properties.Resources.ct_sqbtn_inactive,
                new Rectangle(0, 0, this.Width, this.Height));
            float txt_h = e.Graphics.MeasureString(mText, TextTools.font, layoutRec.Size, TextTools.format).Height;
            layoutRec.Y = this.Height / 2 - txt_h / 2;
            e.Graphics.DrawString(mText.ToUpper(), TextTools.font, new SolidBrush(this.ForeColor), layoutRec, TextTools.format);
        }

        private void SquareButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mouseDown = true;
                Invalidate();
            }
        }

        private void SquareButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                PerformClick();
            }
        }

        public void PerformClick()
        {
            mouseDown = false;
            Inactive = !mInactive;
            mText = Inactive ? mInactiveText : pText;
            if (ToggleChanged != null) ToggleChanged.Invoke(this, new ToggleEventArgs(Inactive ? ToggleState.Inactive : ToggleState.Active));
            Invalidate();
        }
    }

    public class ToggleEventArgs : EventArgs
    {
        public ToggleState State;
        public ToggleEventArgs(ToggleState state)
        {
            this.State = state;
        }

        public string Message { get { return State.ToString(); } }
    }

    public enum ToggleState
    {
        Active,
        Inactive
    }
}
