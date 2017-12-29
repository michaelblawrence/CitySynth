using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CitySynth.UI_Elements
{
    [DefaultEvent("SelectedIndexChanged")]
    [DefaultProperty("SelectedIndex")]
    public partial class PresetSelector : UserControl
    {
        private bool mDropVisible = false, mDragScroll = false;
        private int pHeight = 0;
        private Timer timeout = new Timer();

        private const int items = 8;
        private Rectangle list;
        private RectangleF[] dropItemRecs = null;
        private int listOffset = 0;

        private int lastY = 0;
        private DateTime lastTime = DateTime.Now;
        private float dy, mScroll = 0, scr_h = 25;
        private int mIndex = -1;
        private int pIndex = -1;
        private List<string> mItems = new List<string>();
        private Rectangle btn1, btn2, sel, s_dwn, s_up, scroll;
        public event OnSelectedIndexChanged SelectedIndexChanged;
        public event OnPresetButtonClicked PresetButtonClicked;


        public List<string> Items { get { return mItems; } set { value = mItems; } }
        public int SelectedIndex
        {
            get { return mIndex; }
            set
            {
                if (value < 0 || value > mItems.Count)
                    throw new ArgumentOutOfRangeException();
                else
                {
                    mIndex = value;
                    if (SelectedIndexChanged != null && pIndex != mIndex)
                    {
                        SelectedIndexChanged.Invoke(this, new EventArgs());

                        listOffset = mIndex - 1;
                        listOffset = Math.Max(0, Math.Min(listOffset, mItems.Count - items));
                        mScroll = (int)(scr_h * listOffset / (mItems.Count - items));
                    }
                    Invalidate();
                    pIndex = mIndex;
                }
            }
        }
        [Browsable(false)]
        public string SelectedItem
        {
            get
            {
                if (mIndex >= 0 && mIndex < mItems.Count)  
                    return mItems[mIndex];
                else 
                    return null;
            }
        }

        private bool mScrollLoop = true;
        public bool ScrollLoop
        {
            get
            {
                return mScrollLoop;
            }
            set
            {
                mScrollLoop = value;
            }
        }

        public PresetSelector()
        {
            InitializeComponent();
            mItems.Add("(0) reset");
            mItems.Add("(1) reset1");
            mItems.Add("(2) reset2");
            mItems.Add("(3) reset3");
            this.MouseWheel += PresetSelector_MouseWheel;
            this.MouseDown += PresetSelector_MouseDown;

            list = new Rectangle(13, 5, 242, 165);

            int bg_w = CitySynth.Properties.Resources.ctselbar.Width,
                bg_h = CitySynth.Properties.Resources.ctselbar.Height;
            if (dropItemRecs == null)
            {
                dropItemRecs = new RectangleF[items];
                for (int i = 0; i < items; i++)
                {
                    float ht = list.Height / (items) + 0.4f;
                    float yt = list.Y + (float)i * ht + bg_h;
                    dropItemRecs[i] = new RectangleF(13, yt, 242, ht);
                }
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            int bg_w = CitySynth.Properties.Resources.ctselbar.Width,
                bg_h = CitySynth.Properties.Resources.ctselbar.Height;
            int btn_w = CitySynth.Properties.Resources.ctselbarbtn.Width - 1,
                btn_h = CitySynth.Properties.Resources.ctselbarbtn.Height;
            int sud_w = CitySynth.Properties.Resources.ctselbar_arrow_up.Width,
                sud_h = CitySynth.Properties.Resources.ctselbar_arrow_up.Height;
            int scrfg_w = CitySynth.Properties.Resources.ctseldropdownscrollfg.Width,
                scrfg_h = CitySynth.Properties.Resources.ctseldropdownscrollfg.Height;
            int scr0 = CitySynth.Properties.Resources.ctselddscroll_top.Height,
                scr1 = CitySynth.Properties.Resources.ctselddscroll_mid.Height,
                scr2 = CitySynth.Properties.Resources.ctselddscroll_btm.Height;

            sel = new Rectangle(0, 0, bg_w, bg_h);
            btn1 = new Rectangle(bg_w - 1, 0, btn_w, btn_h);
            btn2 = new Rectangle(bg_w - 1 + btn_w, 0, btn_w, btn_h);
            s_up = new Rectangle(bg_w - sud_w * 2, 0, sud_w, sud_h);
            s_dwn = new Rectangle(bg_w - sud_w, 0, sud_w, sud_h);
            scroll = new Rectangle(bg_w - 6 - scrfg_w, (int)(bg_h + 5 + mScroll), scrfg_w, scr0 + scr2);

            //e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // Background for bar
            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselbar,
                sel);

            // Up/Down Btns
            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselbar_arrow_up,
                s_up);
            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselbar_arrow_dwn,
                s_dwn);

            // Side Btns
            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselbarbtn,
                btn1);
            e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselbarbtn,
                btn2);

            //Bar Text
            e.Graphics.DrawString(SelectedItem, TextTools.font1, new SolidBrush(this.ForeColor),
                new RectangleF(10, -1, bg_w, bg_h), TextTools.format1);

            if (mDropVisible)
            {
                int dd_w = CitySynth.Properties.Resources.ctseldropdownbg.Width,
                    dd_h = CitySynth.Properties.Resources.ctseldropdownbg.Height;
                int scrbg_w = CitySynth.Properties.Resources.ctseldropdownscroll.Width,
                    scrbg_h = CitySynth.Properties.Resources.ctseldropdownscroll.Height;
                if (pHeight == 0)
                    pHeight = this.Height;
                if (this.Height < bg_h + dd_h)
                    this.Height = bg_h + dd_h;
                scr_h = dd_h - scroll.Height + 10 - bg_h;

                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctseldropdownbg,
                    new Rectangle(-1, bg_h - 1, dd_w, dd_h));
                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctseldropdownscroll,
                    new Rectangle(bg_w - 6 - scrbg_w, bg_h + 5, scrbg_w, scrbg_h));
                //e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctseldropdownscrollfg, scroll);
                int midsections = (int)((scroll.Height - scr0 - scr2) / scr1);
                Rectangle r1 = new Rectangle(scroll.X, scroll.Y, scroll.Width, scr0),
                    r3 = new Rectangle(scroll.X, scroll.Y + scr1 * midsections + scr0, scroll.Width, scr2);
                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselddscroll_top, r1);
                for (int i = 0; i < midsections; i++)
                    e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselddscroll_mid, 
                        new Rectangle(scroll.X, scroll.Y + scr0 + i * scr1, scroll.Width, scr1));
                e.Graphics.DrawImageUnscaledAndClipped(CitySynth.Properties.Resources.ctselddscroll_btm, r3);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                

                for (int i = 0; i < items; i++)
                {
                    Brush b;
                    if (i + listOffset == mIndex) b = new SolidBrush(Color.White);
                    else b = new SolidBrush(this.ForeColor);
                    e.Graphics.DrawString(mItems[i + listOffset], TextTools.font1, b,
                        dropItemRecs[i], TextTools.format1);
                    float y = list.Y + (float)(i + 1) * dropItemRecs[0].Height + bg_h + 1f;
                    Pen p = new Pen(Color.FromArgb(128, 25, 25, 25), 1);
                    if (i < items - 1)
                        e.Graphics.DrawLine(p, list.X, y, list.X + list.Width, y);
                }
            }
            else if (pHeight != this.Height && pHeight != 0)
            {
                this.Height = pHeight;
                pHeight = 0;
            }
        }

        private void PresetSelector_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mDragScroll = false;
                Rectangle m = new Rectangle(e.X - 2, e.Y - 2, 4, 4);
                float py = dropItemRecs[0].Y;

                if (m.IntersectsWith(s_up))
                    SelectedIndex = (mItems.Count + mIndex - 1) % mItems.Count;

                else if (m.IntersectsWith(s_dwn))
                    SelectedIndex = (mItems.Count + mIndex + 1) % mItems.Count;
                else if (m.IntersectsWith(sel))
                {
                    this.Focus();
                    mDropVisible = !mDropVisible;
                    listOffset = mIndex - 1;
                    listOffset = Math.Max(0, Math.Min(listOffset, mItems.Count - items));
                    mScroll = (int)(scr_h * listOffset / (mItems.Count - items));
                    Invalidate();
                }
                else if (m.IntersectsWith(btn1))
                    PresetButtonClicked.Invoke(this, new PresetButtonClickEventArgs(0));
                else if (m.IntersectsWith(btn2))
                    PresetButtonClicked.Invoke(this, new PresetButtonClickEventArgs(1));
                else if (e.Y >= py && !m.IntersectsWith(scroll))
                {
                    for (int i = 0; i < items; i++)
                    {
                        float y = dropItemRecs[i].Y;
                        float y1 = dropItemRecs[i].Height + y;
                        if (e.Y < y1 && e.Y > y)
                        {
                            SelectedIndex = listOffset + i;
                            mDropVisible = false;
                            Invalidate();
                        }
                        py = y;
                    }
                }

            }
        }

        private void PresetSelector_Leave(object sender, EventArgs e)
        {
            mDropVisible = false;
            Invalidate();
        }

        private void PresetSelector_MouseLeave(object sender, EventArgs e)
        {
            if (timeout != null)
            {
                timeout.Stop();
                timeout.Dispose();
                timeout = null;
            }
            timeout = new Timer();
            timeout.Interval = 150;
            timeout.Tick += CloseDropdown;
            timeout.Start();
        }

        void CloseDropdown(object sender, EventArgs e)
        {
            if (timeout != null)
            {
                timeout.Stop();
                timeout.Dispose();
                timeout = null;
            }
            mDropVisible = false;
            Invalidate();
        }

        private void PresetSelector_MouseEnter(object sender, EventArgs e)
        {
            if (timeout != null)
            {
                timeout.Stop();
                timeout.Dispose();
                timeout = null;
            }
        }

        private void PresetSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (mDropVisible)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    dy = lastY - e.Y;

                    if ((DateTime.Now - lastTime).TotalMilliseconds > 30)
                    {
                        Rectangle m = new Rectangle(e.X - 2, e.Y - 2, 4, 4);
                        if (mDragScroll)
                            mScroll -= dy;
                        if (mScroll > scr_h) mScroll = scr_h;
                        else if (mScroll < 0) mScroll = 0;

                        listOffset = (int)((mItems.Count - items) * (mScroll / scr_h));

                        Invalidate();

                        lastY = e.Y;
                        lastTime = DateTime.Now;
                    }
                }

            }
        }

        void PresetSelector_MouseWheel(object sender, MouseEventArgs e)
        {
            float cc = mItems.Count - items;
            int delta = e.Delta/120;
            if (mDropVisible)
            {
                listOffset -= delta;
                listOffset = Math.Max(0, Math.Min(listOffset, mItems.Count - items));
                //listOffset = (int)((cc) * (mScroll / scr_h));
                mScroll = (int)(scr_h * listOffset / (cc));
                Invalidate();
            }
            else
            {
                int ind = (mItems.Count + mIndex - delta) % mItems.Count;
                if(!mScrollLoop)
                ind = Math.Max(0, Math.Min(mItems.Count - 1, ind));
                SelectedIndex = ind;
            }
        }

        void PresetSelector_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle m = new Rectangle(e.X - 2, e.Y - 2, 4, 4);
            mDragScroll = (m.IntersectsWith(scroll));
            lastY = e.Y;
        }
    }

    
    public delegate void OnSelectedIndexChanged(object sender, EventArgs e);
    public delegate void OnPresetButtonClicked(object sender, PresetButtonClickEventArgs e);

    public class PresetButtonClickEventArgs : EventArgs
    {
        public int ButtonIndex { get; set; }
        public PresetButtonClickEventArgs(int index) { this.ButtonIndex = index; }
    }

    public static class ImageTools
    {
        public static Bitmap RotateColors(Bitmap image)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;
            float degrees = 180f;
            double r = degrees * System.Math.PI / 180; // degrees to radians 

            Bitmap b = (Bitmap)image.Clone();
            Graphics g = Graphics.FromImage(b);

            float[][] colorMatrixElements = { 
        new float[] {(float)System.Math.Cos(r),  (float)System.Math.Sin(r),  0,  0, 0},
        new float[] {(float)-System.Math.Sin(r),  (float)-System.Math.Cos(r),  0,  0, 0},
        new float[] {0,  0,  2,  0, 0},
        new float[] {0,  0,  0,  1, 0},
        new float[] {0,  0,  0,  0, 1}};

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            //imageAttributes.SetColorMatrix(
            //   colorMatrix,
            //   ColorMatrixFlag.SkipGrays,
            //   ColorAdjustType.Bitmap);

            //g.Clear(Color.Transparent);
            g.DrawImage(
               b,
               new Rectangle(0, 0, width, height),  // destination rectangle 
                0, 0,        // upper-left corner of source rectangle 
                width,       // width of source rectangle
                height,      // height of source rectangle
                GraphicsUnit.Pixel,
               imageAttributes);

            Bitmap ret = (Bitmap)b.Clone();
            b.Dispose();
            g.Dispose();
            return ret;
        }
    }

}
