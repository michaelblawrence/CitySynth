using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Data;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CitySynth.UI_Elements
{
    public delegate void OnValueChanged(object sender, EventArgs e);

    public delegate void OnInactiveChanged(object sender, EventArgs e);

    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    public partial class Dial : UserControl
    {
        private float mDefaultValue = 0;
        [DefaultValue(0)]
        [Description("A floating point value, between 0.0 and 1.0, that represents the control's current value")]
        public float Value
        {
            get { return mValue; }
            set
            {
                if (value > 1) mValue = 1;
                else if (value < 0) mValue = 0;
                else mValue = value;
                if (pValue != mValue)
                {
                    ValueChanged?.Invoke(this, new EventArgs());
                }
                pValue = mValue;
                Invalidate();
            }
        }

        private bool mInactive = true;

        [DefaultValue(true)]
        [Description("Whether or not this control has been explicitly made active")]
        public bool Inactive
        {
            get { return mInactive; }
            set
            {
                var pInactive = mInactive;
                mInactive = value;
                if (mInactive != pInactive)
                {
                    InactiveChanged?.Invoke(this, new EventArgs());
                }
                Invalidate();
            }
        }

        private bool mEnableStandbyMode = true;

        [DefaultValue(true)]
        [Description("Whether or not this control will be affected by its Inactive property")]
        public bool EnableStandbyMode
        {
            get { return mEnableStandbyMode; }
            set
            {
                mEnableStandbyMode = value;
                Invalidate();
            }
        }

        private int mSnapSegments = 10;
        [DefaultValue(10)]
        [Description("Number of discrete points that Value can take")]
        public int SnapSegments
        {
            get { return mSnapSegments; }
            set
            {
                mSnapSegments = value;
                Invalidate();
            }
        }
        private bool mSnapToMarkings = false;
        [DefaultValue(false)]
        [Description("Whether or not this control will a continous or discrete set of value")]
        public bool SnapToMarkings
        {
            get { return mSnapToMarkings; }
            set
            {
                mSnapToMarkings = value;
                Invalidate();
            }
        }

        private float mMinimum = 0, mMaximum = 0;
        [DefaultValue(0)]
        [Description("A floating point value that represents the control's minimum value")]
        public float Minimum
        {
            get { return mMinimum; }
            set
            {
                mMinimum = value;
                Invalidate();
            }
        }
        [DefaultValue(0)]
        [Description("A floating point value that represents the control absolute maximum value")]
        public float Maximum
        {
            get { return mMaximum; }
            set
            {
                mMaximum = value;
                Invalidate();
            }
        }

        bool mLogScale = false;
        [DefaultValue(false)]
        [Description("Whether or not this control will vary logarithmically")]
        public bool LogScale
        {
            get { return mLogScale; }
        }

        public float GetScaledValue()
        {
            if (mLogScale) return (float)Math.Exp(mMinimum + (mMaximum - mMinimum) * mValue);
            else return mMinimum + (mMaximum - mMinimum) * mValue;
        }

        public void SetScaledValue(float value)
        {
            SetScaledValue(value, true);
        }
        public void SetScaledValue(float value, bool makeActive)
        {
            if (mLogScale) Value = (float)(Math.Log(value) - mMinimum) / (mMaximum - mMinimum);
            else Value = (value - mMinimum) / (mMaximum - mMinimum);
            if (makeActive) Inactive = false;
        }
        public void SetScaledValue(float value, bool makeActive, bool suppressEvent)
        {
            if (!suppressEvent) SetScaledValue(value, makeActive);
            else {
                if (mLogScale) mValue = Math.Min(1, Math.Max(0, (float)(Math.Log(value) - mMinimum) / (mMaximum - mMinimum)));
                else mValue = (value - mMinimum) / (mMaximum - mMinimum);
                pValue = mValue;
                if (makeActive) Inactive = false;
                else Invalidate();
            }
        }

        [DefaultValue(0)]
        public float DefaultValue
        {
            get { return mDefaultValue; }
            set
            {
                mDefaultValue = value;
                Invalidate();
            }
        }
        [DefaultValue(true)]
        [Description("Display static default background markings")]
        public bool DefaultMarkings
        {
            get { return mDefaultMarkings; }
            set
            {
                mDefaultMarkings = value;
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
                bool paint = mText != value;
                mText = value;
                if (paint) Invalidate();
                if(AutoSize)
                ResizeByText();
            }
        }

        public static long SelectedDialHandle = -1;

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get;
            set;
        }
        [Description("Occurs when the dial's value changes.")]
        public event OnValueChanged ValueChanged;

        [Description("Occurs when the dial's inactivity state changes.")]
        public event OnInactiveChanged InactiveChanged;

        private float mValue = 0;
        private float pValue = 0;
        private bool mDefaultMarkings = true;
        private bool hoverVisible = false;
        private string mText;

        private int lastY = 0;
        private DateTime lastTime = DateTime.Now;
        private float max_r = 274;
        private float dy;
        private static Bitmap ctdialround_bg = ImageTools.RotateColors(CitySynth.Properties.Resources.ctdialround_bg);
        private int img_w = CitySynth.Properties.Resources.ctdialround.Width;
        private int img_h = CitySynth.Properties.Resources.ctdialround.Height;

        private const int DEFAULT_CONTROL_WIDTH = 57;

        public Dial()
        {
            InitializeComponent();
            Text = this.Name;
            this.Padding = new Padding(3, 0, 3, 0);
            this.MouseWheel += Dial_MouseWheel;
        }

        void Dial_MouseWheel(object sender, MouseEventArgs e)
        {
            float wheel = e.Delta / 120f;
            mInactive = false;
            if (!mSnapToMarkings)
                Value += wheel / 60f;
            else
                Value = (float)(Math.Round(mValue * mSnapSegments) + wheel) / mSnapSegments;
            SelectedDialHandle = Handle.ToInt64();
            lastY = e.Y;
            lastTime = DateTime.Now;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int mk_w = CitySynth.Properties.Resources.ctdialmarker.Width;
            int mk_h = CitySynth.Properties.Resources.ctdialmarker.Height;

            Rectangle dialRec = new Rectangle(0, this.Height - img_h, img_w, img_h);

            if (mDefaultMarkings)
                e.Graphics.DrawImageUnscaledAndClipped(
                ctdialround_bg,
                new Rectangle(0, this.Height - img_h, img_w, img_h));

            e.Graphics.DrawImageUnscaledAndClipped(
                CitySynth.Properties.Resources.ctdialround,
                dialRec);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            RectangleF layoutRec = new RectangleF(0, -1, this.Width, this.Height - 50f);

            e.Graphics.DrawString(mText.ToUpper(), TextTools.font, new SolidBrush(this.ForeColor),
                layoutRec, TextTools.format);

            e.Graphics.TranslateTransform(img_w / 2f, this.Height - 25.5f);
            e.Graphics.RotateTransform(max_r * mValue);

            if (mInactive && mEnableStandbyMode)
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(190, 45, 45, 45)),
                new Rectangle((int)(9 - img_w / 2f),
                    (int)(this.Height + 20 - img_h - (this.Height - 25.5f)),
                    mk_w, mk_h));
            }
            e.Graphics.DrawImageUnscaledAndClipped(
                CitySynth.Properties.Resources.ctdialmarker,
                new Rectangle((int)(9 - img_w / 2f),
                    (int)(this.Height + 20 - img_h - (this.Height - 25.5f)),
                    mk_w, mk_h));
            e.Graphics.ResetTransform();


            if (hoverVisible && false)
            {
                int h_w = CitySynth.Properties.Resources.ctdialhoverbg.Width,
                    h_h = CitySynth.Properties.Resources.ctdialhoverbg.Height;
                e.Graphics.DrawImageUnscaledAndClipped(
                    CitySynth.Properties.Resources.ctdialhoverbg,
                    new Rectangle(this.Width / 2 - h_w / 2,
                        this.Height - img_h + 2 * img_h / 3 - h_h / 2, h_w, h_h)); 
            }

        }

        private void ResizeByText()
        {
            RectangleF layoutRec = new RectangleF(0, -3, this.Width, this.Height - 50f);
            Graphics g = Graphics.FromImage(new Bitmap(10, 10));
            SizeF sf = g.MeasureString(mText.ToUpper(), TextTools.font,
                layoutRec.Size, TextTools.format);
            float sw = sf.Width;
            float sh = sf.Height;
            if (this.Height < img_h + sh - 14)
                this.Height = (int)(img_h + sh - 5);
            g.Dispose();
            Invalidate();
        }

        public void SetValueSuppressed(float value)
        {
            if (value > 1) mValue = 1;
            else if (value < 0) mValue = 0;
            else mValue = value;
            pValue = mValue;
            Invalidate();
        }

        public void SetLogScale(bool enable)
        {
            mLogScale = true;
        }

        public void SetValueBounds(float minimum, float maximum)
        {
            this.Maximum = mLogScale ? (float)Math.Log(maximum) : maximum;
            this.Minimum = mLogScale ? (float)Math.Log(minimum) : minimum; 
        }

        private void Dial_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                dy = lastY - e.Y;

                if ((DateTime.Now - lastTime).TotalMilliseconds > 30)
                {
                    if (!mSnapToMarkings)
                        Value += 0.1f * dy / 10f;
                    else
                    {
                        mValue += 0.1f * dy / 10f;
                        if (mValue > 1) mValue = 1;
                        else if (mValue < 0) mValue = 0;
                        Invalidate();
                    }
                    SelectedDialHandle = Handle.ToInt64();
                    lastY = e.Y;
                    lastTime = DateTime.Now;
                }

            }
        }

        private void Dial_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                lastY = e.Y;
                hoverVisible = true;
                mInactive = false;
                Invalidate();
            }
#if DEBUG
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                MessageBox.Show(this.Handle.ToString());
            }
#endif
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // EDIT: ADD AN EXTRA HEIGHT VALIDATION TO AVOID INITIALIZATION PROBLEMS
            // BITWISE 'AND' OPERATION: IF ZERO THEN HEIGHT IS NOT INVOLVED IN THIS OPERATION
            if ((specified & BoundsSpecified.Width) == 0 || width == DEFAULT_CONTROL_WIDTH)
            {
                base.SetBoundsCore(x, y, DEFAULT_CONTROL_WIDTH, height, specified);
            }
            else
            {
                return; // RETURN WITHOUT DOING ANY RESIZING
            }
        }



        public void ResetToDefault()
        {
            this.Value = this.DefaultValue;
            this.Inactive = true;
        }

        private void Dial_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ResetToDefault();
        }

        private void Dial_MouseHover(object sender, EventArgs e)
        {
            hoverVisible = true;
            Invalidate();
        }

        private void Dial_MouseUp(object sender, MouseEventArgs e)
        {
            hoverVisible = false;
            if (mSnapToMarkings)
            {
                this.Value = (float)Math.Round(mValue * mSnapSegments) / mSnapSegments;
            }
            Invalidate();
        }

        private void Dial_MouseLeave(object sender, EventArgs e)
        {
            hoverVisible = false;
            Invalidate();
        }
    }
    public static class TextTools
    {
        private static PrivateFontCollection _pfc = new PrivateFontCollection();
        private static PrivateFontCollection _pfc1 = new PrivateFontCollection();


        public static FontFamily fontfamily = TextTools.LoadFontFamily(
                CitySynth.Properties.Resources.RobotoCondensed_Regular,
                out _pfc);
        public static FontFamily fontfamily1 = TextTools.LoadFontFamily(
                CitySynth.Properties.Resources.Roboto_Thin,
                out _pfc1);


        public static Font font = new Font(
            fontfamily, 9.6f, FontStyle.Bold, GraphicsUnit.Point);
        public static Font font1 = new Font(
            fontfamily1, 9.6f, FontStyle.Bold, GraphicsUnit.Point);
        private static StringFormat orrformat
        {
            get
            {
                StringFormat ft = new StringFormat(StringFormat.GenericTypographic);
                ft.Alignment = StringAlignment.Center;
                ft.FormatFlags = StringFormatFlags.DisplayFormatControl;
                return ft;
            }
        }
        private static StringFormat orrformat1
        {
            get
            {
                StringFormat ft = new StringFormat(StringFormat.GenericTypographic);
                ft.Alignment = StringAlignment.Near;
                ft.LineAlignment = StringAlignment.Center;
                ft.FormatFlags = StringFormatFlags.DisplayFormatControl;
                return ft;
            }
        }

        public static StringFormat format = new StringFormat(orrformat);
        public static StringFormat format1 = new StringFormat(orrformat1);

        public static FontFamily LoadFontFamily(byte[] buffer, out PrivateFontCollection fontCollection)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont(ptr, buffer.Length);
                return fontCollection.Families[0];
            }
            finally
            {
                // don't forget to unpin the array!
                handle.Free();
            }
        }
    }
}
