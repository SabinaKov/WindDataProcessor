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

        private double InterpolateDisplacement(List<StiffnessPoint> kAstiffnessCurve, double kA)
        {
            throw new NotImplementedException();
        }

        private List<StiffnessPoint> CalculateKAstiffnessCurve(StiffnessCurve stiffnessCurveFMB, StiffnessCurve stiffnessCurveRMB)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private void LoadStiffnessCurves(string stiffnessDataFilePath)
        {
            StiffnessCurves = new List<StiffnessCurve>();
            Dictionary<Tuple<int, int>, string> stringData = MV.FileProcessor.LoadDataFromFile_csvSemicolon(stiffnessDataFilePath);
            int lastRow = stringData.Select(_ => _.Key.Item1).Max();
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
            throw new NotImplementedException();
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