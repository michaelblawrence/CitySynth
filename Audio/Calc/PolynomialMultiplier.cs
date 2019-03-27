using System;
using System.Numerics;

namespace CitySynth.Audio.Calc
{
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
