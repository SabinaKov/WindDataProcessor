using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MV
{
    public static class Algorithm
    {
        /// <summary>
        /// Bisection Method, Interval Halving Method, Binary Search Method, Dichotomy Method
        /// According to English Wikipedia
        /// </summary>
        /// <param name="function">Function which aim is equality to zero.</param>
        /// <param name="lowerEndPoint">lower endpoint</param>
        /// <param name="higherEndpoint">higher endpoint</param>
        /// <param name="tolerance"></param>
        /// <param name="maxIterations"></param>
        /// <returns>Result</returns>
        public static double MetodaPuleniIntervalu(Func<double, double> function, List<object> list, double lowerEndPoint, double higherEndpoint, double tolerance, int maxIterations)
        {
            if (lowerEndPoint >= higherEndpoint)
            {
                throw new Exception("Lower end point must be smaller than Higher end point.");
            }
            double f_a = function(lowerEndPoint);
            double f_b = function(higherEndpoint);
            if (!(f_a < 0 && f_b > 0 || f_a > 0 && f_b < 0))
            {
                throw new Exception("Bisection Method has no solution.");
            }
            int n = 1;
            double a = lowerEndPoint;
            double b = higherEndpoint;
            while (n <= maxIterations)
            {
                double c = (a + b) / 2.0;
                double f_c = function(c);
                if (f_c == 0 || (b - a) / 2 < tolerance)
                {
                    return c;
                }
                n++;
                if (Math.Sign(f_c) == Math.Sign(f_a))
                {
                    a = c;
                }
                else
                {
                    b = c;
                }
            }
            throw new Exception("Max number of iterations of Bisection Method was achieved.");
        }

        /// <summary>
        /// Bisection Method, Interval Halving Method, Binary Search Method, Dichotomy Method
        /// According to English Wikipedia
        /// Parameters can be set.
        /// </summary>
        /// <param name="function">Function which aim is equality to zero.</param>
        /// <param name="lowerEndPoint">lower endpoint</param>
        /// <param name="higherEndpoint">higher endpoint</param>
        /// <param name="tolerance"></param>
        /// <param name="maxIterations"></param>
        /// <returns>Result</returns>
        public static double MetodaPuleniIntervalu(Func<double, List<object>, double> function, List<object> parameters, double lowerEndPoint, double higherEndpoint, double tolerance, int maxIterations)
        {
            if (lowerEndPoint >= higherEndpoint)
            {
                throw new Exception("Lower end point must be smaller than Higher end point.");
            }
            double f_a = function(lowerEndPoint, parameters);
            double f_b = function(higherEndpoint, parameters);
            if (!(f_a < 0 && f_b > 0 || f_a > 0 && f_b < 0))
            {
                throw new Exception("Bisection Method has no solution.");
            }
            int n = 1;
            double a = lowerEndPoint;
            double b = higherEndpoint;
            while (n <= maxIterations)
            {
                double c = (a + b) / 2.0;
                double f_c = function(c, parameters);
                if (f_c == 0 || (b - a) / 2 < tolerance)
                {
                    return c;
                }
                n++;
                if (Math.Sign(f_c) == Math.Sign(f_a))
                {
                    a = c;
                }
                else
                {
                    b = c;
                }
            }
            throw new Exception("Max number of iterations of Bisection Method was achieved.");
        }

        public static double LinearniInterpolace(double x0, double y0, double x1, double y1, double x)
        {
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }
    }
}