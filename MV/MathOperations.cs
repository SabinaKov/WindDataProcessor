using System;
using System.Collections.Generic;
using System.Text;

namespace MV
{
    public class MathOperations
    {
        internal static double AreaOfPartOfCircle(double radius, double delkaTetivy)
        {
            double alpha = 2 * Math.Asin(delkaTetivy / 2 / radius);
            double S = Math.Pow(radius, 2.0) / 2 * (alpha - Math.Sin(alpha));
            return S;
        }

        internal static double LogFrom(double baze, double argument)
        {
            return Math.Log(argument) / Math.Log(baze);
        }

        internal static double Cosd(double hodnotaVeStupnich)
        {
            return Math.Cos(hodnotaVeStupnich * Math.PI / 180);
        }

        internal static double Cotgd(double hodnotaVeStupnich)
        {
            return Cosd(hodnotaVeStupnich) / Sind(hodnotaVeStupnich);
        }

        internal static double Sind(double hodnotaVeStupnich)
        {
            return Math.Sin(hodnotaVeStupnich * Math.PI / 180);
        }

        internal static double Tand(double hodnotaVeStupnich)
        {
            return Math.Tan(hodnotaVeStupnich * Math.PI / 180);
        }

        internal static double Acosd(double hodnota)
        {
            return Math.Acos(hodnota) * 180 / Math.PI;
        }

        internal static double Atand(double hodnota)
        {
            return Math.Atan(hodnota) * 180 / Math.PI;
        }


        /// <summary>
        /// Trapezni integrace funkce
        /// </summary>
        /// <param name="f">Funkce F(x), která se má integrovat. Arg1:integrovaný parametr, Arg2: Array proměnných ve funkci, Arg3: výsledná hodnota</param>
        /// <param name="arrX">Proměnné ve funkci</param>
        /// <param name="lowLim">Dolní limit</param>
        /// <param name="topLim">Horní limit</param>
        /// <param name="accuracy">Přesnost</param>
        /// <returns></returns>
        internal static double IntegralTrapez(Func<double, double[], double> f, double[] arrX, double lowLim, double topLim, double accuracy)
        {
            if (accuracy <= 0.0)
            {
                throw new Exception("Accuracy must be possitive.");
            }
            double area = 0.0, areaLastStep, step;
            double e, a, b;
            int initStepNum = 100, loopCounter = 0, loopMax = 10;
            int stepRecalcLim = loopMax;
            while (loopCounter <= loopMax && stepRecalcLim > 0.0)
            {
                a = lowLim;
                step = (topLim - lowLim) / initStepNum;
                areaLastStep = area;
                area = 0.0;
                // Cyklus integrace:
                for (int i = 0; i < initStepNum; i++)
                {
                    b = a + step;
                    area += 0.5 * step * (f(a, arrX) + f(b, arrX));
                    a = b;
                }
                // Konec integračního cyklu:
                e = 100 * (Math.Abs(area - areaLastStep)) / area;
                // Přepočet kroku, pokud je přesnost příliš nízká:
                if (e > accuracy)
                {
                    stepRecalcLim--;
                    initStepNum = initStepNum * 2; // half step method
                }
                else
                {
                    stepRecalcLim = 0;
                }
                loopCounter++;
            }
            return area;
        }

        internal static double ConvertToRadians(double hodnotaVeStupnich)
        {
            return hodnotaVeStupnich / 180 * Math.PI;
        }

        /// <summary>
        /// Vypočítá vzdálenost dvou bodů ve 2D
        /// </summary>
        /// <param name="Ax"></param>
        /// <param name="Ay"></param>
        /// <param name="Bx"></param>
        /// <param name="By"></param>
        /// <returns></returns>
        internal static double PointsDistance(double Ax, double Ay, double Bx, double By)
        {
            return Math.Sqrt(Math.Pow(Ax - Bx, 2.0) + Math.Pow(Ay - By, 2.0));
        }
    }
}
