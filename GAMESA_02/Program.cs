using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindDataProcessing;

namespace GAMESA_02
{
    internal class Program
    {
        /// <summary>
        /// Nastavení zátěžných stavů pro processing.
        /// 1. string tuplu - cesta k souboru se seznamem LoadCase (1. sloupec) a jejich četnosti (2. sloupec) - CSV soubor oddělený středníky.
        /// 2. string tuplu - adresář, ve kterém jsou uloženy data se zátěžnými stavy (LTS).
        /// 3. string tuplu - adresář, kam se mají uložit výsledky.
        /// /// 4. string tuplu - cesta k souboru s tuhostmi
        /// </summary>
        internal static Dictionary<int, Tuple<string, string, string, string, string>> pathSettings = new Dictionary<int, Tuple<string, string, string, string, string>>
                {
                    { 1, new Tuple<string, string, string, string, string>
                        (
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test\gf.csv",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test\data",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test",
                            @"",
                            @""
                        )},
                    { 2, new Tuple<string, string, string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\komplexní výpočet\CALC\PRJ-0899.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\Gamesa zadání\fatigue SAR-0899-FY21",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\komplexní výpočet\CALC",
                            @"",
                            @""
                        )},
                    { 3, new Tuple<string, string, string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\komplexní výpočet\CALC\PRJ-0907.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\Gamesa zadání\fatigue SAR-0907-FY21",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG3.X CALULATION OF BEARINGS AGAINST 2 WINDFARM FATIGUE LOADS\komplexní výpočet\CALC",
                            @"",
                            @""
                        )},
                    { 4, new Tuple<string, string, string, string, string>
                        (
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test\Verifikace\ver.csv",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test\Verifikace\data",
                            @"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\GAMESA_02\Test\Verifikace",
                            @"",
                            @""
                        )}
                };

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the CSV file with Load Case Time Shares: ");
                const int choosedSettings = 3;
                string loadCasesTimeShareFilePath = pathSettings[choosedSettings].Item1;
                Console.WriteLine($"You set: {loadCasesTimeShareFilePath}");
                Console.WriteLine("Path to the Project Directory: ");
                string projectDirectoryPath = pathSettings[choosedSettings].Item2;
                Console.WriteLine($"You set: {projectDirectoryPath}");
                Console.WriteLine("Path to the Directory where results will be saved: ");
                string resultsDirectoryPath = pathSettings[choosedSettings].Item3;
                Console.WriteLine($"You set: {resultsDirectoryPath}");
                string stifnessesFMBFilePath = pathSettings[choosedSettings].Item4;
                string stifnessesRMBFilePath = pathSettings[choosedSettings].Item5;
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath, stifnessesFMBFilePath, stifnessesRMBFilePath)
                {
                    SourceDataType = Enums.SourceDataType.TXT,
                    SourceDataFirstLine = 3,
                };
                dataProcessor.SourceDataColumn.FX = 3;//2
                dataProcessor.SourceDataColumn.FY = 4;//3
                dataProcessor.SourceDataColumn.FZ = 5;//5
                dataProcessor.SourceDataColumn.MX = 6;//7
                dataProcessor.SourceDataColumn.MY = 7;//9
                dataProcessor.SourceDataColumn.MZ = 8;//9
                dataProcessor.SourceDataColumn.Speed = 2;//11
                dataProcessor.NumberOfLevels = 144;

                dataProcessor.CP = new CalculationParametersCollection()
                {
                    FgShaft = 128737,
                    FgGearbox = 264870,
                    AxialPreload = 0,
                    n = 10.0 / 3.0,
                    ShaftTiltAngle = 0.0,
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
                        Arm_a = 361.6,
                        Y1 = 3.4,
                        Y2 = 5.1,
                        e = 0.2
                    },
                    RMB = new BearingParametersColection()
                    {
                        ContactAngle = 14,
                        Z = 62,
                        Arm_a = 249.85,
                        Y1 = 2.4,
                        Y2 = 3.6,
                        e = 0.28
                    }
                };
                //await dataProcessor.FindMinMaxRadialReactions();
                //await dataProcessor.BearingReactions();
                await dataProcessor.SphericalBearingsReactions();
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
