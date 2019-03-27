using System;
using System.Numerics;
using CitySynth.State;

namespace CitySynth
{
    /// <summary>
    /// Butterworth IIR filter implementation.
    /// </summary>
    public class ButterworthFilter
    {
        /// <summary>
        /// LPF cutoff frequency.
        /// </summary>
        private double fc;

        /// <summary>
        /// The IIR coefficients for previous output.
        /// </summary>
        private double[] IIRCoefsA;
        /// <summary>
        /// The IIR coefficients for previous input.
        /// </summary>
        private double[] IIRCoefsB;

        /// <summary>
        /// Sample rate.
        /// </summary>
        private int sr;

        /// <summary>
        /// Buffered previous input
        /// </summary>
        private float[] past_x;
        /// <summary>
        /// Buffered previous output
        /// </summary>
        private float[] past_y;

        /// <summary>
        /// Filtered output single sample buffer
        /// </summary>
        float output = 0;

        /// <summary>
        /// Gets or sets the LPF cutoff frequency.
        /// </summary>
        /// <value>The cutoff frequency.</value>
        public double CutoffFrequency { get { return fc; } set { fc = value; } }
        /// <summary>
        /// Gets or sets the number of poles.
        /// </summary>
        /// <value>Number of poles.</value>
        public int Poles { get; set; }
        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>Sample rate.</value>
        public int SampleRate { get { return sr; } }

        /// <summary>
        /// Initializes a new instance of an n-th order <see cref="T:CitySynth.ButterworthFilter"/>.
        /// </summary>
        /// <param name="poles">Number of poles.</param>
        /// <param name="sampleRate">Sample rate.</param>
        public ButterworthFilter(int poles, int sampleRate)
        {
            Poles = poles;
            sr = sampleRate;
            past_x = new float[poles];
            past_y = new float[poles];

            for (int i = 0; i < poles; i++)
            {
                past_x[i] = past_y[i] = 0;
            }
        }

        /// <summary>
        /// Processes an audio sample for filtering.
        /// </summary>
        /// <returns>Output filtered audio sample.</returns>
        /// <param name="x">Input audio sample.</param>
        public float Process(float x)
        {
            x += output * R.FilterFeedback;
            double y = x * IIRCoefsB[0];
            for (int i = 1; i < IIRCoefsB.Length; i++)
            {
                y += past_x[past_x.Length - i] * IIRCoefsB[i];
            }
            for (int i = 1; i < IIRCoefsA.Length; i++)
            {
                y -= past_y[past_y.Length - i] * IIRCoefsA[i];
            }
            for (int i = 1; i < past_x.Length; i++)
            {
                past_x[i - 1] = past_x[i];
                past_y[i - 1] = past_y[i];
            }
            if (double.IsInfinity(y) || double.IsNaN(y))
                y = 0;

            past_x[past_x.Length - 1] = x;

            past_y[past_y.Length - 1] = (float)y;
            
            y = Math.Sin(Math.Max(Math.Min(y * R.FilterDrive, 1), -1) * Math.PI / 2) / R.FilterDrive;

            output = (float)y;

            return output;
        }

        /// <summary>
        /// Sets the LPF cutoff frequency, recalculates IIR coefficients.
        /// </summary>
        /// <param name="hz">LPF cutoff frequency in Hz.</param>
        public void SetCutoffFrequency(double hz)
        {
            int n = Poles;
            double pi = Math.PI;
            CutoffFrequency = hz = Math.Max(hz, 250);
            double omegac = NormaliseFrequency(hz);
            Complex[] sk = new Complex[n];
            for (int k = 0; k < n / 2; k++)
            {
                sk[2 * k] = Complex.FromPolarCoordinates(1, (2 * k + n + 1) * pi / (2 * n));
                sk[2 * k + 1] = Complex.FromPolarCoordinates(1, (2 * (n - k - 1) + n + 1) * pi / (2 * n));
            }
            if (n % 2 == 1)
                sk[n - 1] = Complex.FromPolarCoordinates(1, (2 * (n - 1) + n + 1) * pi / (2 * n));
            Complex[] pk = new Complex[n];
            for (int k = 0; k < n; k++)
                pk[k] = sk[k] * omegac;
            double H0 = Math.Pow(-omegac, n);
            double H0z_denom = 1;
            for (int k = 0; k < n / 2; k++)
            {
                double value = (1 - pk[2 * k].Real) * (1 - pk[2 * k].Real) - (pk[2 * k].Imaginary * pk[2 * k + 1].Imaginary);
                H0z_denom *= value;
            }
            if (n % 2 == 1)
                H0z_denom *= 1 - pk[n - 1].Real;
            double H0z = H0 / H0z_denom;

            Complex[] Hz_numer_factor = new Complex[n + 1];
            Hz_numer_factor[0] = 1; Hz_numer_factor[1] = 1;

            PolynomialMultiplier Hz_numer_polyn = new PolynomialMultiplier(n + 1);
            Hz_numer_polyn.Result = Hz_numer_factor;
            for (int i = 0; i < n - 1; i++) Hz_numer_polyn.MultiplyBy(Hz_numer_factor);
            Hz_numer_polyn.MultiplyBy(H0z);

            PolynomialMultiplier Hz_denom_polyn = new PolynomialMultiplier(n + 1);
            Complex[] Hz_denom_factor = new Complex[n + 1];
            for (int k = 0; k < n; k++)
            {
                Hz_denom_factor[0] = 1;
                Hz_denom_factor[1] = -(1 + pk[k]) / (1 - pk[k]);
                Hz_denom_polyn.MultiplyBy(Hz_denom_factor);
            }

            IIRCoefsB = new double[Hz_numer_polyn.Order];
            IIRCoefsA = new double[Hz_denom_polyn.Order];

            for (int i = 0; i < Hz_numer_polyn.Order; i++)
                IIRCoefsB[i] = Hz_numer_polyn.Result[i].Real;
            for (int i = 0; i < Hz_denom_polyn.Order; i++)
                IIRCoefsA[i] = Hz_denom_polyn.Result[i].Real;
        }

        /// <summary>
        /// Normalises frequency to units of omega_c.
        /// </summary>
        /// <returns>Normalised frequency.</returns>
        /// <param name="hz">Frequency in Hz.</param>
        public double NormaliseFrequency(double hz)
        {
            return Math.Tan(1 * Math.PI * hz / sr);
        }
    }

    /// <summary>
    /// Analytic multiplier of finite complex polynomial expressions.
    /// </summary>
    class PolynomialMultiplier
    {
        /// <summary>
        /// Gets or sets the order of the polynomial.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }
        /// <summary>
        /// The stored result register for reading or subsequent multiply calls.
        /// </summary>
        public Complex[] Result;

        /// <summary>
        /// The display varible of which Result is a polynomial. For polynomial string preview.
        /// </summary>
        private char displayVarible = 'z';
        /// <summary>
        /// The starting (lowest) exponent of the polynomial terms. For polynomial string preview.
        /// </summary>
        private int startIndex = -4;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CitySynth.PolynomialMultiplier"/> class.
        /// </summary>
        /// <param name="order">Order of polynomial.</param>
        public PolynomialMultiplier(int order)
        {
            Order = order;
            Result = new Complex[order];
            Result[0] = 1;
        }

        /// <summary>
        /// Multiplies the current stored polynomial by a complex number.
        /// </summary>
        /// <returns>New current polynomial.</returns>
        /// <param name="rhs">Complex multiplication factor.</param>
        public Complex[] MultiplyBy(Complex rhs)
        {
            for (int i = 0; i < Order; i++)
            {
                Result[i] *= rhs;
            }
            return Result;
        }

        /// <summary>
        /// Multiplies the current stored polynomial by another complex polynomial.
        /// </summary>
        /// <returns>New current polynomial.</returns>
        /// <param name="rhs">Complex polynomial by which to multiply.</param>
        public Complex[] MultiplyBy(Complex[] rhs)
        {
            Complex[] res = new Complex[Math.Max(rhs.Length, Order)];
            for (int lhsIndex = 0; lhsIndex < Order; lhsIndex++)
            {
                for (int rhsIndex = 0; rhsIndex < rhs.Length; rhsIndex++)
                {
                    Complex value = Result[lhsIndex] * rhs[rhsIndex];
                    if (value != Complex.Zero)
                    {
                        if (Result[lhsIndex].Real == rhs[rhsIndex].Real && rhs[rhsIndex].Imaginary != 0)
                        {

                        }
                        res[lhsIndex + rhsIndex] += value;
                    }
                }
            }
            Result = res;
            Order = Result.Length;
            return Result;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:CitySynth.PolynomialMultiplier"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:CitySynth.PolynomialMultiplier"/>.</returns>
        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < Order; i++)
            {
                if (Result[i] != Complex.Zero)
                {
                    float sign = Math.Sign(startIndex);
                    float mag = startIndex / sign;
                    res += String.Format("({0}+{1}i){2}^{3}", Result[i].Real, Result[i].Imaginary, displayVarible, i * sign);
                    if (i < Order - 1) res += " + ";
                }
            }
            return res;
        }
    }
}
