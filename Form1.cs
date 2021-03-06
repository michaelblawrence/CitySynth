﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CitySynth.UI_Elements;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using Midi;

namespace CitySynth
{
    public partial class Form1 : Form
    {
        #region Local Declarations
        int calibstat = -1;
        List<int> calibdata = new List<int>();
        int activekey = -1;
        int firstkey = -1;
        int DriverIndex = 2; 
        int filenameIndex = 0;
        string rawsets = Encoding.UTF8.GetString(Properties.Resources.factorypresets);
        string rawrec = DateTime.Now.ToLongTimeString() + "\n";
        string presetFilename = null;
        string argsFilename = null;
        bool enableTouch = false, _enableTouch = false;
        System.Threading.Timer uiWait;
        DateTime lastKey = DateTime.Now;
        bool flashPending = false;
        bool recnotes = false;
        DateTime recStarted = DateTime.Now;
        bool defaultReverb = true;
        DateTime flashStarted = DateTime.Now;
        List<InputDevice> devs;
        Object varyingDial = null;
        double pdB = 0;

        int waveform_w, waveform_h;
        Bitmap b;
        Graphics g;
        PointF[] waveformPts;

        WaveOut wout = new WaveOut();
        AsioOut aout = null;
        WaveProvider16 cwp;
        WaveRecorder wr;
        Timer timer = new Timer();

        public static float[] semitones = new float[12];
        public const int FlashLength = 200;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion
        #region init

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CitySynth.Form1"/> class.
        /// </summary>
        /// <param name="argsFilename">Arguments file path.</param>
        public Form1(string argsFilename)
        {
            InitializeComponent();
            InitializeDials();
            this.argsFilename = argsFilename;
        }

        /// <summary>
        /// Loads audio and midi drivers, presets, wavefunctions and GUI update thread
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            //Audio Init and device selection
            string[] drivers = AsioOut.GetDriverNames();
            if (!Program.displayNoDialog)
            {
                for (int i = 0; i < drivers.Length; i++)
                {
                    DialogResult dr = MessageBox.Show("Use " + drivers[i] + "?", "Choose Audio Device", MessageBoxButtons.YesNo);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        DriverIndex = i;
                        break;
                    }
                }
                aout = new AsioOut(DriverIndex);
                aout.ShowControlPanel();
            }
            if (aout == null) aout = new AsioOut(DriverIndex);

            //Semitone factor calcluations
            semitones[0] = 1;
            double semrat = Math.Pow(2, 1 / 12.0d);
            for (int i = 1; i < 12; i++)
            {
                semitones[i] = (float)Math.Round(semitones[i - 1] * semrat, 5, MidpointRounding.ToEven);
            }

            //Preset loading and preparation
            string filename = Path.Combine(Application.LocalUserAppDataPath, "userpresets.sdp");
            this.presetFilename = filename;
            LoadFromFile(filename, 0, true);
            if (argsFilename != null &&
                (argsFilename.EndsWith(".sdp") || argsFilename.EndsWith(".sdp.backup")))
            {
                try
                {
                    DialogResult dr = DialogResult.No;
                    if (!Program.displayNoDialog) dr = MessageBox.Show(
                        "Do you wish commit this collection to your local user library? \n(Warning this feature is experimental!)",
                        "Store Preset", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    string s = LoadFromFile(argsFilename, 0, false);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        string backfn = filename + ".backup";
                        if (File.Exists(backfn)) File.Delete(backfn);
                        File.Copy(filename, backfn);
                        string exist = File.ReadAllText(filename).Trim();
                        if (File.Exists(filename)) File.Delete(filename);
                        File.WriteAllText(filename, exist + "\n" + s);
                    }
                }
                catch (Exception ex) { if (!Program.displayNoDialog) MessageBox.Show("Loading Error: " + ex.Message); }
                
            }

            //WaveProvider Loading and Playing
            if (R.MonoEnabled)
                cwp = new CustomWaveProvider(R.SampleRate);
            else
                cwp = new PolyWaveProvider(R.SampleRate);
            if (aout != null) { aout.Init(cwp); aout.Play(); }
            else { wout.Init(cwp); wout.Play(); }

            //MIDI init and device selection
            devs = new List<InputDevice>();
            int mididevs = InputDevice.InstalledDevices.Count;
            
            if (mididevs > 0)
            {
                foreach (InputDevice id in InputDevice.InstalledDevices)
                {
                    string name = id.Name;
                    DialogResult dr;
                    if (Program.displayNoDialog) dr = System.Windows.Forms.DialogResult.Yes;
                    else dr = MessageBox.Show("Would you like to use " + name + " as a midi device?",
                        "Enable Midi", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        MidiOpen:
                        try {
                            id.Open();
                            id.NoteOn += InputDevice_NoteOn;
                            id.NoteOff += id_NoteOff;
                            id.ControlChange += InputDevice_ControlChange;
                            id.PitchBend += InputDevice_PitchBend;
                            id.StartReceiving(null);
                            devs.Add(id);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Error: " + ex.Message);
                            DialogResult dres =
                                MessageBox.Show("Midi device " + id.Name + " is busy", "Midi device unavailable", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);
                            if (dres == DialogResult.Retry) goto MidiOpen;
                            else continue;
                        }
                    }
                }
            }

            //Wave Preview Init
            waveform_w = osc1WavePreviewBox.Width;
            waveform_h = osc1WavePreviewBox.Height;
            b = new Bitmap(waveform_w, waveform_h, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(b);
            g.FillRectangle(Brushes.White, 0, 0, waveform_w, waveform_h);

            //UI Update thread Init and Start
            timer.Interval = 1000 / R.RefreshRate;
            timer.Tick += OnScreenRefresh;
            timer.Start();
        }

        /// <summary>
        /// Initializes the dials GUI controls.
        /// </summary>
        private void InitializeDials()
        {
            filterCutoffDial.SetLogScale(true);
            filterCutoffDial.SetValueBounds(5, 21000);
            
            delayLengthDial.SetValueBounds(0, 1000 * R.DelayBufferLength / R.SampleRate);
            delayWetDial.SetValueBounds(0, 1.1f);
            pitchRateDial.SetValueBounds(0, 40);
            //pitchWidthDial.SetLogScale(true);
            pitchWidthDial.SetValueBounds(0, 0.2f);

            osc1GainDial.SetValueBounds(-48, 6);
            osc1PhaseDial.SetValueBounds(0, 1);

            ampAttackDial.SetValueBounds(1, 1500);
            ampDecayDial.SetValueBounds(5, 1500);
            ampSustainDial.SetValueBounds(0, 1);
            ampReleaseDial.SetValueBounds(2, 1500);

            envReleaseDial.SetValueBounds(5, 1500);
            envAttackDial.SetValueBounds(5, 1500);
            envFloorDial.SetLogScale(true);
            envFloorDial.SetValueBounds(5, 21000);
            envCeilingDial.SetLogScale(true);
            envCeilingDial.SetValueBounds(5, 21000);

            lfoRateDial.SetValueBounds(0, 20);
            lfoWidthDial.SetValueBounds(10, 15000);
            ampLfoRateDial.SetValueBounds(0, 440);
            ampLfoWidthDial.SetValueBounds(0, 1);
        }

        #endregion

        /// <summary>
        /// Handles form key down events for musical keyboard updates and key presses to update UI and parameters
        /// </summary>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int kv = e.KeyValue;
            UpdateCaps();

            float keyPressedFreq = R.KeyToFreq(e.KeyValue);

            if (keyPressedFreq == -1)
            {
                switch (e.KeyCode)
                {
                    // Preset navigation
                    case Keys.PageUp:
                        presetSelector.SelectedIndex = ((presetSelector.Items.Count + presetSelector.SelectedIndex - 1) % presetSelector.Items.Count);
                        break;
                    case Keys.PageDown:
                        presetSelector.SelectedIndex = ((presetSelector.SelectedIndex + 1) % presetSelector.Items.Count);
                        break;

                    // Master volume control
                    case Keys.Home:
                        masterLevelDial.Value += 0.02f;
                        break;
                    case Keys.End:
                        masterLevelDial.Value -= 0.02f;
                        break;


                    // Numberpad Controls
                    //  Current dial value nudging
                    case Keys.NumPad8:
                        if (varyingDial != null)
                            ((Dial)varyingDial).Value += 0.02f;
                        break;
                    case Keys.NumPad2:
                        if (varyingDial != null)
                            ((Dial)varyingDial).Value -= 0.02f;
                        break;
                    //  Voice harmonic amount varying
                    case Keys.NumPad4:
                        R.MaxHarmonic = Math.Max(R.MaxHarmonic - 1, 1);
                        break;
                    case Keys.NumPad6:
                        R.MaxHarmonic = Math.Min(R.MaxHarmonic + 1, 10);
                        break;


                    // Function keys
                    //  Record audio to file
                    case Keys.F3:
                        recButton.PerformClick();
                        break;
                    //  Reverb active toggle
                    case Keys.F4:
                        if (!R.keys[(int)Keys.ControlKey])
                        {
                            // Toggles state of reverb return
                            defaultReverb = !defaultReverb;
                            if (defaultReverb && R.ReverbWet == 0)
                            {
                                if (!enableTouch) R.ReverbWet = 0.32f;
                                else delayWetDial.Value = 0.32f;
                            }

                            else if (!defaultReverb)
                            {
                                if (!enableTouch) R.ReverbWet = 0f;
                                else delayWetDial.Value = 0f;
                            }
                        }
                        else
                        {
                            // Enables reverb
                            if (PolyWaveProvider.reverb[0].GetType() == typeof(TapeReverb))
                                for (int i = 0; i < PolyWaveProvider.reverb.Length; i++)
                                {
                                    int[] cc = new int[PolyWaveProvider.reverb[i].GetBufferLengths().Length];
                                    Array.Copy(PolyWaveProvider.reverb[i].GetBufferLengths(), cc, cc.Length);
                                    PolyWaveProvider.reverb[i].Dispose();
                                    PolyWaveProvider.reverb[i] = new DigitalReverb(R.SampleRate, cc);
                                }
                            else
                                for (int i = 0; i < PolyWaveProvider.reverb.Length; i++)
                                    PolyWaveProvider.reverb[i] = new TapeReverb(R.SampleRate, PolyWaveProvider.reverb[i].GetBufferLengths());
                        }
                        break;
                    //  Harmonic generation method toggle / (Alt) Enable spacebar sustain pedal toggle
                    case Keys.F6:
                        if (!enableTouch)
                        {
                            R.HarmonicV1 = !R.HarmonicV1;
                            if (R.HarmonicV1) R.MaxHarmonic *= 2;
                            else R.MaxHarmonic /= 2;
                        }
                        else
                        {
                            R.SusToSpacebar = !R.SusToSpacebar;
                        }
                        break;
                    //  Stereo reverb toggle / (Alt) Enable fixed velocity toggle
                    case Keys.F7:
                        if (!enableTouch)
                        {
                            if (!R.keys[kv])
                            {
                                R.StereoReverb = !R.StereoReverb;
                            }
                            if (R.tmp) FlashMidiIndicator();
                        }
                        else
                        {
                            R.MaxVelocity = R.MaxVelocity == 1 ? 127 : 1;
                        }
                        R.keys[kv] = true;
                        break;
                    //  Save current state as preset
                    case Keys.F12:
                        SavePreset();
                        break;


                    // Wave Function Selectors
                    case Keys.D1:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.sin;
                            float i = (int)WaveFuntion.sin;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.sin;
                        break;
                    case Keys.D2:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.tri;
                            float i = (int)WaveFuntion.tri;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.tri;
                        break;
                    case Keys.D3:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.saw;
                            float i = (int)WaveFuntion.saw;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.saw;
                        break;
                    case Keys.D4:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.bit;
                            float i = (int)WaveFuntion.bit;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.bit;
                        break;
                    case Keys.D5:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.comp;
                            float i = (int)WaveFuntion.comp;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.comp;
                        break;
                    case Keys.D6:
                        if (!enableTouch)
                        {
                            R.WFunction = WaveFuntion.pulse;
                            float i = (int)WaveFuntion.pulse;
                            osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        }
                        else R.HarmonicFunction = WaveFuntion.pulse;
                        break;


                    // Octave Control
                    case Keys.Z:
                        R.BaseFrequency *= 0.5f;
                        break;
                    case Keys.X:
                        R.BaseFrequency *= 2f;
                        break;


                    // Momentary Octave Control
                    case Keys.ControlKey:
                        if (!R.keys[kv])
                            R.BaseFrequency *= 0.5f;
                        R.keys[kv] = true;
                        R.SystemPaused = false;
                        break;
                    case Keys.ShiftKey:
                        if (!R.keys[kv])
                            R.BaseFrequency *= 2.0f;
                        R.keys[kv] = true;
                        break;


                    // Momentary Interval Control
                    case Keys.C:
                        float _factor = semitones[5];
                        if (!R.keys[kv])
                            R.BaseFrequency /= _factor;
                        R.keys[kv] = true;
                        break;
                    case Keys.V:
                        float _factor1 = semitones[7];
                        if (!R.keys[kv])
                            R.BaseFrequency /= _factor1;
                        R.keys[kv] = true;
                        break;


                    // Tuning control
                    case Keys.Oemplus:
                        if (!enableTouch)
                            R.kb_off++;
                        else
                            R.kb_trem++;
                        break;
                    case Keys.OemMinus:
                        if (!enableTouch)
                        {
                            if (R.kb_off > 0)
                                R.kb_off--;
                        }
                        else
                            R.kb_trem--;
                        break;


                    // Momentary tuning control (trill keys)
                    case Keys.D0:
                        if (!R.keys[kv])
                            R.kb_off++;
                        R.keys[kv] = true;
                        break;
                    case Keys.D9:
                        if (!R.keys[kv])
                            R.kb_off += R.kb_trem;
                        R.keys[kv] = true;
                        break;


                    // Key Modifiers
                    case (Keys.RButton | Keys.ShiftKey):  // Alt key down
                        enableTouch = true;
                        break;
                    case Keys.Capital:
                        R.keys[kv] = true;
                        break;
                    //  Spacebar state send to voices
                    case Keys.Space:
                        R.Amplitude1 = 1;
                        break;
                }
            }

            // Handles PC keyboard to synth keys events
            if (keyPressedFreq != -1)
            {
                e.SuppressKeyPress = true;
                activekey = e.KeyValue;

                // sets global state for monophonic mode
                R.Frequency = keyPressedFreq;
                R.Amplitude = 1;

                // Activates a paused state synth and resets idle time clock
                lastKey = DateTime.Now; R.SystemPaused = false; R.SystemState = 1;

                if (R.ke.Count < R.Voices
                    && !R.keys[kv])
                {
                    // adds key press to list of key down for voices
                    R.ke.Add(new int[] { kv, 127 });
                    FlashMidiIndicator();
                }

                // updates key state buffer
                R.keys[kv] = true;
                if (firstkey == -1) firstkey = kv;
            }
        }

        /// <summary>
        /// Handles form key up events for musical keyboard updates and key presses to update UI and parameters
        /// </summary>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // sets global state for monophonic mode
            if (e.KeyValue == activekey && R.keys[e.KeyValue])
                R.Amplitude = 0;
            if (e.KeyValue == firstkey) firstkey = -1;
            else if (firstkey != -1)
            {
                // retriggers previous note in monophonic mode
                KeyEventArgs ee = new KeyEventArgs((Keys)firstkey);
                Form1_KeyDown(sender, ee);
            }

            // updates key state buffer
            R.keys[e.KeyValue] = false;


            // adds key press to list of key up for voices for musical PC keyboard events
            if (R.ke.ConvertAll<int>(a => a[0]).Contains(e.KeyValue))
            {
                R._ke.Add(e.KeyValue);
                FlashMidiIndicator();
            }

            // Close Form
            if (e.KeyCode == Keys.Escape)
            {
                R.Amplitude = 0;
                this.Close();
            }

            // Spacebar state send to voices
            if (e.KeyCode == Keys.Space)
            {
                R.Amplitude1 = 0;
            }
            // Reset Momentary Interval Control
            if (e.KeyCode == Keys.C)
            {
                float _factor = semitones[5];
                R.BaseFrequency *= _factor;
            }
            if (e.KeyCode == Keys.V)
            {
                float _factor1 = semitones[7];
                R.BaseFrequency *= _factor1;
            }


            // Reset Momentary Octave Control
            if (e.KeyCode == Keys.ControlKey)
            {
                R.BaseFrequency *= 2.0f;
            }
            if (e.KeyCode == Keys.ShiftKey)
            {
                R.BaseFrequency *= 0.5f;
            }

            // Allows for tracking of Mod key (Alt) and CapsLoak
            if (e.KeyCode == (Keys.RButton | Keys.ShiftKey)) // Alt key up
            {
                if (!e.Control)
                    enableTouch = false;
            }
            if (e.KeyCode == Keys.CapsLock)
            {
                R.keys[e.KeyValue] = true;
            }

            // Resets Momentary Tuning Control
            if (e.KeyCode == Keys.D0)
            {
                R.kb_off--;
                R.keys[e.KeyValue] = false;
            }
            if (e.KeyCode == Keys.D9)
            {
                R.kb_off -= R.kb_trem;
                R.keys[e.KeyValue] = false;
            }
        }

        /// <summary>
        /// Handles midi key down and up events for synth voice updates
        /// </summary>
        /// <param name="msg">MIDI device note on data.</param>
        void InputDevice_NoteOn(NoteOnMessage msg)
        {
            FlashMidiIndicator();
            if (msg.Velocity == 0)
            {
                NoteOffMessage off = new NoteOffMessage(msg.Device, msg.Channel, msg.Pitch, msg.Velocity, msg.Time);
                id_NoteOff(off);
            }
            else
            {
                //Note On
                int oct = msg.Pitch.Octave(), pos = msg.Pitch.PositionInOctave();
                int st = (oct - 2) * 12 + pos;

                if (calibstat != 0)
                {
                    if (calibstat == -1)
                        activekey = st;
                    else
                    {
                        activekey = st = calibdata.IndexOf(st);
                    }
                    float factor = 0;


                    lastKey = DateTime.Now; R.SystemPaused = false; R.SystemState = 1;
                    int kv = st + 300;
                    if (!R.keys[kv])
                    {
                        int index = -1;
                        if ((index = R.ke.ConvertAll<int>(i => i[0]).IndexOf(kv)) >= 0)
                            R.ke.RemoveAt(index);
                        R.ke.Add(new int[] { kv, msg.Velocity });
                    }
                    R.keys[kv] = true;

                    if (recnotes) rawrec += String.Format("{0}#{1}#down\n", (DateTime.Now - recStarted).Ticks, st);

                    factor = semitones[(R.kb_off + st + 12 * 12) % 12] * (1 + (R.kb_off + st) / 12);
                    R.Frequency = R.BaseFrequency * factor;
                    if (st != -1)
                        R.Amplitude = 1;
                }
                else
                {
                    calibdata.Add(st);
                }
            }
        }

        /// <summary>
        /// Handles midi key up events for synth voice updates
        /// </summary>
        /// <param name="msg">MIDI device note off data.</param>
        void id_NoteOff(NoteOffMessage msg)
        {
            FlashMidiIndicator();

            // Note Off
            int oct = msg.Pitch.Octave(), pos = msg.Pitch.PositionInOctave();
            int st = (oct - 2) * 12 + pos;
            if (calibstat != -1) st = calibdata.IndexOf(st);
            if (st == activekey)
                R.Amplitude = 0;

            if (recnotes) rawrec += String.Format("{0}#{1}#up\n", (DateTime.Now - recStarted).Ticks, st);

            int kv = st + 300;
            R.keys[kv] = false;
            int index;
            if ((index = R.ke.ConvertAll<int>(a => a[0]).IndexOf(kv)) >= 0)
            {
                R._ke.Add(kv);
                R.ke[index][1] = 0;
            }
        }

        /// <summary>
        /// Handles midi CC events for synth and UI parameter updates
        /// </summary>
        /// <param name="msg">MIDI device changed CC data.</param>
        void InputDevice_ControlChange(ControlChangeMessage msg)
        {
            switch (msg.Control)
            {
                case Midi.Control.SustainPedal:
                    bool susPedalDown = msg.Value == 127;
                    if (R.SusToSpacebar) R.Amplitude1 = susPedalDown ? 1 : 0;
                    else
                        R.SusPedal = susPedalDown;
                    break;
                case Midi.Control.ModulationWheel:
                    float i = msg.Value / 127.0f;
                    if (varyingDial != null)
                        ((Dial)varyingDial).Value = i;
                    break;
                case (Midi.Control)114:
                    float val = msg.Value / 127f;
                    osc1GainDial.Value = val;
                    break;
                case (Midi.Control)43:
                    filterCutoffDial.Value = msg.Value / 127f;
                    break;
                case (Midi.Control)30:
                    delayLengthDial.Value = msg.Value / 127f;
                    break;
                case (Midi.Control)31:
                    delayWetDial.Value = msg.Value / 127f;
                    break;
            }
            
        }

        /// <summary>
        /// Handles midi pitch bend events for synth and UI parameter updates
        /// </summary>
        /// <param name="msg">MIDI device pitch bend data.</param>
        private void InputDevice_PitchBend(PitchBendMessage msg)
        {
            R.PitchBendFactor = (float)Math.Pow(2, (4 * msg.Value / 16383.0f - 2) / 12);
        }

        /// <summary>
        /// Called periodically when GUI must redraw
        /// </summary>
        void OnScreenRefresh(object sender, EventArgs e)
        {
            // Update metering levels with RMS audio level from synth engine samples
            double ave = (PolyWaveProvider.meteringSamples.Max() - PolyWaveProvider.meteringSamples.Min()) / (2 * ((double)short.MaxValue));
            double dB = 10 * 0.5 * Math.Log10(Math.Max(ave, Math.Pow(2, -49)));
            dB = Math.Max(dB, -48);
            float meterFloor = 20f;
            float rmsLevel = (float)((dB + meterFloor) / (meterFloor+3));
            float a = 0.8f;
            a *= (rmsLevel - (float)((dB + meterFloor) / (meterFloor + 3)) + 0.4f);
            pdB = dB;
            if (!Program.displayNoUI) 
                masterMeter.BothProgress = masterMeter.LeftProgress + a * (rmsLevel - masterMeter.LeftProgress); 

            // Clear UI for subsequent drawing
            g.Clear(Color.Transparent);

            // Prepare graphics to draw audio wavefrom 
            int img_w = waveform_w - 3,
                img_h = waveform_h - 12;
            float px = 0, py = 0;
            int len = PolyWaveProvider.wavDisplaySamples.Length;
            if (waveformPts == null)
                waveformPts = new PointF[len];
            int scaleFactor = 8;

            // Map waveform data to bitmap coordinates
            for (int i = 0; i < len; i++)
            {
                short val = PolyWaveProvider.wavDisplaySamples[i];
                float y = img_h * scaleFactor / 2 * val / (float)short.MaxValue + waveform_h / 2;
                float x = 1 + i * (img_w / (float)len);
                waveformPts[i] = new PointF(x, y);
                px = x; py = y;

            } 

            // Draw audio waveform
            g.DrawCurve(Pens.Orange, waveformPts);
            if (!R.SystemPaused && (!Program.displayNoUI)) 
                osc1WavePreviewBox.Image = b;

            // Flash MIDI indicator on request
            if (flashPending)
            {
                DateTime now = DateTime.Now;
                if ((now - flashStarted).TotalMilliseconds < FlashLength)
                {
                    if (!midiIndicator.Value)
                        midiIndicator.Value = true;
                }
                else
                {
                    midiIndicator.Value = false;
                    flashPending = false;
                }
            }
            else if (midiIndicator.Value)
                midiIndicator.Value = false;

            // Handle synth engine paused state to save energy
            if (!R.SystemPaused && (DateTime.Now - lastKey).TotalSeconds > R.SystemSleepTime)
            { 
                if (R.ke.Count == 0 && R.ActiveVoices == 0 && R.SystemState != 2 && !R.keys[(int)Keys.ControlKey])
                {
                    uiWait = new System.Threading.Timer(new System.Threading.TimerCallback((s) =>
                    {
                        if (R.SystemState == 2)
                        {
                            R.SystemPaused = true;
                            R.SystemState = 1;
                        }
                    }), R.SystemPaused, 2000, -1);
                     
                    R.SystemState = 2;    
                }
            }

            // remap select dials if TOUCH key (Alt) is down
            if (_enableTouch != enableTouch)
            {
                delayHeaderLabel.Text = enableTouch ? "REVERB" : "DELAY";
                delayWetDial.SetScaledValue(enableTouch ? R.ReverbWet : R.DelayWet, true, true);
                delayLengthDial.SetScaledValue(enableTouch ? R.ReverbDelay : R.DelayTime, true, true);

                osc1GainDial.Text = enableTouch ? "Drive" : "Gain";
                osc1GainDial.SetScaledValue(enableTouch ? ((4 * (R.FilterDrive - 1)) - 48) : R.Gain, true, true);

                osc1PhaseDial.Text = enableTouch ? "Pump" : "Phase";
                osc1PhaseDial.SetScaledValue(enableTouch ? R.FilterFeedback : R.HarmonicPhase, true, true);
                
                filterHeaderLabel.Text = enableTouch ? "HPF" : "FILTER";
                filterCutoffDial.SetScaledValue(enableTouch ? R.HPFCutoff : R.LPF, true, true);
                
            }
            _enableTouch = enableTouch;

            // Allow LFO varying of dial value
            if (R.SendLFO && R.LFOreturn != -2)
            {
                if (varyingDial != null) ((Dial)varyingDial).Value = 0.5f + 0.5f * R.LFOreturn / lfoWidthDial.Maximum;
                else R.HarmonicPhase = 0.5f + 0.5f * R.LFOreturn / lfoWidthDial.Maximum;
            }
            R.LFOreturn = -2;
        }

        /// <summary>
        /// Handles the changing of active preset when the preset selector's selected index changed.
        /// </summary>
        private void presetSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clean up input lines
            int index = presetSelector.SelectedIndex;
            string line = rawsets.Replace("\r", "").Trim().Split('\n')
                .Where(s => { return !(s.StartsWith("//") || s.Trim() == ""); }).ToArray()
            [index].Trim().TrimEnd(';').Trim();
            string[] whole = line.Trim().Split('|');

            // Resets controls and associated global values to default value for continuity
            touchPad_DoubleClick(sender, e);
            foreach (CitySynth.UI_Elements.Dial di
                in panel1.Controls.OfType<CitySynth.UI_Elements.Dial>())
                if (di.Name != masterLevelDial.Name)
                    di.ResetToDefault();
            foreach (CitySynth.UI_Elements.ToggleIcon ti
                in panel1.Controls.OfType<CitySynth.UI_Elements.ToggleIcon>())
                ti.ResetToDefault();
            R.ReverbWet = defaultReverb ? 0.32f : 0;
            R.HarmonicFunction = WaveFuntion.sin;

            // Iterates through saved non-default parameters editing UI controls and underlying associated global values
            foreach (string data in whole[1].Split(';'))
            {
                string[] half = data.Split(':');
                float i = -1;
                if (half.Length != 2) continue;
                if (!float.TryParse(half[1], out i)) continue; // parse value into var i
                switch (half[0]) // switched key
                {
                    case "a": // Attack
                        ampAttackDial.SetScaledValue(i);
                        break;
                    case "d": // Decay
                        ampDecayDial.SetScaledValue(i);
                        break;
                    case "s": // Sustain
                        ampSustainDial.SetScaledValue(i);
                        break;
                    case "r": // Release
                        ampReleaseDial.SetScaledValue(i);
                        break;
                    case "b": // Octave Shift
                        R.BaseFrequency = 440 * (float)Math.Pow(2, i);
                        break;
                    case "h": // Harmonic count for DCO
                        R.MaxHarmonic = (int)i;
                        break;
                    case "hw": // Wavefunction for added harmonics
                        R.HarmonicFunction = (WaveFuntion)i;
                        break;
                    case "hp": // Phase shift for harmonics
                        osc1PhaseDial.SetScaledValue(i);
                        break;
                    case "h2": // Harmonics gain
                        R.Harmonic2Gain = i;
                        break;
                    case "sub": // Sub oscillator gain
                        R.SubOscGain = i;
                        break;
                    case "hv1": // Harmonic generation algoritm selector
                        R.HarmonicV1 = Convert.ToBoolean((int)i);
                        break;
                    case "g": // Synth voice gain
                        R.Gain = i;
                        R.GeneralAtten = (float)Math.Pow(10, R.Gain / 10.0f);
                        break;
                    case "w": // Wavefunction for main DCO
                        osc1WaveSelDial.Value = i / osc1WaveSelDial.SnapSegments;
                        break;
                    case "hpf": // High pass filter (HPF) cutoff frequency
                        if (!enableTouch) R.HPFCutoff = i;
                        else filterCutoffDial.SetScaledValue(i);
                        break;
                    case "lpf": // Low pass filter (LPF) cutoff frequency
                        if (!enableTouch) filterCutoffDial.SetScaledValue(i);
                        else R.LPF = i;
                        break;
                    case "lfo": // LPF low frequency oscillator (LFO) rate
                        lfoRateDial.SetScaledValue(i);
                        break;
                    case "lwidth": // LPF low frequency oscillator (LFO) width
                        lfoWidthDial.SetScaledValue(i);
                        break;
                    case "prate": // Pitch LFO rate
                        pitchRateDial.SetScaledValue(i);
                        break;
                    case "pwidth": // Pitch LFO width
                        pitchWidthDial.SetScaledValue(i);
                        break;
                    case "arate": // Amplitude LFO rate
                        ampLfoRateDial.SetScaledValue(i);
                        break;
                    case "awidth": // Amplitude LFO width
                        ampLfoWidthDial.SetScaledValue(i);
                        break;
                    case "delay": // Delay FX time
                        delayLengthDial.SetScaledValue(i);
                        break;
                    case "dwet": // Delay FX wet level
                        delayWetDial.SetScaledValue(i);
                        break;
                    case "rwet": // Reverb FX wet level
                        R.ReverbWet = i;
                        break;
                    case "hc": // Added harmonic level balance control
                        R.HarmonicsControl = i;
                        break;
                    case "ffb": // Filter feedback level
                        R.FilterFeedback = i;
                        break;
                    case "fd": // Filter drive level
                        R.FilterDrive = i;
                        break;
                    case "filter": // LPF FFT algotirm enabled
                        fftFilterToggle.Checked = i != 0;
                        if (fftFilterToggle.Checked) R.FFTMode = (int)i;
                        break;
                    case "lpfenv": // LPF cuttoff EG enabled
                        envActivateButton.Checked = i == 1;
                        break;
                    case "la": // LPF cuttoff EG attack
                        envAttackDial.SetScaledValue(i);
                        break;
                    case "lr": // LPF cuttoff EG release
                        envReleaseDial.SetScaledValue(i);
                        break;
                    case "lf": // LPF cuttoff EG floor frequency
                        envFloorDial.SetScaledValue(i);
                        break;
                    case "lc": // LPF cuttoff EG ceiling frequency
                        envCeilingDial.SetScaledValue(i);
                        break;
                }
            }
        }
        
        #region Utils

        /// <summary>
        /// Links a Dial control's value to an output variable value.
        /// </summary>
        /// <param name="variable">Scaled output variable.</param>
        /// <param name="sender">Dial control object.</param>
        public static void LinkVar(out float variable, object sender)
        {
            variable = ((CitySynth.UI_Elements.Dial)sender).GetScaledValue();
        }
        /// <summary>
        /// Type of Toggle control class
        /// </summary>
        private static Type Toggle = new CitySynth.UI_Elements.ToggleIcon().GetType();

        /// <summary>
        /// Links a Toggle Icon control's value to an output variable value.
        /// </summary>
        /// <param name="variable">Boolean output variable.</param>
        /// <param name="sender">Toggle Icon control object.</param>
        public static void LinkVar(out bool variable, object sender)
        {
            if (sender.GetType() == Toggle)
                variable = ((CitySynth.UI_Elements.ToggleIcon)sender).Checked;
            else
                variable = false;
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        void UpdateCaps()
        {
            if (IsKeyLocked(Keys.CapsLock))
            {
                const int KEYEVENTF_EXTENDEDKEY = 0x1;
                const int KEYEVENTF_KEYUP = 0x2;
                keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
        }

        /// <summary>
        /// Flashes the midi indicator at next screen update call.
        /// </summary>
        private void FlashMidiIndicator()
        {
            flashStarted = DateTime.Now;
            flashPending = true;
        }

        /// <summary>
        /// Toggles between the polyphonic and monophonic modes.
        /// </summary>
        private void SwitchPolyMonoModes()
        {
            SwitchPolyMonoModes(!R.MonoEnabled);
        }

        /// <summary>
        /// Toggles between the polyphonic and monophonic modes.
        /// </summary>
        /// <param name="isMono">If set to <c>true</c> is monophonic.</param>
        private void SwitchPolyMonoModes(bool isMono)
        {
            R.MonoEnabled = isMono;
        }

        /// <summary>
        /// Restarts and replaces the wave provider. This resets the audio driver and synth engine.
        /// </summary>
        /// <param name="wp">Wp.</param>
        private void ReplaceWaveProvider(IWaveProvider wp)
        {
            if (aout != null)
            {
                aout.Stop();
                aout.Dispose();
                aout = new AsioOut(DriverIndex);
                aout.Init(wp);
                aout.Play();
            }
            else
            {
                wout.Stop();
                wout.Dispose();
                wout = new WaveOut();
                wout.Init(wp);
                wout.Play();
            }
        }

        /// <summary>
        /// Cleans up as the form is closing.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            R.Amplitude = 0.0f;
            if (aout != null && aout.PlaybackState == PlaybackState.Playing)
                aout.Stop();
            if (wout.PlaybackState == PlaybackState.Playing)
                wout.Stop();
            if (aout != null) aout.Dispose();
            wout.Dispose();
        }


        /// <summary>
        /// Loads preset data from file.
        /// </summary>
        /// <returns>The from file.</returns>
        /// <param name="filename">File path of CitySynth preset format file.</param>
        private string LoadFromFile(string filename)
        {
            return LoadFromFile(filename, -1, true);
        }

        /// <summary>
        /// Loads preset data from file.
        /// </summary>
        /// <returns>CitySynth preset format string.</returns>
        /// <param name="filename">File path of CitySynth preset format file.</param>
        /// <param name="loadIndex">Index of preset to load.</param>
        /// <param name="writeToRaw">If set to <c>true</c> writes to local store.</param>
        private string LoadFromFile(string filename, int loadIndex, bool writeToRaw)
        {
            string s = null;
            if (File.Exists(filename))
            {
                s = File.ReadAllText(filename, Encoding.UTF8);
                rawsets = rawsets.Trim().TrimEnd(';') + ";\n";
                if (writeToRaw)
                    rawsets += s;
            }
            if (writeToRaw)
                s = LoadPresets(rawsets, loadIndex);
            else
                s = LoadPresets(s, loadIndex);
            return s;
        }

        /// <summary>
        /// Loads the presets from CitySynth preset format string adding each to current session
        /// </summary>
        /// <returns>CitySynth preset format string.</returns>
        /// <param name="input">Input CitySynth preset format string.</param>
        /// <param name="selectedIndex">Index of preset to load.</param>
        private string LoadPresets(string input, int selectedIndex)
        {
            string raw = "";

            string[] lines = input.Replace('\r', '\n').Trim().Split('\n')
                .Where<string>(s => { return !(s.StartsWith("//") || s.Trim() == ""); }).ToArray<string>();
            if (rawsets == input)
                presetSelector.Items.Clear();
            List<string> ids = new List<string>();
            Random r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0) continue;
                string main = line.Split('|')[1];
                string[] s = line.Split('|')[0].Split(':');
                string p = s[0];
                string name = s[1];
                string iden = s[2];

                string id = s[2] + "|" + s[1];
                if (!ids.Contains(id))
                {
                    ids.Add(id);
                    string presetName = s[1];
                    int index = presetSelector.Items.Count;
                    presetSelector.Items.Add(String.Format("({0:000}) {1}", index, presetName));
                    raw += String.Format("{0}:{1}:{3}|{2}\n", index, name, main, iden);
                }
            }
            if (rawsets == input)
                rawsets = raw;
            if (selectedIndex != -1)
                presetSelector.SelectedIndex = selectedIndex;
            presetSelector_SelectedIndexChanged(presetSelector, new EventArgs());
            return raw;
        }

        /// <summary>
        /// Saves the current preset to CitySynth preset format file.
        /// </summary>
        private void SavePreset()
        {
            string dialogPresetName;
            DialogResult dr= ShowSavePresetDialog(out dialogPresetName);

            if (dr == System.Windows.Forms.DialogResult.OK && dialogPresetName != "")
            {
                string s = CurrentPresetToString(dialogPresetName, presetSelector.Items.Count);
                StreamWriter sw = null;
                if (presetFilename == null)
                {
                    presetFilename = Application.LocalUserAppDataPath.TrimEnd('\\') + "\\" + "userpresets.sdp";
                    sw = File.CreateText(presetFilename);
                }
                else sw = File.AppendText(presetFilename);
                sw.WriteLine(s.TrimEnd(';'));
                sw.Flush();
                sw.Close();

                rawsets += "\n" + s;

                int i = presetSelector.Items.Count;

                LoadPresets(rawsets, i);
            }

        }

        /// <summary>
        /// Shows the saving preset name dialog.
        /// </summary>
        /// <returns>Result of the preset name dialog.</returns>
        /// <param name="dialogPresetName">Entered preset name.</param>
        private DialogResult ShowSavePresetDialog(out string dialogPresetName)
        {
            bool placeBelow = true;
            Form inputForm = new Form
            {
                Text = "Preset Name",
                Size = new System.Drawing.Size(217, 60),
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                BackColor = Color.DarkGray
            };
            inputForm.Location = new Point(this.Location.X + presetSelector.Location.X + presetSelector.Width / 2 - inputForm.Width / 2,
                this.Location.Y + presetSelector.Location.Y
                + (placeBelow ? presetSelector.Height :
                -inputForm.Height));

            TextBox tb = new TextBox
            {
                Size = new Size(150, 20),
                BorderStyle = BorderStyle.FixedSingle,
                TabIndex = 0,
                Text = "Untitled " + DateTime.Now.ToShortDateString()
            };
            tb.SelectAll();
            inputForm.Controls.Add(tb);

            Button b = new Button
            {
                FlatStyle = FlatStyle.Popup,
                DialogResult = System.Windows.Forms.DialogResult.OK,
                Size = new Size(50, 20),
                Text = "Save",
                TabIndex = 1,
                Location = new Point(150, 0)
            };
            b.Click += (sender, e) =>
            {
                if (tb.Text.Trim() != "")
                    inputForm.Close();
            };

            inputForm.Controls.Add(b);
            inputForm.KeyPreview = true;
            inputForm.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    inputForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    inputForm.Close();
                }
            };
            inputForm.AcceptButton = b;

            DialogResult dr = inputForm.ShowDialog(this);

            dialogPresetName = tb.Text;

            inputForm.Dispose();
            b.Dispose();
            tb.Dispose();

            return dr;
        }

        /// <summary>
        /// Converts the current preset to CitySynth preset format string.
        /// </summary>
        /// <returns>CitySynth preset format string.</returns>
        /// <param name="presetName">Preset name.</param>
        /// <param name="index">Preset index.</param>
        private string CurrentPresetToString(string presetName, int index)
        {
            string name = presetName.Replace(':', ' ').Replace('|', ' ');
            int iden = new Random(DateTime.Now.Millisecond).Next(1023);

            string s = index + ":" + name + ":" + iden + "|";
            s += "a:" + R.Attack + ";";
            s += "d:" + R.Decay + ";";
            s += "s:" + R.Sustain + ";";
            s += "r:" + R.Release + ";";
            s += "h:" + R.MaxHarmonic + ";";
            s += "hc:" + R.HarmonicsControl + ";";
            s += "hp:" + R.HarmonicPhase + ";";
            s += "w:" + (int)R.WFunction + ";";
            s += "hw:" + (int)R.HarmonicFunction + ";";
            s += "b:" + Math.Log(R.BaseFrequency / 440, 2) + ";";
            s += "hpf:" + R.HPFCutoff + ";";
            s += "lpf:" + R.LPF + ";";
            s += "lfo:" + R.LPFmodrate + ";";
            s += "prate:" + R.Pitchmod + ";";
            s += "pwidth:" + R.Pitchmodwidth + ";";
            s += "arate:" + R.AmpLFOrate + ";";
            s += "awidth:" + R.AmpLFOwidth + ";";
            s += "lwidth:" + R.LPFwidth + ";";
            s += "la:" + R.LPFattack + ";";
            s += "lr:" + R.LPFrelease + ";";
            s += "lf:" + R.LPFfloor + ";";
            s += "lc:" + R.LPFceiling + ";";
            s += "delay:" + R.DelayTime + ";";
            s += "dwet:" + R.DelayWet + ";";
            s += "rwet:" + R.ReverbWet + ";";
            s += "ffb:" + R.FilterFeedback + ";";
            s += "fd:" + R.FilterDrive + ";";
            s += "filter:" + (R.FFTEnable ? R.FFTMode : 0) + ";";
            s += "lpfenv:" + (R.LPFenvelope ? 1 : 0) + ";";
            if (R.Harmonic2Gain != 0)
                s += "h2:" + R.Harmonic2Gain + ";";
            if (R.SubOscGain != 0)
                s += "sub:" + R.SubOscGain + ";";
            if (!osc1GainDial.Inactive)
                s += "g:" + R.Gain + ";";
            if (!R.HarmonicV1)
                s += "hv1:" + Convert.ToInt16(R.HarmonicV1) + ";";

            return s;
        }

        /// <summary>
        /// Exports the preset pack in packaged form for sharing. Experimental.
        /// </summary>
        private void ExportPresetPack()
        {
            string[] orr = Encoding.UTF8.GetString(CitySynth.Properties.Resources.factorypresets).Split('\n')
                .ToList().ConvertAll<string>(s => s.Trim()).ToArray();
            string[] raws = rawsets.Split('\n');
            List<string> expContent = new List<string>();
            foreach (string s in raws) if (!orr.Contains(s)) expContent.Add(s);
            List<object[]> bytes = new List<object[]>();
            foreach (string s in expContent) bytes.Add(new object[] { Encoding.ASCII.GetBytes(s.Trim()), Encoding.ASCII.GetByteCount(s.Trim()) });
            int cc = bytes.Sum<object[]>(o => ((byte[])o[0]).Length);
            List<int> init = (new int[]{1,1}).ToList();
            int newInds = 5;
            for (int i = 0; i < newInds; i++) init.Add(init[i] + init[i + 1]);
            init.AddRange(init.Reverse<int>().ToList());
            int[] datagaps =
                init.ToArray();

            int datagapindex = 0;
            double sizemul = datagaps.Sum(i => i + 1) / (double)datagaps.Length;
            int c = (int)Math.Ceiling(cc * sizemul);
            byte[] ret = new byte[c];
            int ret_index = 0;
            int byte_counter = 0, bytes_index = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < cc; i++)
            {
                if (byte_counter >= ((byte[])(bytes[bytes_index][0])).Length)
                {
                    bytes_index++;
                    byte_counter = 0;
                }
                ret[ret_index] = (byte)(((byte[])(bytes[bytes_index][0]))[byte_counter] + 127);
                ret_index++;
                for (int a = 0; a < datagaps[datagapindex]; a++)
                {
                    ret[ret_index] = (byte)r.Next(255);
                    ret_index++;
                }
                datagapindex++;
                datagapindex %= datagaps.Length;
            }
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "CitySynth PresetPack (*.spdx)|*.spdx";
            save.FileName = "PresetPack_" + Environment.UserName + "_" + DateTime.Now.ToShortDateString().Replace('/', '-');
            DialogResult dr = save.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = File.Create(save.FileName);
                fs.Write(ret, 0, ret.Length);
                fs.Flush();
                fs.Close();

            }
            save.Dispose();
        }
        #endregion
        #region Event Handlers

        #region Dials, Checkboxes and Buttons
        private void ampAttackDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Attack, sender); }
        private void ampDecayDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Decay, sender); }
        private void ampSustainDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Sustain, sender); }
        private void ampReleaseDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Release, sender); }

        private void filterCutoffDial_ValueChanged(object sender, EventArgs e) { if (!enableTouch) LinkVar(out R.LPF, sender); else LinkVar(out R.HPFCutoff, sender); }
        private void fftFilterToggle_CheckedChanged(object sender, EventArgs e) { LinkVar(out R.FFTEnable, sender); }

        private void envReleaseDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFrelease, sender); }
        private void envAttackDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFattack, sender); }
        private void envFloorDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFfloor, sender); }
        private void envCeilingDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFceiling, sender); }

        private void lfoRateDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFmodrate, sender); }
        private void lfoWidthDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.LPFwidth, sender); }

        private void ampLfoRateDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.AmpLFOrate, sender); }
        private void ampLfoWidthDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.AmpLFOwidth, sender); }

        private void delayLengthDial_ValueChanged(object sender, EventArgs e) { if (!enableTouch)LinkVar(out R.DelayTime, sender); else LinkVar(out R.ReverbDelay, sender); }
        private void delayWetDial_ValueChanged(object sender, EventArgs e) { if (!enableTouch)LinkVar(out R.DelayWet, sender); else LinkVar(out R.ReverbWet, sender); }

        private void pitchRateDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Pitchmod, sender); }
        private void pitchWidthDial_ValueChanged(object sender, EventArgs e) { LinkVar(out R.Pitchmodwidth, sender); }
        private void masterLevelDial_ValueChanged(object sender, EventArgs e) { R.SetMaster = ((CitySynth.UI_Elements.Dial)sender).GetScaledValue(); }

        private void osc1PhaseDial_ValueChanged(object sender, EventArgs e) { if (!enableTouch) LinkVar(out R.HarmonicPhase, sender); else LinkVar(out R.FilterFeedback, sender); }

        private void envActivateButton_CheckedChanged(object sender, EventArgs e) { LinkVar(out R.LPFenvelope, sender); }

        private void polyMonoSwitch_CheckedChanged(object sender, EventArgs e) { SwitchPolyMonoModes(); }
        private void touchPadEnableToggle_CheckedChanged(object sender, EventArgs e) { LinkVar(out enableTouch, sender); }

        private void closeWindowBtn_CheckChanged(object sender, EventArgs e)
        {
            R.Amplitude = 0;
            this.Close();
        }

        private void lfoHpfToggle_CheckChanged(object sender, EventArgs e)
        {
            if (lfoHpfToggle.Checked)
            {
                lfoLpfToggle.Checked = false;
                R.SendLFO = true;
            }
            else if (!lfoLpfToggle.Checked)
            {
                lfoHpfToggle.Checked = true;
                R.SendLFO = true;
            }
        }

        private void lfoLpfToggle_CheckChanged(object sender, EventArgs e)
        {
            if (lfoLpfToggle.Checked)
            {
                lfoHpfToggle.Checked = false;
                R.SendLFO = false;
            }
            else if (!lfoHpfToggle.Checked)
            {
                lfoLpfToggle.Checked = true;
                R.SendLFO = false;
            }
        }

        private void osc1GainDial_ValueChanged(object sender, EventArgs e)
        { 
            float db = 0;
            LinkVar(out db, sender);
            float dec = (float)Math.Pow(10, db / 10.0f);
            if (!enableTouch) {
                R.Gain = db;
                R.GeneralAtten = dec;
            }
            else R.FilterDrive = (48 + db) * 0.25f + 1;
        }
        private void osc1WaveSelDial_ValueChanged(object sender, EventArgs e)
        {
            int index = (int)(osc1WaveSelDial.Value * osc1WaveSelDial.SnapSegments);
            R.WFunction = (WaveFuntion)index;
        }

        #endregion

        #region Everything else UI

        /// <summary>
        /// Handles custom UI window dragging
        /// </summary>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Y <= 78)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        /// <summary>
        /// Handles touchpad controls to modify harmonic balance
        /// </summary>
        private void touchPad_MouseMove(object sender, MouseEventArgs e)
        {
            if (enableTouch)
            {
                R.Frequency = (float)Math.Round((e.X / (float)touchPad.Width) * 2500 + 20);
                if (R.keys[(int)Keys.ControlKey])
                    R.SubOscGain = (e.X / (float)touchPad.Width) * 0.5f;
                else
                {
                    R.HarmonicsControl = (e.Y / (float)touchPad.Height) * 2.5f + 0.1f;
                    R.Harmonic2Gain = (e.X / (float)touchPad.Width) * 0.5f;
                }
            }
        }

        /// <summary>
        /// Touchpad mouse enter. Depreciated.
        /// </summary>
        private void touchPad_MouseEnter(object sender, EventArgs e)
        {
            if (enableTouch)
                R.Amplitude = 1;
        }

        /// <summary>
        /// Touchpad mouse leave. Depreciated.
        /// </summary>
        private void touchPad_MouseLeave(object sender, EventArgs e)
        {
            if (enableTouch)
                R.Amplitude = 0;
        }


        /// <summary>
        /// Touchpad mouse click to toggle touchpad controls.
        /// </summary>
        private void touchPad_Click(object sender, EventArgs e)
        {
            touchPadEnableToggle.Checked = !touchPadEnableToggle.Checked;
        }

        /// <summary>
        /// Touchpad mouse double click to reset controls and Global Vars for continuity
        /// </summary>
        private void touchPad_DoubleClick(object sender, EventArgs e)
        {
            if (touchPadEnableToggle.Checked) touchPadEnableToggle.Checked = false;
            enableTouch = false;
            R.HarmonicsControl = 1;
            R.Harmonic2Gain = 0;
            R.FilterFeedback = 0;
            R.FilterDrive = 1;
            R.HPFCutoff = 0;
            R.HarmonicFunction = WaveFuntion.sin;
            R.MaxHarmonic = 5;
            R.SubOscGain = 0;
            R.FFTMode = 1;
            R.HarmonicV1 = true;
            masterLevelDial.SetScaledValue(10 * (float)Math.Log10(R.MasterLevel));

            if (devs != null && devs.Count == 0)
                R.BaseFrequency = 220;
            else
                R.BaseFrequency = 22.5f / 2;
            R.kb_off = 3;
            if (touchPadEnableToggle.Checked) touchPadEnableToggle.Checked = false;
        }

        /// <summary>
        /// Handles selection of active Dial control by dragging from toggle button
        /// </summary>
        private void lfoHpfToggle_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                try
                {
                    Point p = new Point(e.X + lfoHpfToggle.Location.X,
                        e.Y + lfoHpfToggle.Location.Y);
                    if (panel1.GetChildAtPoint(p).GetType() == osc1GainDial.GetType())
                    {
                        this.lfoHpfToggle.ChangeDisplayText(
                            panel1.GetChildAtPoint(p).Text);
                        varyingDial = panel1.GetChildAtPoint(p);
                        if (varyingDial == lfoRateDial || varyingDial == lfoWidthDial)
                        {
                            lfoHpfToggle.RestoreDisplayText();
                            varyingDial = null;
                            return;
                        }
                        lfoHpfToggle.Checked = true;
                    }
                    else this.lfoHpfToggle.RestoreDisplayText();

                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Handles preview of selection of active Dial control by dragging from toggle button
        /// </summary>
        private void lfoHpfToggle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            { 
                this.Cursor = Cursors.Cross;
                try
                {
                    Point p = new Point(e.X + lfoHpfToggle.Location.X,
                        e.Y + lfoHpfToggle.Location.Y);
                    if (panel1.GetChildAtPoint(p).GetType() == osc1GainDial.GetType())
                    {
                        this.lfoHpfToggle.ChangeDisplayText(
                            panel1.GetChildAtPoint(p).Text);
                    }

                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Handles resetting selection of active Dial control 
        /// </summary>
        private void lfoHpfToggle_DoubleClick(object sender, EventArgs e)
        {
            this.lfoHpfToggle.Text = "HPF...";
            varyingDial = null;
        }

        /// <summary>
        /// Handles preset selector preset button clicked.
        /// </summary>
        private void presetSelector_PresetButtonClicked(object sender, PresetButtonClickEventArgs e)
        {
            switch (e.ButtonIndex)
            {
                case 0:
                    ExportPresetPack();
                    break;
                case 1:
                    SavePreset();
                    break;
            }
        }

        /// <summary>
        /// Toggles recording of synth engine audio output to file
        /// </summary>
        private void recButton_ToggleChanged(object sender, ToggleEventArgs e)
        {
            if (e.State == ToggleState.Active)
            {
                if (aout != null)
                    aout.Stop();
                else
                    wout.Stop();
                wr.Dispose();
                recnotes = false;
                string notedata = rawrec;
                string sounddata = CurrentPresetToString("REC" + recStarted.ToString("HHmmddMMyyyy"), 0);
                ReplaceWaveProvider(cwp);
            }
            else
            {
                if (aout != null)
                    aout.Stop();
                else
                    wout.Stop();
                while (File.Exists(String.Format("temp_00{0}.wav", ++filenameIndex)))
                { }
                wr = new WaveRecorder(cwp, String.Format("temp_00{0}.wav", filenameIndex));
                rawrec = DateTime.Now.ToLongTimeString() + "\n";
                recnotes = true;
                recStarted = DateTime.Now;
                ReplaceWaveProvider(wr);
            }
        }

        /// <summary>
        /// Opens the record directory in explorer
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void recButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = Application.StartupPath;
                p.Start();
            }
        }
        
        #endregion

        
        #endregion


    }
}