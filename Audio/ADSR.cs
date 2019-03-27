namespace CitySynth.Audio
{

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