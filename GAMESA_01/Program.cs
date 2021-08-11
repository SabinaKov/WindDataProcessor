using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindDataProcessing;

namespace GAMESA_01
{
    internal class Program
    {
        /// <summary>
        /// Nastavení zátěžných stavů pro processing.
        /// 1. string tuplu - cesta k souboru se seznamem LoadCase (1. sloupec) a jejich četnosti (2. sloupec) - CSV soubor oddělený středníky.
        /// 2. string tuplu - adresář, ve kterém jsou uloženy data se zátěžnými stavy (LTS).
        /// 3. string tuplu - adresář, kam se mají uložit výsledky.
        /// </summary>
        internal static Dictionary<int, Tuple<string, string, string>> pathSettings = new Dictionary<int, Tuple<string, string, string>>
                {
                    { 1, new Tuple<string, string, string>
                        (
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData\SG5x_LDD.csv",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData\SG5x_LDD",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData"
                        )},
                    { 2, new Tuple<string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\testRates.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\Test",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076"
                        )},
                    { 3, new Tuple<string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\PRJ-6076_rates.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\PRJ-6076_TIMESERIES",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076"
                        )},
                    { 4, new Tuple<string, string, string>
                        (
                            @"\\brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák\Vybrané LC redukované pro Miru\list.csv",
                            @"\\brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák\Vybrané LC redukované pro Miru\LTS",
                            @"\\brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák\Vybrané LC redukované pro Miru"
                        )},
                    { 5, new Tuple<string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6144\PRJ-6144_rates.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6144\PRJ-6144_TIMESERIES",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6144"
                        )}
                };

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the CSV file with Load Case Time Shares: ");
                const int choosedSettings = 5;
                string loadCasesTimeShareFilePath = pathSettings[choosedSettings].Item1;
                Console.WriteLine($"You set: {loadCasesTimeShareFilePath}");
                Console.WriteLine("Path to the Project Directory: ");
                string projectDirectoryPath = pathSettings[choosedSettings].Item2;
                Console.WriteLine($"You set: {projectDirectoryPath}");
                Console.WriteLine("Path to the Directory where results will be saved: ");
                string resultsDirectoryPath = pathSettings[choosedSettings].Item3;
                Console.WriteLine($"You set: {resultsDirectoryPath}");
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath)
                {
                    SourceDataType = Enums.SourceDataType.CSV,
                    SourceDataFirstLine = 19,
                };
                dataProcessor.SourceDataColumn.FX = 2;//2
                dataProcessor.SourceDataColumn.FY = 3;//3
                dataProcessor.SourceDataColumn.FZ = 5;//5
                dataProcessor.SourceDataColumn.MY = 7;//7
                dataProcessor.SourceDataColumn.MZ = 9;//9
                dataProcessor.SourceDataColumn.Speed = 11;//11
                dataProcessor.NumberOfLevels = 144;

                dataProcessor.CP = new CalculationParametersCollection()
                {
                    FgShaft = 243778.5,
                    FgGearbox = 451260,
                    AxialPreload = 500000,
                    n = 10.0 / 3.0,
                    StiffnesCoefficient_a = 15534288.2,
                    StiffnesCoefficient_b = 7232282.1,
                    StiffnesCoefficient_c = 515472.2,
                    StiffnesCoefficient_d = 7652455.4,
                    StiffnesCoefficient_e = -4633033.7,
                    StiffnesCoefficient_f = 495386.1,
                    FMB = new BearingParametersColection()
                    {
                        ContactAngle = 19,
                        Z = 52,
                        Arm_a = 361.6
                    },
                    RMB = new BearingParametersColection()
                    {
                        ContactAngle = 14,
                        Z = 62,
                        Arm_a = 249.85
                    }
                };

                await dataProcessor.BearingReactions();
                //await dataProcessor.LDDlifesTester(); //- Vývoj
                //await dataProcessor.Process(); - vytvoří LDD
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.Write(ex.StackTrace);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}