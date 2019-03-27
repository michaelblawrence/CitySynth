using System;
using CitySynth.Enums;
using NAudio.Wave;
using CitySynth.State;
using CitySynth.Audio.FX;

namespace CitySynth.Audio
{
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
                FFT.lpf = lpf; // sends LPF cutoff to poly scope filters

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
}