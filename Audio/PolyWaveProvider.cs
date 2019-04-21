using System;
using System.Collections.Generic;
using NAudio.Wave;
using CitySynth.State;
using CitySynth.Helpers;
using System.Linq;
using CitySynth.Audio.FX;

namespace CitySynth.Audio.Providers
{
    /// <summary>
    /// Polyphonic synth wave provider with variable filter design, algorithmic reverb and modulating delay. 
    /// Manages variable number (no maximum voices) staked interval digital mono voices
    /// Also manages global level effects (reverb, delay, end-stage FFT or IIR filter), summing voices and sends
    /// </summary>
    public class PolyWaveProvider : WaveProvider16
    {
        #region var declarations

        /// <summary>
        /// Synth voices
        /// </summary>
        public static CustomWaveProvider[] cwps = new CustomWaveProvider[R.Voices];

        /// <summary>
        /// Audio sample buffer for FFT filter processing
        /// </summary>
        private static short[] filterSamples;

        /// <summary>
        /// Audio sample buffer for UI audio level monitoring
        /// </summary>
        public static int[] meteringSamples = new int[128];

        /// <summary>
        /// Circular audio sample buffer for UI audio signal waveform display
        /// </summary>
        public static short[] wavDisplaySamples = new short[R.SampleRate / R.RefreshRate];

        /// <summary>
        /// Index of circular audio sample buffer for UI audio signal waveform display
        /// </summary>
        private static int wavDisplayIndex = 0;


        /// <summary>
        /// Reverb unit
        /// </summary>
        public static ReverbProvider[] reverb;

        /// <summary>
        /// Delay unit
        /// </summary>
        private static Delay delay;

        /// <summary>
        /// Butterworth IIR Filter
        /// </summary>
        private static ButterworthFilter butterworth;
        
        /// <summary>
        /// Current buffer for synth voices to read into
        /// </summary>
        private float[] data = null;

        /// <summary>
        /// Queue for voices pending clean up process
        /// </summary>
        private List<int> pendingDel = new List<int>();

        /// <summary>
        /// Number of samples per channel to write into
        /// </summary>
        private int cc;
        #endregion

        public PolyWaveProvider(int sr)
        {
            // init WaveProvider with sample rate and number of channels: 2 (stereo)
            this.SetWaveFormat(sr, 2);
            for (int i = 0; i < cwps.Length; i++)
            {
                // inits and indexes all mono synth voices
                cwps[i] = new CustomWaveProvider();
                cwps[i].index = i;
            }

            // inits variable modlating delay unit
            delay = new Delay(sr, R.DelayBufferLength);

            // sets init delay time modulation amounts
            delay.SetFlutter(6.57, 0.01);

            // inits generalised reverb units
            reverb = new ReverbProvider[R.StereoReverb ? 2 : 1];
            
            // inits intergrated variable delay reverb unit
            // based on a custom modified Schroeder reverberator algorithm
            for (int i = 0; i < reverb.Length; i++) reverb[i] = new TapeReverb(sr);

            // inits custom saturating Butterworth filter adapted to run without latency at low computaional cost using IIR design
            butterworth = new ButterworthFilter(4, sr);
        }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            // buffer stereo sample count
            cc = sampleCount / WaveFormat.Channels;

            // init small length buffers for UI and effect processing
            if (delay.savedSamplesLength != cc) delay.SetSavedSamplesCount(cc);
            if (filterSamples == null) filterSamples = new short[cc];
            if (meteringSamples.Length > cc) meteringSamples = new int[cc];

            // init single voice output buffer
            if (data == null) data = new float[cc];

            // set length of delay time in seconds
            delay.SetLength(R.DelayTime / 1000);

            // Clear output buffers
            for (int i = 0; i < cc; i++)
            {
                buffer[2 * i] = 0;
                buffer[2 * i + 1] = 0;
            }

            // allow power saving mode to skip processing
            if (R.SystemPaused) return sampleCount;

            // Polyphonic Code
            else //if (!R.MonoEnabled)
            {
                // Read Inactive Synths
                for (int o = R.ke.Count; o < cwps.Length; o++)
                {
                    // set voice envelope input signal (CV) to 0. (Note OFF)
                    cwps[o].inamp = 0;
                    if (cwps[o].Running) // If key is up but voice is still alive (note still decaying) continue to add to output
                    {
                        // audio buffer retrieved from voice
                        cwps[o].Read(data, 0, cc);
                        for (int i = 0; i < cc; i++)
                        {
                            // summing of voice buffers to output
                            buffer[2 * i] += (short)data[i];
                            buffer[2 * i + 1] += (short)data[i];
                        }
                    }
                }

                //
                // Management of active voices 
                //

                // init list to queue dead voices for clean up
                pendingDel.Clear();
                int vc = R.ke.Count;
                int start = Math.Max(0, vc - (R.MonoEnabled ? 1 : R.Voices));

                // iterate thorugh active voices
                for (int o = 0; o < vc - start; o++)
                {
                    // index of active voices
                    int oo = o + start;
                    bool voiceDead = false;

                    // buffer key input for thread safety
                    int[] codes = R.ke[oo];
                    // verify the key buffer
                    if (codes == null) voiceDead = false;
                    // queue voice for clean up if it is not being held by the pedal and key is up
                    else if (!R.SusPedal) voiceDead = R._ke.Contains(codes[0]);

                    // set voice envelope input signal (CV) to 0 (Note OFF) or 1 (Note ON) 
                    cwps[o].inamp = voiceDead ? 0 : 1;

                    // Clean up active voices that have since died
                    if (!cwps[o].Running)
                    {
                        if (voiceDead)
                            pendingDel.Add(R.ke[oo][0]); // queue for clean up
                        else
                        {
                            // set master tuning for voice
                            cwps[o].afreq = R.BaseFrequency;
                            int[] keys = null;

                            // set voice velocity to key level
                            if (R.ke != null && R.ke.Count > oo && (keys = R.ke[oo]) != null)
                                cwps[o].vel = keys[1];
                        }
                    }
                    int[] key = R.ke[oo];
                    cwps[o].ff = MidiHelpers.KeyToFreq(key != null ? key[0] : 0, cwps[o].afreq);

                    // audio buffer retrieved from voice
                    cwps[o].Read(data, 0, cc);
                    for (int i = 0; i < cc; i++)
                    {
                        // summing of voice buffers to output and filter input
                        filterSamples[i] = buffer[2 * i] += (short)data[i];
                        buffer[2 * i + 1] += (short)data[i];
                    }
                }

                // Retrieve current number of active mono voices
                R.ActiveVoices = cwps.Count(cwp => cwp.Running);

                // Handle additional key up events in monophonic mode
                if (R.MonoEnabled)
                {
                    // buffer key input for thread safety
                    int[] codes = R._ke.ToArray();
                    // verify the key buffer
                    if (codes != null) foreach (int _ke in R._ke) { if (!pendingDel.Contains(_ke)) pendingDel.Add(_ke); };
                }
                // Remove voices pending clean up, Update Key Lists
                foreach (int i in pendingDel)
                {
                    int o = R.ke.ConvertAll<int>(a => a[0]).LastIndexOf(i);
                    for (int ii = o + 1; ii < cwps.Length; ii++)
                    {
                        if (cwps[ii].Running)
                        {
                            // pull active voices forward in index
                            CustomWaveProvider.SwapStates(cwps, o, ii);
                            break;
                        }
                    }
                    // get index of removed key in key down list
                    o = R.ke.ConvertAll<int>(a => a[0]).IndexOf(i);

                    // clean up key down and key up lists
                    R.ke.RemoveAt(o);
                    R._ke.Remove(i);
                }
            }

            // FFT Filter
            if (R.FFTEnable)
            {
                // Process output buffer with FFT filter
                FFT.ButterworthTwoPole(ref buffer, cc, filterSamples, FFT.FFT_FILTER0);
            }
            else
            {
                // Process output buffer with IIR butterworth filter
                butterworth.SetCutoffFrequency(FFT.lpf);
                for (int i = 0; i < cc; i++)
                    buffer[i * 2] = buffer[i * 2 + 1] = (short)(short.MaxValue * butterworth.Process(buffer[i * 2]/ (float)short.MaxValue));
            }

            // Delay Return
            float[] dsamps = delay.GetSavedSamples();
            for (int i = 0; i < cc; i++)
            {
                // Mixing delay return signal with output mix buffer
                short s = (short)(R.DelayWet * dsamps[i]);
                buffer[i * 2] += s;
                buffer[i * 2 + 1] += s;
            }

            // Reverb Send and Return
            for (int i = 0; i < cc; i++)
            {
                // Mixing reverb return signal with output mix buffer
                short s = buffer[2 * i];
                // Process mono reverb samples
                buffer[2 * i] = reverb[0].GetSample(s, R.ReverbWet);
                // Process another mono reverb sample for right channel if StereoReverb is enabled
                buffer[2 * i + 1] = R.StereoReverb ? reverb.Last().GetSample(s, R.ReverbWet) : buffer[2 * i]; 
            }

            // Delay Send and Wave Display Send
            for (int i = 0; i < cc; i++)
            {
                // sends samples to delay object after reverb send/return
                delay.sendSample(buffer[i * 2]);

                // provides small number of samples for waveform preview display
                wavDisplaySamples[wavDisplayIndex] = (short)-buffer[i * 2];
                wavDisplayIndex = (wavDisplayIndex + 1) % wavDisplaySamples.Length;
            }

            // Master Level Control
            if (R.MasterLevel != 1 || R.tmp)
            for (int i = 0; i < cc; i++)
            {
                // Attenuate output buffer at last stage
                buffer[i * 2] = (short)(buffer[i * 2] * R.MasterLevel);
                buffer[i * 2 + 1] = (short)(buffer[i * 2 + 1] * R.MasterLevel);
            }

            // Metering Send
            // provides small number of samples for UI volume level display
            for (int i = 0; i < meteringSamples.Length; i += cc / meteringSamples.Length)
                meteringSamples[i] = buffer[i * 2];

            // return number of samples processed. non-zero return keeps the realtime calling of Read()
            return sampleCount;
        }

        public void SwitchReverb()
        {
            // Enables reverb
            if (reverb[0].GetType() == typeof(TapeReverb))
                for (int i = 0; i < reverb.Length; i++)
                {
                    int[] cc = new int[reverb[i].GetBufferLengths().Length];
                    Array.Copy(reverb[i].GetBufferLengths(), cc, cc.Length);
                    //Audio.Providers.PolyWaveProvider.reverb[i].Dispose();
                    reverb[i] = new DigitalReverb(R.SampleRate, cc);
                }
            else
                for (int i = 0; i < reverb.Length; i++)
                    reverb[i] = new TapeReverb(R.SampleRate, reverb[i].GetBufferLengths());
        }
    }
}