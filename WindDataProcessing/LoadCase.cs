using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    internal class LoadCase
    {
        /// <summary>
        /// Index load case dle posloupnosti nahrávání do RAM... Odpovídá posloupnosti v seznamu s Load Cases
        /// </summary>
        public int Position { get; internal set; }

        public int NumberOfLoadStates { get; internal set; }
        public double AverageSpeed { get; internal set; }
        internal string Name { get; set; }

        /// <summary>
        /// Rate, Četnost
        /// </summary>
        internal double TimeShare { get; set; }

        internal List<LoadState> LoadStates { get; set; }
        internal double FReqFMB { get; set; }
        internal double FAeqFMB { get; set; }
        internal double PeqFMB { get; set; }
        internal double FReqRMB { get; set; }
        internal double FAeqRMB { get; set; }
        internal double PeqRMB { get; set; }

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 1,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoFirstCondition { get; set; } = 0;

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 2,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoSecondCondition { get; set; } = 0;

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 3,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoThirdCondition { get; set; } = 0;

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 4,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoFourthCondition { get; set; } = 0;

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 5,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoFifthCondition { get; set; } = 0;

        /// <summary>
        /// Kolikrát v daném load case bylo zatížení load state takové, že byla splněna podmínka č. 6,
        /// která rozhoduje o způsobu výpočtu rozložení axiální reakce v ložiscích.
        /// </summary>
        internal int NoSixthCondition { get; set; } = 0;
    }
}