using System;
using CitySynth.State;

namespace CitySynth.Audio
{
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
}