namespace CitySynth.Audio.FX
{
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
}