using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace CitySynth
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
                    cwps[o].ff = R.KeyToFreq(key != null ? key[0] : 0, cwps[o].afreq);

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
                FFTs.FFTv3(ref buffer, cc, filterSamples, FFTs.FFT_FILTER0);
            }
            else
            {
                // Process output buffer with IIR butterworth filter
                butterworth.SetCutoffFrequency(FFTs.lpf);
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
    }

    /// <summary>
    /// Single harmonic synth voice wave provider
    /// Provides:
    ///     variable number stacked fixed-interval, variable-phase digital oscillators
    ///         (Sine, Triangle, Square, Sawtooth, Composite and Pulse available),
    ///     Voice-level RC design high and low pass filter (HPF, LPF)
    ///     ADSR envelope generator (EV),
    ///     frequency and amplitude modulation (FM/AM) via user defined LFOs
    /// </summary>
    public class CustomWaveProvider : WaveProvider16, IDisposable
    {
        #region var declarations

        /// <summary>
        /// Sample rate of wave provider
        /// </summary>
        int sr;
        /// <summary>
        /// Frequency of voice pitch in Hertz
        /// </summary>
        public float ff = 440;

        /// <summary>
        /// Tuning of voice. Frequency of A4 in Hz (default: 440 Hz)
        /// </summary>
        public float afreq = 440;
        /// <summary>
        /// Internal gain (value: 0 - 1)
        /// </summary>
        public float inamp = 0;
        /// <summary>
        /// Velocity of triggered key
        /// </summary>
        public float vel = 127;
        /// <summary>
        /// Running state of voice. Enabled when true, false otherwise
        /// </summary>
        public bool Running = false;
        /// <summary>
        /// Running state changed since last read
        /// </summary>
        public bool ChangedState = false;
        /// <summary>
        /// Index of DCO units waveform (Current position of waveform)
        /// </summary>
        float o = 0, o1 = 0, o2 = 0, o3 = 0, o4 = 0;
        /// <summary>
        /// Factor for harmonic DCO gain attenuation
        /// </summary>
        float ampl_fact = 0.90f, aff = 0.7f;
        /// <summary>
        /// Buffered samples for RC filter processing
        /// </summary>
        float prev = 0, prev11 = 0, prev1 = 0;
        float lpf;
        /// <summary>
        /// Number of seconds per sample
        /// </summary>
        float repsr;

        /// <summary>
        /// Monophonic mode is enabled when true, false otherwise
        /// </summary>
        bool manual = true;

        /// <summary>
        /// Running state of voice at point of last read
        /// </summary>
        bool prevRunning = false;

        /// <summary>
        /// ADSR AMP envelope
        /// </summary>
        ADSR adsr1;
        /// <summary>
        /// ADSR momentary filter mod envelope
        /// </summary>
        ADSR adsr2;
        /// <summary>
        /// ADSR filter envelope
        /// </summary>
        ADSR adsr3;

        /// <summary>
        /// Index of voice
        /// </summary>
        public int index = -1;
        #endregion

        #region init

        /// <summary>
        /// Transfers the current state of a given voice to this voice
        /// </summary>
        /// <param name="c">Source voice to copy from</param>
        public void CopyState(CustomWaveProvider c)
        {
            // Copy state of ASDR EGs
            this.adsr1.CopyState(c.adsr1);
            this.adsr2.CopyState(c.adsr2);

            // Copy variable values
            sr = c.sr;
            ff = c.ff;
            afreq = c.afreq;
            inamp = c.inamp;
            Running = c.Running; ChangedState = c.ChangedState;
            o = c.o; o2 = c.o2; o3 = c.o3; o4 = c.o4;
            if (!R.SendLFO) o1 = c.o1;
            ampl_fact = c.ampl_fact; aff = c.aff; vel = c.vel;
            prev = c.prev; prev11 = c.prev11;
            prev1 = c.prev1;
            lpf = c.lpf;
            repsr = c.repsr;
            manual = c.manual; prevRunning = c.prevRunning;

        }
        
        /// <summary>
        /// Initialises voice object with gloabl sample rate
        /// </summary>
        public CustomWaveProvider()
        {
            Setup(R.SampleRate);
        }

        /// <summary>
        /// Initialises voice object
        /// </summary>
        /// <param name="sr">Sample rate for voice</param>
        public CustomWaveProvider(int sr)
        {
            Setup(sr);
        }

        /// <summary>
        /// Initialises voice. Must be called before Read()
        /// </summary>
        /// <param name="sr"></param>
        private void Setup(int sr)
        {
            this.sr = sr;
            adsr1 = new ADSR(sr);
            adsr2 = new ADSR(sr);
            adsr3 = new ADSR(sr);
            this.SetWaveFormat(sr, 1);
            SetMaxHarmonic(R.MaxHarmonic);
            lpf = R.LPF;
        }

        #endregion

        /// <summary>
        /// Provides an buffer of given length of voice audio output
        /// </summary>
        /// <param name="buffer">16 bit sample buffer to fill</param>
        /// <param name="offset">Index at which to start filling buffer</param>
        /// <param name="sampleCount">Total number of buffer samples to fill across all channels</param>
        /// <returns>Number of generated samples</returns>
        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            float[] inbuffer = Array.ConvertAll<short, float>(buffer, Convert.ToSingle);
            int result = Read(inbuffer, offset, sampleCount);
            short[] outbuffer = Array.ConvertAll<float, short>(inbuffer, Convert.ToInt16);
            for (int i = 0; i < Math.Min(buffer.Length, outbuffer.Length); i++) buffer[i] = outbuffer[i];
            return result;
        }

        /// <summary>
        /// Provides an buffer of given length of voice audio output
        /// </summary>
        /// <param name="buffer">32 bit floating-point sample buffer to fill</param>
        /// <param name="offset">Index at which to start filling buffer</param>
        /// <param name="sampleCount">Total number of buffer samples to fill across all channels</param>
        /// <returns>Number of generated samples</returns>
        public int Read(float[] buffer, int offset, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                repsr = 1.0f / sr;
                if (!manual) // solo monophonic voice mode
                {
                    float glide = 20;
                    inamp = R.Amplitude;
                    if (R.Glide)
                    {
                        ff += inamp * (R.Frequency - ff) / (sr / glide);
                    }
                    else
                        ff = R.Frequency;

                }
                // compute FM LFO modulation rate
                float fmfreq = R.FMScale ? (float)Math.Exp(R.Pitchmod / 4 + 0) - 1 : R.Pitchmod;

                // computes fundamental frequency of voice
                float fund = ff * ((R.Pitchmodwidth * fmfreq != 0) ? (1 + R.Pitchmodwidth * sinwave(fmfreq, 1, o2) / short.MaxValue) : 1);
                fund *= R.PitchBendFactor;

                // increase interger indices used in generation of each DCO's periodic wavefunction
                o++;
                o %= (sr / fund);
                o1++;
                o1 %= (sr / R.LPFmodrate);
                o2++;
                o2 %= (sr / fmfreq);
                o3++;
                o3 %= (sr / R.AmpLFOrate);
                o4++;
                o4 %= (2 * sr / fund);

                // read AMP ASDR for amplitude modulation
                float ampl = adsr1.GetEnvelopeValue(inamp, R.Attack, R.Decay, R.Sustain, R.Release);
                ampl *= R.GeneralAtten * 0.5f * Math.Min(vel / R.MaxVelocity, 1) / ampl_fact;

                if ((short)(ampl * short.MaxValue) == 0) // run only for zero value (16-bit quantisation) samples
                {
                    // update state and ASDR EGs

                    Running = adsr1.Running;
                    ChangedState = Running != prevRunning;
                    buffer[i] = 0;
                    adsr3.GetEnvelopeValue(inamp, R.LPFattack, 0, 1, R.LPFrelease);
                    adsr2.GetEnvelopeValue(R.Amplitude1, 50, 0, 1, 250);
                    prev = prev1 = prev11 = 0;
                    prevRunning = Running;
                    continue;
                }
                // modulate the amp level by AMP LFO
                if (R.AmpLFOwidth > 0 && ampl > 0)
                {
                    ampl *= 1 - R.AmpLFOwidth / 2 + R.AmpLFOwidth / 2 * sinwave(R.AmpLFOrate, 1, o3) / (float)short.MaxValue;
                }
                // update state
                Running = adsr1.Running;
                ChangedState = Running != prevRunning;

                // init new sample for synth voice output
                float s = 0;
                
                // if envelope is open
                if (ampl > 0)
                {
                    // fill synth voice output sample with fundamental pitch sample
                    s = UseWave(R.WFunction, fund, ampl, o);
                    // enable generation of odd or even harmonics
                    int x = R.HarmonicV1 ? 1 : 0;
                    // itterate through chosen number of additional harmonics (overtones)
                    for (int ii = 2 + x; ii < R.MaxHarmonic + x * 2; ii++)
                    {
                        // calculate harmonics gain
                        float vol = ampl *= (aff * R.HarmonicsControl);
                        if (ii == 2) // apply additional control to 1st harmonic
                        {
                            if (R.HarmonicFix)
                                break;
                            else
                                vol = (ampl * 10 - vol) * R.Harmonic2Gain + vol;
                        }
                        float phase = (ii - x) * (sr / fund) * R.HarmonicPhase / R.MaxHarmonic;

                        // add harmonics output to synth voice output sample
                        s += UseWave(R.HarmonicFunction, fund * ii / (x + 1), vol, o + phase);
                    }
                    // add sub osc (- 1 Oct) DCO output to synth voice output sample
                    if (R.SubOscGain > 0) s += sinwave(fund / 2, ampl * R.SubOscGain, o4);
                }
                // retrives LPF cutoff from global
                lpf = R.LPF;
                // compute LFO to modulate LPF cutoff
                float sv = R.LPFmodrate > 0 ? sinwave(R.LPFmodrate, 1, o1++) / (float)short.MaxValue : 1;
                // compute EG to modulate LPF cutoff
                if (R.LPFenvelope)
                    lpf = R.LPFfloor + (R.LPFceiling - R.LPFfloor)
                        * adsr3.GetEnvelopeValue(inamp, R.LPFattack, 0, 1, R.LPFrelease);
                // allow voice to send LFO value to global vars
                if (R.SendLFO)
                {
                    if (R.LFOreturn == -2)
                            R.LFOreturn = R.LPFwidth * sv;
                }
                else lpf += R.LPFwidth * sv; // apply LPF cutoff LFO mod
                lpf += (100 - lpf) * adsr2.GetEnvelopeValue(R.Amplitude1, 50, 0, 1, 250); // apply LPF cutoff momentary key mod
                lpf = Math.Max(lpf, 0); // clamp modulated LFO value to be >= 0
                FFTs.lpf = lpf; // sends LPF cutoff to poly scope filters

                // RC high pass filter
                float hold = s;
                if (R.HPFCutoff > 0) s = HighPassFiler(prev1, s, prev11, R.HPFCutoff);

                // update output buffer with new sample
                buffer[i] = s;

                // buffer sample values for later processing
                prev11 = hold;
                prev1 = s;
                prev = s;
                prevRunning = Running;
            }

            // return full buffer
            return sampleCount;
        }

        #region Utils

        /// <summary>
        /// Transfers the current state of one voice to another in a given array of voices
        /// </summary>
        /// <param name="collection">Array of CustomWaveProvider voices</param>
        /// <param name="index1">Index of voice 1 to swap</param>
        /// <param name="index2">Index of voice 2 to swap</param>
        public static void SwapStates(CustomWaveProvider[] collection, int index1, int index2)
        {
            using (CustomWaveProvider cw = new CustomWaveProvider())
            {
                int[] cwp_indecies = new int[] { collection[index1].index, collection[index2].index };
                cw.index = collection[index1].index = cwp_indecies[0];
                cw.CopyState(collection[index1]);
                collection[index1].CopyState(collection[index2]);
                collection[index1].index = cwp_indecies[1];
                collection[index2].CopyState(cw);
            }
        }

        /// <summary>
        /// Attenuates the gain of the DCO outputs using the number of simultaneous DCOs (harmonics) playing
        /// </summary>
        /// <param name="count">Number of cocurrent harmonics playing</param>
        void SetMaxHarmonic(int count)
        {
            float total = 0;
            float af = total = ampl_fact;
            for (int i = 1; i < R.MaxHarmonic; i++)
            {
                total += af *= aff;
            }
            // sets global variable of number of harmonics sounding per voice
            R.MaxHarmonic = count;

            // updates harmonic-dependant attenuation factor
            ampl_fact = total;
        }

        /// <summary>
        /// Sets the state of the voice contorl to be global (mono synth compatible) or local (poly synth compatible)
        /// </summary>
        /// <param name="on">True to enable voice global control. False otherwise</param>
        /// <returns>Returns current CustomWaveProvider voice object</returns>
        public CustomWaveProvider SetManualMode(bool on)
        {
            manual = on;
            return this;
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Converts length of time period given in milliseconds to number of samples at the gloabal sample rate
        /// </summary>
        /// <param name="ms">Length of time period given in milliseconds</param>
        /// <returns>Equivalent number of samples for time period given</returns>
        float ms2sc(float ms)
        {
            return (int)((ms / 1000) * sr);
        }

        /// <summary>
        /// Provides switchable wave digitally controlled oscillator (DCO) sample output 
        /// </summary>
        /// <param name="wf">Wave shape for DCO</param>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short UseWave(WaveFuntion wf, float freq, float amp, float index)
        {
            // returns a DCO output when the requested wave amplitude is non-zero
            if (amp > 0)
                switch (wf)
                {
                    case WaveFuntion.sin:
                        if (R.SinV2)
                            return dsinwave(freq, amp, index);
                        else
                            return sinwave(freq, amp, index);
                    case WaveFuntion.bit:
                        return bitwave(freq, amp, index);
                    case WaveFuntion.tri:
                        return triwave(freq, amp, index);
                    case WaveFuntion.saw:
                        return sawwave(freq, amp, index);
                    case WaveFuntion.comp:
                        return compwave(freq, amp, index);
                    case WaveFuntion.pulse:
                        return pulsewave(freq, amp, index);
                }
            return 0;
        }

        #endregion

        #region Wavefunctions

        float dsinwave_def_freq = 0;

        /// <summary>
        /// Provides sine wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short sinwave(float freq, float amp, float index)
        {
            return (short)(short.MaxValue * (amp * Math.Sin((repsr * freq * 2 * Math.PI) * index)));
        }

        /// <summary>
        /// Provides sine wave digitally controlled oscillator (DCO) sample output (depreciated)
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short dsinwave(float freq, float amp, float index)
        {
            if (index % Math.Round(sr / (freq)) == 0)
                dsinwave_def_freq = freq;
            return (short)(short.MaxValue * (amp * Math.Sin((repsr * dsinwave_def_freq * 2 * Math.PI) * index)));
        }

        /// <summary>
        /// Provides square wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short bitwave(float freq, float amp, float index)
        {
            return (short)(short.MaxValue * (amp * Math.Pow(-1, (int)(repsr * freq * index * 2))));
        }

        /// <summary>
        /// Provides triangle wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short triwave(float freq, float amp, float index)
        {
            index %= (int)Math.Round(sr / (ff));
            float fl = (2 * Math.Abs(1 - repsr * freq * index * 2) - 1);
            return (short)(short.MaxValue * (amp * fl));
        }

        /// <summary>
        /// Provides alternative triangle wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short vintriwave(float freq, float amp, int index)
        {
            index %= (int)Math.Round(sr / ff);
            float fl = (Math.Abs(1 - repsr * freq * index * 4) - 1);
            return (short)(short.MaxValue * (amp * fl));
        }

        /// <summary>
        /// Provides sawtooth wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short sawwave(float freq, float amp, float index)
        {
            index %= (int)Math.Round(sr / ff);
            return (short)(short.MaxValue * (amp * (2 * (0.5 - repsr * freq * index))));
        }

        /// <summary>
        /// Provides composite wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short compwave(float freq, float amp, float index)
        {
            index %= sr / ff;
            float fl = (2 * Math.Abs(1 - repsr * freq * index * 2) - 1);
            return (short)(short.MaxValue * ((0.5 * amp * Math.Pow(-1, (int)(repsr * freq * index * 2))) + (0.5 * amp * fl)));
        }

        /// <summary>
        /// Provides pulse (bit) wave digitally controlled oscillator (DCO) sample output
        /// </summary>
        /// <param name="freq">Frequency of oscillator pitch</param>
        /// <param name="amp">Frequency of oscillator</param>
        /// <param name="index">Index position of oscillator cycle. Increment by one for each new contiguous sample</param>
        /// <returns>Returns single sample of DCO output</returns>
        short pulsewave(float freq, float amp, float index)
        {
            return (short)(short.MaxValue * (index * freq * repsr > 0.7 ? amp : -amp));
        }
        #endregion

        #region Simple RC Filters
        /// <summary>
        /// RC design Low Pass Filter (LPF)
        /// </summary>
        /// <param name="s1">Input sample for filtering</param>
        /// <param name="s2">Previous buffered input sample for filtering</param>
        /// <returns>Processed filtered sample</returns>
        short LowPassFiler(short s1, short s2)
        {
            double res = 0;
            double dt = 1.0d / sr;
            double RC = 1 / (lpf * 2 * Math.PI);
            double a = dt / (RC + dt);
            res = s2 + a * (s1 - s2);
            return (short)res;
        }

        /// <summary>
        /// RC design Low Pass Filter (LPF)
        /// </summary>
        /// <param name="s1">Input sample for filtering</param>
        /// <param name="s2">Previous buffered input sample for filtering</param>
        /// <returns>Processed filtered sample</returns>
        float LowPassFiler(float s1, float s2)
        {
            double res = 0;
            double dt = 1.0d / sr;
            double RC = 1 / (lpf * 2 * Math.PI);
            double a = dt / (RC + dt);
            res = s2 + a * (s1 - s2);
            return (float)res;
        }
        
        /// <summary>
        /// RC design High Pass Filter (HPF)
        /// </summary>
        /// <param name="s1">Previous buffered filtered output sample</param>
        /// <param name="s2">Input sample for filtering</param>
        /// <param name="s3">Previous buffered input sample for filtering</param>
        /// <param name="hpf">Cutoff frequency (Hz)</param>
        /// <returns>Processed filtered sample</returns>
        short HighPassFiler(short s1, short s2, short s3, float hpf)
        {
            double res = 0;
            double dt = 1.0d / sr;
            double RC = 1 / (hpf * 2 * Math.PI);
            double a = dt / (RC + dt);
            res = a * (s1 + s2 - s3);
            return (short)res;
        }

        /// <summary>
        /// RC design High Pass Filter (HPF)
        /// </summary>
        /// <param name="s1">Previous buffered filtered output sample</param>
        /// <param name="s2">Input sample for filtering</param>
        /// <param name="s3">Previous buffered input sample for filtering</param>
        /// <param name="hpf">Cutoff frequency (Hz)</param>
        /// <returns>Processed filtered sample</returns>
        float HighPassFiler(float s1, float s2, float s3, float hpf)
        {
            double res = 0;
            double dt = 1.0d / sr;
            double RC = 1 / (hpf * 2 * Math.PI);
            double a = RC / (RC + dt);
            res = a * (s1 + s2 - s3);
            return (float)res;
        }
        #endregion
    }

    /// <summary>
    /// Abstract reverb unit based upon Schroeder reverberator algorithm 
    /// Adaptation of Schroeder, M.R. & Logan, B.F.. (1961). Colorless artificial reverberation. Audio, IRE Transactions on. AU-9. 209 - 214. 10.1109/TAU.1961.1166351.
    /// Designed to integrate within modulated delay send and receive points for optimum control
    /// </summary>
    public abstract class ReverbProvider : IDisposable
    {
        #region var declarations
        /// <summary>
        /// Number of nested delays for Schroeder reverberator
        /// </summary>
        protected static int c = 5;
        /// <summary>
        /// Random number generator
        /// </summary>
        protected static Random r = new Random(DateTime.Now.Millisecond);

        // constants for Schroeder reverberator algorithm
        protected const float g = 0.708f;
        protected const float g2 = 1 - g * g;
        protected double tau;

        /// <summary>
        /// Indecies of reverb nested delay circular buffers
        /// </summary>
        protected int[] b_inds = new int[c];
        /// <summary>
        /// Lengths of nested delays in number of samples
        /// </summary>
        protected int[] ccs = new int[c];
        /// <summary>
        /// Maximum length of reverb predelay circular buffer
        /// </summary>
        protected static int bcc = R.RevDelayBufferLength;
        /// <summary>
        /// Circular sample buffer for reverb predelay
        /// </summary>
        protected short[] predelay = new short[bcc];
        /// <summary>
        /// Index of reverb predelay circular buffer
        /// </summary>
        protected int dindex = 0;
        /// <summary>
        /// Sample Rate
        /// </summary>
        protected int sr;

        /// <summary>
        /// Enable to bypass effect processing on GetSample calls
        /// </summary>
        protected bool bypass = false;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ReverbProvider class
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        public ReverbProvider(int sr)
        {
            this.sr = sr;

            int attempts = 0;
            B:
            tau = Math.Pow(3, c - 1) / 1000;

            double t1 = sr * tau;
            t1 += 0.01 * t1 * (2 * (0.5 * r.NextDouble() - 1));

            // Compute new nested delay buffer lengths
            ccs[0] = (int)t1;
            for (int i = 1; i < ccs.Length; i++)
            {
                // attempt constrained PRNG assignment of nested delay buffer lengths
                while (true)
                {
                    // if no solution can be found, prompt user on next step
                    if (attempts++ >= 100)
                    {
                        System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(
                            "Try setting audio driver sample rate to greater value",
                            "Reverb initialisation Error", System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore,
                            System.Windows.Forms.MessageBoxIcon.Error);
                        if (dr == System.Windows.Forms.DialogResult.Abort)
                            System.Windows.Forms.Application.Exit();
                        else if (dr == System.Windows.Forms.DialogResult.Retry) attempts = 0;
                        else
                        {
                            bypass = true;
                            break;
                        }

                    }
                    double ran = 0.023;
                    double ranfact = (3 - ran) + ran * r.NextDouble();
                    t1 /= ranfact;
                    int len = (int)(t1);
                    if (len == 0) goto B;
                    if (ccs[i - 1] % len == 0) continue;
                    ccs[i] = len;
                    break;
                }
            }
            InitBuffers();
        }

        /// <summary>
        /// Initializes a new instance of the ReverbProvider class with given nested delay buffer lengths
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        /// <param name="bufferLengths">Nested delay buffer lengths Schroeder reverberator algorithm</param>
        public ReverbProvider(int sr, int[] bufferLengths)
        {
            this.sr = sr;
            Array.Copy(bufferLengths, ccs, ccs.Length);
            InitBuffers();
        }

        /// <summary>
        /// Initialise the nested delay buffers
        /// </summary>
        abstract protected void InitBuffers();
        
        /// <summary>
        /// Modify the value of a sample at a given index
        /// </summary>
        /// <param name="index">Index of sample</param>
        /// <param name="value">Value of sample</param>
        abstract protected void ModifySample(int index, float value);

        /// <summary>
        /// Retrieve value of a sample at a given index
        /// </summary>
        /// <param name="index">Index of sample</param>
        /// <returns>Value of a sample</returns>
        abstract protected float GetDelaySample(int index);

        /// <summary>
        /// Clean up nested delay buffers
        /// </summary>
        abstract protected void DisposeBuffers();

        /// <summary>
        /// Get processed sample of reverb effect
        /// </summary>
        /// <param name="sampleIn">Input sample to be processed</param>
        /// <param name="wet">Propotion of proccesed sample to be returned (value: 0->1)</param>
        /// <returns>Processed sample</returns>
        public short GetSample(short sampleIn, float wet)
        {
            if (bypass) return sampleIn;
            predelay[dindex] = sampleIn;
            dindex = (int)(bcc + dindex - Math.Min(sr * R.ReverbDelay / 1000f, bcc - 1)) % bcc;
            float input = predelay[dindex];
            float sampleOut = 0;

            for (int i = 0; i < c; i++)
            {
                sampleOut = -input * g;
                float delayed = GetDelaySample(i);
                ModifySample(i, input + delayed * g);
                sampleOut += g2 * delayed;
                input = sampleOut;
            }

            return (short)(-wet * sampleOut + (1 - wet) * sampleIn);
        }

        /// <summary>
        /// Returns lengths of nested delays
        /// </summary>
        /// <returns>Array of sample counts of each nested delay length</returns>
        public int[] GetBufferLengths()
        {
            return ccs;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    DisposeBuffers();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ReverbProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Custom non-modulating mono reverb unit based upon Schroeder reverberator algorithm 
    /// Adaptation of Schroeder, M.R. & Logan, B.F.. (1961). Colorless artificial reverberation. Audio, IRE Transactions on. AU-9. 209 - 214. 10.1109/TAU.1961.1166351.
    /// Designed to integrate within modulated delay send and receive points for optimum control
    /// </summary>
    public class DigitalReverb : ReverbProvider
    {
        /// <summary>
        /// Simple buffers (delays) for Schroeder reverberator algorithm
        /// </summary>
        float[][] reverbDelays = new float[c][];

        /// <summary>
        /// Initializes a new instance of the DigitalReverb class
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        public DigitalReverb(int sr) : base(sr)
        {

        }
        public DigitalReverb(int sr, int[] bufferLengths) : base(sr, bufferLengths)
        {

        }

        protected override void InitBuffers()
        {
            // init delay buffers for Schroeder reverberator algorithm
            for (int i = 0; i < reverbDelays.Length; i++)
                reverbDelays[i] = new float[ccs[i]];
        }

        protected override void ModifySample(int index, float sample)
        {
            // modify required delay sample with sample provided by extended ReverbProvider
            reverbDelays[index][b_inds[index] = (b_inds[index] + 1) % ccs[index]] = sample;
        }

        protected override float GetDelaySample(int index)
        {
            // provide extended ReverbProvider algoritm with required delay sample
            return reverbDelays[index][(b_inds[index] + 1) % ccs[index]];
        }

        protected override void DisposeBuffers()
        {
        }
    }

    /// <summary>
    /// Custom pitch modulating mono reverb unit based upon Schroeder reverberator algorithm 
    /// Adaptation of Schroeder, M.R. & Logan, B.F.. (1961). Colorless artificial reverberation. Audio, IRE Transactions on. AU-9. 209 - 214. 10.1109/TAU.1961.1166351.
    /// Designed to integrate within modulated delay send and receive points for optimum control
    /// </summary>
    public class TapeReverb : ReverbProvider
    {
        /// <summary>
        /// Delay units for Schroeder reverberator algorithm
        /// </summary>
        Delay[] reverbDelays = new Delay[c];

        /// <summary>
        /// Initializes a new instance of the TapeReverb class
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        public TapeReverb(int sr) : base(sr)
        {

        }
        public TapeReverb(int sr, int[] bufferLengths) : base(sr, bufferLengths)
        {

        }

        protected override void InitBuffers()
        {
            Random r = new Random();
            for (int i = 0; i < reverbDelays.Length; i++)
            {
                // init delay buffers for Schroeder reverberator algorithm
                reverbDelays[i] = new Delay(sr, ccs[i], 1);
                // produce randomly modulating delay units for reverb
                reverbDelays[i].SetFlutter(6 + 3 * r.NextDouble(), 0.0025);
            }
        }

        protected override void ModifySample(int index, float sample)
        {
            // modify required delay sample with sample provided by extended ReverbProvider
            reverbDelays[index].sendSample(sample);
        }

        protected override float GetDelaySample(int index)
        {
            // provide extended ReverbProvider algoritm with required delay sample
            return reverbDelays[index].ReturnSampleF();
        }

        protected override void DisposeBuffers()
        {
        }
    }

    /// <summary>
    /// Custom modulating pitch delay unit modelling a fixed length tape echo hardware unit
    /// Designed to integrate with modulated reverb for optimum control
    /// </summary>
    public class Delay
    {
        /// <summary>
        /// Floating point position of circular buffer index (value 0->Max Length in samples)
        /// </summary>
        private double index = 0;
        /// <summary>
        /// Previous position of circular buffer index (value 0->Max Length in samples)
        /// </summary>
        private double pindex = 0;
        /// <summary>
        /// Circular buffer for tape
        /// </summary>
        private float[] samples;
        /// <summary>
        /// Position of circular buffer index (value 0->Max Length in samples)
        /// </summary>
        public int Index { get { return (int)(index); } }
        /// <summary>
        /// Amount of flutter effect modulation
        /// </summary>
        public double Flutter { get; set; }
        /// <summary>
        /// Rate of flutter effect modulation
        /// </summary>
        public double FlutterSpeed { get; set; }
        /// <summary>
        /// Sample rate at which to run Delay
        /// </summary>
        private int sr = R.SampleRate;
        /// <summary>
        /// Index to track progress of flutter sine wave modulation
        /// </summary>
        private int intclock = 0;
        /// <summary>
        /// Length of delay
        /// </summary>
        private int cc = -1;

        /// <summary>
        /// Speed at which tape reel is spinning
        /// </summary>
        public double reelSpeed = 1, setreelSpeed = 1;
        /// <summary>
        /// Time legnth of the tape reel
        /// </summary>
        private double reelSeconds = -1;


        private float[] savedSamples, saveReturn;
        public int savedSamplesLength = -1;
        private int savedSamplesIndex = -1;

        /// <summary>
        /// Initializes a new instance of the Tape Delay class
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        /// <param name="len">Maximum length, in samples, of the delay buffer</param>
        public Delay(int sr, int len) : this(sr, len, 6)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Tape Delay class
        /// </summary>
        /// <param name="sr">Sample rate for processing</param>
        /// <param name="len">Maximum length, in samples, of the delay buffer</param>
        /// <param name="scale">Interpolation factor of tape length. Higher the value, lower the write rate to the tape (lower fidelity, more Lo-Fi)</param>
        public Delay(int sr, int len, int scale)
        {
            len /= scale;
            Flutter = 0.001;
            FlutterSpeed = 6;
            samples = new float[len];
            cc = len;
            this.sr = sr;
            reelSeconds = len / (double)sr;
        }

        /// <summary>
        /// Writes given sample to tape at current position and saves returned sample to buffer
        /// </summary>
        /// <param name="s">Sample to write</param>
        public void sendSample(short s)
        {
            sendSample((float)s);
        }

        /// <summary>
        /// Writes given sample to tape at current position and saves returned sample to buffer
        /// </summary>
        /// <param name="s">Sample to write</param>
        public void sendSample(float s)
        {
            int index = (int)this.index,
                previndex = (int)pindex;
            float[] ss = reelSpeed > 1 ?
                ArrayInterpol(this.index, reelSpeed, s) 
                : ArrayInterpol(this.index, s);
            if (previndex != index)
            {
                samples[(index + 1) % cc] = 0;
            }
            else previndex = (index - 1 + cc) % cc;
            
            // modulate reelSpeed with flutter parameters
            if (Flutter > 0) reelSpeed = setreelSpeed - setreelSpeed * Flutter * (1+Math.Sin(2 * Math.PI * (intclock / (double)sr) * FlutterSpeed));

            for (int i = 0; i < ss.Length; i++)
            {
                samples[(index + i) % cc] += ss[i] * (float)Math.Sqrt(reelSpeed); //(1 / (float)/*Math.Sqrt*/(reelSpeed));
            }

            //update state
            pindex = this.index;
            this.index = (this.index + reelSpeed) % cc;
            intclock = (intclock + 1) % sr;

            // append processed sample return to saved buffer
            if (savedSamplesLength > 0)
                savedSamples[savedSamplesIndex = ((savedSamplesIndex + 1) % savedSamplesLength)] = ReturnSample();
        }

        /// <summary>
        /// Sets the length of the saved return sample buffer
        /// </summary>
        /// <param name="len">New length of buffer</param>
        public void SetSavedSamplesCount(int len)
        {
            if (savedSamplesLength != len)
            {
                savedSamples = new float[len];
                saveReturn = new float[len];
            }
            savedSamplesLength = len;

        }

        /// <summary>
        /// Retrieves return sample buffer 
        /// </summary>
        /// <returns>Saved return buffer</returns>
        public float[] GetSavedSamples()
        {
            return GetSavedSamples(savedSamplesLength);
        }
        /// <summary>
        /// Retrieves return sample buffer 
        /// </summary>
        /// <param name="len">(Not used)</param>
        /// <returns>Saved return buffer</returns>
        public float[] GetSavedSamples(int len)
        {
            int nextIndex = (savedSamplesIndex + 1) % savedSamplesLength;
            Array.Clear(saveReturn, 0, saveReturn.Length);
            Array.Copy(savedSamples, nextIndex, saveReturn, 0, savedSamplesLength - nextIndex);
            Array.Copy(savedSamples, 0, saveReturn, savedSamplesLength - nextIndex, nextIndex);
            return saveReturn;
        }

        /// <summary>
        /// Sets the average length of delay effect
        /// </summary>
        /// <param name="seconds">Seconds between input and output of a given sample</param>
        public void SetLength(double seconds)
        {
            if (seconds > 0)
                reelSpeed = setreelSpeed = reelSeconds / seconds;
        }

        /// <summary>
        /// Sets the parameters for tape 'flutter' effect
        /// </summary>
        /// <param name="rate">Rate of delay time/pitch modulation</param>
        /// <param name="intensity">Intensity of delay time/pitch modulation</param>
        public void SetFlutter(double rate, double intensity)
        {
            Flutter = intensity;
            FlutterSpeed = rate;
        }

        /// <summary>
        /// Returns processed sample of delay effect. (Called at sample send if SavedSamplesCount is non-zero)
        /// </summary>
        /// <returns>Processed sample</returns>
        public float ReturnSampleF()
        {
            int ii = (int)index;
            int offset = (int)(2+reelSpeed);
            double rem = index - ii;
            return (float)((1 - rem) * samples[(ii + offset) % cc] + rem * samples[(ii + offset + 1) % cc]);
        }

        /// <summary>
        /// Returns processed sample of delay effect. (Called at sample send if SavedSamplesCount is non-zero)
        /// </summary>
        /// <returns>Processed sample</returns>
        public short ReturnSample()
        {
            return (short)ReturnSampleF();
        }

        /// <summary>
        /// Linear interpolation of single sample to allow for temporal stretching. Allowing for low tape speeds and pitch shifting.
        /// </summary>
        /// <param name="position">Floating point index representation</param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static float[] ArrayInterpol(double position, float value)
        {
            int ipos = (int)position;
            double rem = position - ipos;
            return new float[] { (float)(value * (1 - rem)), (float)(value * rem) };
        }

        /// <summary>
        /// Linear interpolation of single sample to allow for temporal stretching. Allowing for low tape speeds and pitch shifting.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="speed">Tape speed</param>
        /// <param name="value">Input sample for stretching</param>
        /// <returns>Audio buffer containing the stretched input sample</returns>
        private static float[] ArrayInterpol(double position, double speed, float value)
        {
            int ipos = (int)position;
            double rem = position - ipos;
            float val1 = (float)(value * (1 - rem)),
                val2 = (float)(value * rem),
                val3 = (val1+val2)/2;

            int lenpast = (int)(speed + 1);

            float[] ret = new float[lenpast + 1];
            ret[0] = val1;
            for (int i = 1; i < lenpast; i++) ret[i] = val3;
            ret[lenpast] = val2;
            return ret;
        }
    }

    /// <summary>
    /// Envelope generator for modulating volume of signal from the synth voice digital oscillators
    /// ADSR (Attack, Decay, Sustain and Release) control of envelope output
    /// </summary>
    public class ADSR
    {
        /// <summary>
        /// Index of current state
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Current state of envelope generator
        /// </summary>
        public EnvelopeState CurrentState { get { return (EnvelopeState)State; } }
        /// <summary>
        /// Number of passed samples (calls to GetEnvelopeValue) since last change in State
        /// </summary>
        int Position { get; set; }
        /// <summary>
        /// Current sustain level
        /// </summary>
        float Sustain { get; set; }
        /// <summary>
        /// Envelope generator is currently open (Key pressed or release ramp active)
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Sample rate at which to run EG objects
        /// </summary>
        public static float SampleRate { get; set; }

        float prevState = 1;
        private float a, d, s, r;

        /// <summary>
        /// Initialises a new ASDR envelope generator object
        /// </summary>
        /// <param name="sampleRate">Sample rate at which to run EG</param>
        public ADSR(int sampleRate)
        {
            ADSR.SampleRate = sampleRate;
        }

        /// <summary>
        /// Converts length of time period given in milliseconds to number of samples at the gloabal sample rate
        /// </summary>
        /// <param name="ms">Length of time period given in milliseconds</param>
        /// <returns>Equivalent number of samples for time period given</returns>
        static float ms2sc(float ms)
        {
            return (int)((ms / 1000) * SampleRate);
        }

        /// <summary>
        /// Copy current state from a given ASDR object to this object
        /// </summary>
        /// <param name="a1"></param>
        public void CopyState(ADSR a1)
        {
            this.State = a1.State;
            this.Position = a1.Position;
            this.Sustain = a1.Sustain;
            this.Running = a1.Running;
            this.prevState = a1.prevState;
            this.a = a1.a;
            this.d = a1.d;
            this.s = a1.s;
            this.r = a1.r;
        }

        /// <summary>
        /// State of ASDR envelope generator
        /// </summary>
        public enum EnvelopeState
        {
            AwaitingKeyPress = -1,
            AttackRamping = 0,
            DecayRamping = 1,
            Sustaining = 2,
            ReleaseRamping = 3,
        }

        /// <summary>
        /// Get output level of ASDR envelope generator for current sample
        /// </summary>
        /// <param name="amplitude">Maximum amplitude of envelope</param>
        /// <returns>ASDR envelope generator modulation output (value 0.f -> 1.f)</returns>
        public float GetEnvelopeValue(float amplitude)
        {
            float output = 0;
        A:
            switch (State)
            {
                case -1:
                    // At Zero level and Waiting for Key Press
                    if (amplitude == 1 && amplitude != prevState)
                    {
                        Position = 0;
                        State++;
                        goto A;
                    }
                    break;
                case 0:
                    // Attack ramp-up to full output level
                    if (Position >= ms2sc(a))
                    {
                        Position = 0;
                        State++;
                        goto A;
                    }
                    output = (Position / ms2sc(a));
                    Position++;
                    break;
                case 1:
                    // Decay ramp-down to Sustain output level
                    if (Position >= ms2sc(d))
                    {
                        Position = 0;
                        State++;
                        Sustain = s;
                        goto A;
                    }
                    output = (1 - (1 - s) * Position / ms2sc(d));
                    Position++;
                    break;
                case 2:
                    // At Sustain level and waiting for key off
                    if ((prevState != amplitude && amplitude == 0) || amplitude == 0)
                    {
                        Position = 0;
                        State++;
                        goto A;
                    }
                    output = s;
                    break;
                case 3:
                    // Release ramp-down to Zero output level
                    if (Position >= ms2sc(r))
                    {
                        Position = 0;
                        State = -1;
                    }
                    output = Sustain - Sustain * Position / ms2sc(r);
                    Position++;
                    break;
            }

            // change current state to release ramp down on key up
            if (amplitude == 0 && amplitude != prevState && State != 3)
            {
                Position = 0;
                State = 3;
                Sustain = output;
            }
            // allow relase ramping down EG to ramp up with attack on key press
            if (amplitude == 1 && amplitude != prevState && State == 3)
            {
                Position = 0;
                State = 0;
            }

            // update state
            Running = (State != -1);
            prevState = amplitude;

            // return EG output
            return output;
        }

        /// <summary>
        /// Get output level of ASDR envelope generator for current sample
        /// </summary>
        /// <param name="amplitude">Maximum amplitude of envelope</param>
        /// <param name="a">Attack time in milliseconds</param>
        /// <param name="d">Decay time in milliseconds</param>
        /// <param name="s">Sustain level</param>
        /// <param name="r">Release time in milliseconds</param>
        /// <returns>ASDR envelope generator modulation output (value 0.f -> 1.f)</returns>
        public float GetEnvelopeValue(float amplitude, float a, float d, float s, float r)
        {
            this.a = a;
            this.d = d;
            this.s = s;
            this.r = r;
            return GetEnvelopeValue(amplitude);
        }
    }
}