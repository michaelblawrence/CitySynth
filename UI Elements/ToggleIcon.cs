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
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckChanged")]
    public partial class ToggleIcon : UserControl
    {
        public event OnCheckedChanged CheckedChanged;
        private bool mChecked;
        private bool pChecked;
        private string mText;
        private bool mDefaultChecked;
        private bool mDraggable;
        private string pText = "";

        [DefaultValue(false)]
        public bool Draggable
        {
            get { return mDraggable; }
            set
            {
                mDraggable = value;
            }
        }
        [DefaultValue(false)]
        public bool DefaultChecked
        {
            get { return mDefaultChecked; }
            set
            {
                mDefaultChecked = value;
            }
        }
        [DefaultValue(false)]
        public bool Checked
        {
            get { return mChecked; }
            set
            {
                mChecked = value;
                if (CheckedChanged != null && pChecked != mChecked)
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
                pText = mText = value;
                Invalidate();
            }
        }


        public ToggleIcon()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int img_w = CitySynth.Properties.Resources.ct_toggleoff.Width,
                img_h = CitySynth.Properties.Resources.ct_toggleon.Height;
            Rectangle imgLayoutRec = new Rectangle(this.Width / 2 - img_w / 2, this.Height - img_h, img_w, img_h);
            Rectangle txtLayoutRec = new Rectangle(0, 0, this.Width, this.Height + 5 - img_h);

            if (!Checked)
                e.Graphics.DrawImageUnscaledAndClipped(
                    CitySynth.Properties.Resources.ct_toggleoff, imgLayoutRec);
            else
                e.Graphics.DrawImageUnscaledAndClipped(
                    CitySynth.Properties.Resources.ct_toggleon, imgLayoutRec);

            if (this.Text != null && this.Text.Trim() != "")
            e.Graphics.DrawString(this.Text.ToUpper(), TextTools.font, new SolidBrush(this.ForeColor),
                txtLayoutRec, TextTools.format);
        }

        private void ToggleIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Checked = !Checked;
        }

        private void ToggleIcon_DoubleClick(object sender, EventArgs e)
        {
            ResetToDefault();
        }

        public void ResetToDefault()
        {
            Checked = DefaultChecked;

        }

        private void ToggleIcon_MouseDown(object sender, MouseEventArgs e)
        {
            //if (mDraggable)
            //{
            //    this.Cursor = Cursors.Cross;
            //}
        }

        private void ToggleIcon_MouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == System.Windows.Forms.MouseButtons.Left && mDraggable)
            //{
            //    Type t = (this.GetContainerControl()).GetType();
            //    if (t == CitySynth.Form1.F1)
            //        this.mText =
            //            ((Form)this.GetContainerControl()).GetChildAtPoint(new Point(e.X, e.Y)).Text;
            //    Invalidate();
            //}
            //this.Cursor = Cursors.Default;
            //this.Text = pText;
        }

        private void ToggleIcon_MouseMove(object sender, MouseEventArgs e)
        {
        }

        public void ChangeDisplayText(string text)
        {
            pText = mText;
            mText = text;
            Invalidate();
        }

        public void RestoreDisplayText()
        {
            this.Text = pText;
        }
    }
}
