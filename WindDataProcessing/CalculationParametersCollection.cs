using System;
using System.Collections.Generic;
using System.Text;

namespace WindDataProcessing
{
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
        public double Z { get; set; }
        public double Arm_a { get; set; }
    }
}