using System;
using System.Collections.Generic;
using System.Text;

namespace WindDataProcessing
{
    /// <summary>
    /// Soubor měnitelných vstupů
    /// </summary>
    public class CalculationParametersCollection
    {
        /// <summary>
        /// [N]
        /// </summary>
        public double FgShaft { get; set; }

        /// <summary>
        /// [N]
        /// </summary>
        public double FgGearbox { get; set; }

        /// <summary>
        /// [N]
        /// </summary>
        public double AxialPreload { get; set; }
        public double ShaftTiltAngle { get; set; }

        public BearingParametersColection FMB { get; set; }
        public BearingParametersColection RMB { get; set; }
        public double n { get; set; }
        public double StiffnesCoefficient_a { get; set; }
        public double StiffnesCoefficient_b { get; set; }
        public double StiffnesCoefficient_c { get; set; }
        public double StiffnesCoefficient_d { get; set; }
        public double StiffnesCoefficient_e { get; set; }
        public double StiffnesCoefficient_f { get; set; }
    }

    /// <summary>
    /// Soubor měnitelných vstupů vázaných na konkrétní ložisko
    /// </summary>
    public class BearingParametersColection
    {
        private double contactAngle;

        public double ContactAngle
        {
            get => contactAngle;
            set
            {
                contactAngle = value;
                Y1 = 1 / MV.MathOperation.Tand(value) * 0.4;
            }
        }

        public double Y1 { get; set; }
        public double Y2 { get; set; }
        public double e { get; set; }
        /// <summary>
        /// Počet valivých elementů
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Rameno, na kterém působí radiální reakce ložiska. Závislé na úhlu styku.
        /// </summary>
        public double Arm_a { get; set; }

        public double ForceGenerationCoef_a01 { get; set; }
        public double ForceGenerationCoef_a02 { get; set; }
    }
}