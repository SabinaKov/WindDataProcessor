using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindDataProcessing
{
    public class AxialReactionCalculator
    {
        public AxialReactionCalculator(string stiffnessFMBDataFilePath, string stiffnessRMBDataFilePath)
        {
            _stiffnessCurvesFMB = new BearingStiffnessCurves(stiffnessFMBDataFilePath);
            _stiffnessCurvesRMB = new BearingStiffnessCurves(stiffnessRMBDataFilePath);
        }

        private BearingStiffnessCurves _stiffnessCurvesFMB;
        private BearingStiffnessCurves _stiffnessCurvesRMB;

        private StiffnessCurve _stiffnessCurveFMB;
        private StiffnessCurve _stiffnessCurveRMB;

        public double CaluclateFaFMB(double FrFMB, double FrRMB, double KA)
        {
            double displacement = CalculateDisplacement(FrFMB, FrRMB, KA);
            return _stiffnessCurveFMB.InterpolateFaFor(displacement);
        }

        public double CaluclateFaRMB(double FrFMB, double FrRMB, double KA)
        {
            double displacement = CalculateDisplacement(FrFMB, FrRMB, KA);
            return _stiffnessCurveRMB.InterpolateFaFor(displacement);
        }

        private double CalculateDisplacement(double FrFMB, double FrRMB, double KA)
        {
            _stiffnessCurveFMB = _stiffnessCurvesFMB.InterpolateStifnessCurveFor(FrFMB);
            _stiffnessCurveRMB = _stiffnessCurvesRMB.InterpolateStifnessCurveFor(FrRMB);
            List<StiffnessPoint> KAstiffnessCurve = CalculateKAstiffnessCurve(_stiffnessCurveFMB, _stiffnessCurveRMB);
            return InterpolateDisplacement(KAstiffnessCurve, KA);
        }

        private double InterpolateDisplacement(List<StiffnessPoint> KAstiffnessCurve, double KA)
        {
            List<double> KaData = new List<double>();
            List<double> displData = new List<double>();
            for (int i = 0; i < KAstiffnessCurve.Count; i++)
            {
                KaData.Add(KAstiffnessCurve[i].Fa);
                displData.Add(KAstiffnessCurve[i].Ua);
            }

            double displacement = MV.Algorithm.LinearInterpolationOfPoint(KA, KaData, displData);
            return displacement;
        }

        private List<StiffnessPoint> CalculateKAstiffnessCurve(StiffnessCurve stiffnessCurveFMB, StiffnessCurve stiffnessCurveRMB)
        {
            List<StiffnessPoint> KaStiffnessCurve = new List<StiffnessPoint>();
            for (int i = 0; i < stiffnessCurveFMB.StiffnessPoints.Count; i++)
            {
                double Ua = stiffnessCurveFMB.StiffnessPoints[i].Ua;
                var stifnessPointsRMB = stiffnessCurveRMB.StiffnessPoints;
                int posOfUaInRMB = stifnessPointsRMB.IndexOf(stifnessPointsRMB.Where(x => x.Ua == Ua).FirstOrDefault());
                double Ka = stiffnessCurveFMB.StiffnessPoints[i].Fa - stiffnessCurveRMB.StiffnessPoints[posOfUaInRMB].Fa;
                KaStiffnessCurve.Add(new StiffnessPoint { Ua = Ua, Fa = Ka });
            }
            return KaStiffnessCurve;
        }
    }

    internal class BearingStiffnessCurves
    {
        public BearingStiffnessCurves(string stiffnessDataFilePath)
        {
            LoadStiffnessCurves(stiffnessDataFilePath);
        }

        internal List<StiffnessCurve> StiffnessCurves { get; set; }

        internal StiffnessCurve InterpolateStifnessCurveFor(double Fr)
        {
            List<List<double>> FaData = new List<List<double>>();
            List<double> FrData = new List<double>();
            List<double> UaData = new List<double>();
            foreach (StiffnessCurve stiffnessCurve in StiffnessCurves)
            {
                FrData.Add(stiffnessCurve.Fr);
                FaData.Add(new List<double>());
                foreach (StiffnessPoint stiffnessPoint1 in stiffnessCurve.StiffnessPoints)
                {
                    FaData.LastOrDefault().Add(stiffnessPoint1.Fa);
                }
            }
            foreach (StiffnessPoint stiffnessPoint2 in StiffnessCurves.FirstOrDefault().StiffnessPoints)
            {
                UaData.Add(stiffnessPoint2.Ua);
            }
            List<double> FaInterpolated = MV.Algorithm.LinearInterpolationOfCurve(Fr, FrData, FaData);
            StiffnessCurve interpolatedStiffnessCurve = new StiffnessCurve { Fr = Fr, StiffnessPoints = new List<StiffnessPoint>() };
            for (int i = 0; i < FaInterpolated.Count; i++)
            {
                interpolatedStiffnessCurve.StiffnessPoints.Add(new StiffnessPoint { Ua = UaData[i], Fa = FaInterpolated[i] });
            }
            return interpolatedStiffnessCurve;
        }

        private void LoadStiffnessCurves(string stiffnessDataFilePath)
        {
            StiffnessCurves = new List<StiffnessCurve>();
            Dictionary<Tuple<int, int>, string> stringData = MV.FileProcessor.LoadDataFromFile_csvSemicolon(stiffnessDataFilePath);
            int lastRow = stringData.Select(_ => _.Key.Item1).Max() + 1;
            double lastFr = -1.0;
            const int startingRow = 1;
            for (int r = startingRow; r < lastRow; r++)
            {
                double Fr = Convert.ToDouble(stringData[new Tuple<int, int>(r, 1)]);
                if (Fr != lastFr)
                {
                    StiffnessCurves.Add(new StiffnessCurve { Fr = Fr, StiffnessPoints = new List<StiffnessPoint>() });
                }
                double Ua = Convert.ToDouble(stringData[new Tuple<int, int>(r, 0)]);
                double Fa = Convert.ToDouble(stringData[new Tuple<int, int>(r, 2)]);
                StiffnessCurves.LastOrDefault().StiffnessPoints.Add(new StiffnessPoint { Ua = Ua, Fa = Fa });
                lastFr = Fr;
            }
        }
    }

    internal class StiffnessCurve
    {
        internal double Fr { get; set; }
        internal List<StiffnessPoint> StiffnessPoints { get; set; }

        internal double InterpolateFaFor(double displacement)
        {
            List<double> displacements = new List<double>();
            List<double> forces = new List<double>();
            for (int i = 0; i < StiffnessPoints.Count; i++)
            {
                displacements.Add(StiffnessPoints[i].Ua);
                forces.Add(StiffnessPoints[i].Fa);
            }
            double FA = MV.Algorithm.LinearInterpolationOfPoint(displacement, displacements, forces);
            return FA;
        }
    }

    internal class StiffnessPoint
    {
        internal double Fa { get; set; }

        /// <summary>
        /// Axial deflection
        /// </summary>
        internal double Ua { get; set; }
    }
}