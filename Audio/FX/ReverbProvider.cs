using System;
using CitySynth.State;

namespace CitySynth.Audio.FX
{
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
}