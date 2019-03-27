using System;
using CitySynth.State;
using NAudio.Dsp;

namespace CitySynth.Audio.FX
{
    /// <summary>
    /// Static class for FFT filter implementation.
    /// </summary>
    public static class FFT
    {
        /// <summary>
        /// Sample rate
        /// </summary>
        public static int sr = 44100;
        /// <summary>
        /// LPF Cutoff frequency
        /// </summary>
        public static float lpf = 440;

        public const int FFT_FILTER0 = 0;
        public const int FFT_FILTER1 = 1;
        public const int FFT_FILTER2 = 2;

        /// <summary>
        /// Previous audio buffer for filter processing input
        /// </summary>
        private static short[][] lastbuffer = new short[3][];


        /// <summary>
        /// FFT implementaion of a two-pole LPF Butterworth filter
        /// </summary>
        /// <param name="buffer">Audio buffer for filter processing output</param>
        /// <param name="sampleCount">Buffer sample length</param>
        /// <param name="ss">Audio buffer for filter processing input</param>
        /// <param name="filterindex">LFP cutoff shape [0,1 or 3]</param>
        public static void ButterworthTwoPole(ref short[] buffer, int sampleCount, short[] ss, int filterindex)
        {
            int poles = 2;
            int cc = ss.Length;
            int ssbits = (int)Math.Floor(Math.Log(ss.Length, 2)) + 1;
            ssbits = Math.Max(ssbits, 9);
            int fftsize = (int)Math.Pow(2, ssbits);
            int lastbufferlength = fftsize - ss.Length;
            if (lastbuffer[filterindex] == null)
            {
                lastbuffer[filterindex] = new short[lastbufferlength];
            }
            Complex[] cpxs = new Complex[fftsize * 2];
            for (int i = 0; i < cc; i++)
            {
                cpxs[lastbufferlength + i].X = (float)ss[i] / short.MaxValue;
            }
            for (int i = 0; i < lastbufferlength; i++)
            {
                cpxs[i].X = (float)lastbuffer[filterindex][i] / short.MaxValue;
            }
            for (int i = 0; i < lastbufferlength - ss.Length; i++)
            {
                lastbuffer[filterindex][i] = lastbuffer[filterindex][i + ss.Length];
            }
            for (int i = 0; i < Math.Min(ss.Length, lastbufferlength); i++)
            {
                int bufferdif = Math.Max(0, ss.Length - lastbufferlength);
                lastbuffer[filterindex][bufferdif + lastbufferlength - ss.Length + i] = ss[bufferdif + i];
            }

           
            Array.Clear(ss, 0, cc);
            for (int i = fftsize; i < cpxs.Length; i++)
            {
                cpxs[i].X = 0;
            }
            {
                int bits = (int)Math.Floor(Math.Log(cpxs.Length, 2));
                FastFourierTransform.FFT(true, bits, cpxs);

                for (int i = 1; i < cpxs.Length / 2; i++)
                {
                    double f = ((double)(i) * sr / (cpxs.Length / 2));
                    float gain = 0;
                    switch (R.FFTMode)
                    {
                        case 1:
                            gain = (float)Math.Sqrt(1 / (1 + Math.Pow(f / lpf, 2 * poles)));
                            break;
                        case 2:
                            gain = (f < lpf) ? 0 : 1;
                            break;
                        case 3:
                            float maxf = R.SampleRate / 2;
                            gain = (float)Math.Sqrt(1 / (1 + Math.Pow((f - maxf) / (maxf - lpf), 2 * poles)));
                            break;
                        case 4:
                            maxf = R.SampleRate / 2;
                            poles = 3;
                            gain = (float)Math.Sqrt(1 / (1 + Math.Pow(f / lpf, 2 * poles)));
                            break;
                    }
                    cpxs[i].X *= gain;
                    cpxs[i].Y *= gain;
                    cpxs[cpxs.Length - i].X *= gain;
                    cpxs[cpxs.Length - i].Y *= gain;
                }
                FastFourierTransform.FFT(false, bits, cpxs);
            }
            for (int i = 0; i < cc; i++)
            {
                int cpxs_start = (fftsize - cc) / 2;

                float x = cpxs[i + cpxs_start].X;
                float y = cpxs[i + cpxs_start].Y;
                if (y == 0 && x != 0) { }
                short s = (short)(short.MaxValue * x);

                buffer[i * 2] = s;
                buffer[i * 2 + 1] = s;
                ss[i] = s;
            }
        }
        
    }
}
