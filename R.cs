using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CitySynth
{
    /// <summary>
    /// Static class for CitySynth global variables
    /// </summary>
    public static class R
    {
        public static float Amplitude = 0;
        public static float Amplitude1 = 0;
        public static float Gain = 1;
        public static float GeneralAtten = 0.3f;
        public static float MasterLevel = 1;
        public static float Frequency = 440;
        public static float BaseFrequency = 440;
        public static float Pitchmod = 0;
        public static float Pitchmodwidth = 0;
        public static float AmpLFOrate = 30;
        public static float AmpLFOwidth = 0;
        public static float LPF = 1200;
        public static float LPFmodrate = 0;
        public static float LFOreturn = 0;
        public static float HPFCutoff = 0;
        public static bool SendLFO = false;
        public static bool LPFenvelope = false;
        public static float LPFwidth = 500;
        public static float LPFfloor = 300;
        public static float LPFattack = 180;
        public static float LPFceiling = 5000;
        public static float LPFrelease = 30;
        public static float ReverbDelay = 100;
        public static float ReverbWet = 0.32f;
        public static float DelayTime = 100;
        public static float DelayWet = 0.5f;
        public static int SampleRate = 96000;//44100*2;
        public static int DelayBufferLength = SampleRate;
        public static int RevDelayBufferLength = SampleRate / 5;
        public static int MaxHarmonic = 5;
        public static int MaxVelocity = 127;
        public static int MidiOctaveShift = -3;
        public static int Voices = 12;
        public static int FFTMode = 1;
        public static float SubOscGain = 0;
        public static float HarmonicsControl = 1;
        public static float Harmonic2Gain = 0;
        public static float HarmonicPhase = 0;
        public static float FilterFeedback = 0.0f;
        public static float FilterDrive = 1f;
        public static float PitchBendFactor = 1;
        public static float Attack = 30, Decay = 50, Sustain = 0.55f, Release = 20;
        public static bool SusPedal = false, SusToSpacebar = false;
        public static bool StereoIm = false, SinV2 = false, MonoEnabled = false, Glide = false, FFTEnable = true, HarmonicFix = false, HarmonicV1 = true, FMScale = true, StereoReverb = true;
        public static bool tmp = false;
        public static List<int[]> ke = new List<int[]>();
        public static List<int> _ke = new List<int>();
        public static bool[] keys = new bool[512];
        public static int kb_off = 3;
        public static int kb_trem = 2;
        public static int ActiveVoices = 0;
        public static int RefreshRate = 30, SystemState = 1;
        public static int SystemSleepTime = 30;
        public static bool SystemPaused = true;
        public static WaveFuntion WFunction = WaveFuntion.sin;
        public static WaveFuntion HarmonicFunction = WaveFuntion.sin;

        /// <summary>
        /// Sets mixer master output volume level
        /// </summary>
        public static float SetMaster { set { MasterLevel = (float)Math.Pow(10, value / 10.0f); } get { return MasterLevel; } }

        /// <summary>
        /// Converts keyboard key press code to frequency of pitch
        /// </summary>
        /// <param name="keyCode">Computer keyboard key code</param>
        /// <returns>Frequency of pressed keyboard note</returns>
        public static float KeyToFreq(int keyCode)
        {
            return KeyToFreq(keyCode, BaseFrequency);
        }
        /// <summary>
        /// Converts keyboard key press code to frequency of pitch
        /// </summary>
        /// <param name="keyCode">Computer keyboard key code</param>
        /// <param name="a4Freq">Tuning of keyboard. Frequency of A4: </param>
        /// <returns>Frequency of pressed keyboard note</returns>
        public static float KeyToFreq(int keyCode, float a4Freq)
        {
            int st = -2;
            if (keyCode < 256)
            switch ((Keys)keyCode)
            {
                //Keyboard Keys
                case Keys.Q:
                    st = -1;
                    break;
                case Keys.A:
                    st = 0;
                    break;
                case Keys.W:
                    st = 1;
                    break;
                case Keys.S:
                    st = 2;
                    break;
                case Keys.E:
                    st = 3;
                    break;
                case Keys.D:
                    st = 4;
                    break;
                case Keys.F:
                    st = 5;
                    break;
                case Keys.T:
                    st = 6;
                    break;
                case Keys.G:
                    st = 7;
                    break;
                case Keys.Y:
                    st = 8;
                    break;
                case Keys.H:
                    st = 9;
                    break;
                case Keys.U:
                    st = 10;
                    break;
                case Keys.J:
                    st = 11;
                    break;
                case Keys.K:
                    st = 12;
                    break;
                case Keys.O:
                    st = 13;
                    break;
                case Keys.L:
                    st = 14;
                    break;
                case Keys.P:
                    st = 15;
                    break;
                case Keys.OemSemicolon:
                    st = 16;
                    break;
                case Keys.Oemtilde:
                    st = 17;
                    break;
            }
            else
                //if ((ind = ke.IndexOf(keyCode)) >= 0 && vels[ind] != 0)
            {
                st = keyCode - 300 + 12;
                a4Freq *= (float)Math.Pow(2, MidiOctaveShift);
            }

            if (st == -2)
                return -1;
            else
            {
                //float mult = 1;
                int key = kb_off + st;
                //if (key > 12) mult = key / 12;
                float factor = Form1.semitones[(key + 12) % 12] * (float)(Math.Pow(2, (int)Math.Floor(key / 12.0f)));
                return a4Freq * factor;
            }
        }

    }

    /// <summary>
    /// Shape of DCO generated waveforms
    /// </summary>
    public enum WaveFuntion
    {
        sin,
        saw,
        tri,
        bit,
        comp,
        pulse
    }

}