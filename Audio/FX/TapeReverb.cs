using System;

namespace CitySynth.Audio.FX
{
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
}