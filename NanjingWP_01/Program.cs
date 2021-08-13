using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindDataProcessing;

namespace NanjingWP_01
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
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Zadání\working condition schedule.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Zadání\主轴承LTS\Complet",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Výpočet"
                        )},
                    { 2, new Tuple<string, string, string>
                        (
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Zadání\test.csv",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Zadání\test",
                            @"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Cina\wind_power_CHINA\Nanjing_WP\3.xMW - TRBs\Zadání"
                        )}
                };

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the TXT file with Load Case Time Shares: ");
                const int choosedSettings = 1;
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
                    SourceDataType = Enums.SourceDataType.TXT,
                    SourceDataFirstLine = 2,
                };
                dataProcessor.SourceDataColumn.FX = 6;
                dataProcessor.SourceDataColumn.FY = 7;
                dataProcessor.SourceDataColumn.FZ = 8;
                dataProcessor.SourceDataColumn.MY = 3;
                dataProcessor.SourceDataColumn.MZ = 4;
                dataProcessor.SourceDataColumn.Speed = 10;
                dataProcessor.NumberOfLevels = 144;

                dataProcessor.CP = new CalculationParametersCollection()
                {
                    FgShaft = 127530,
                    FgGearbox = 255060,
                    AxialPreload = 500000,
                    n = 10.0 / 3.0,
                    StiffnesCoefficient_a = 19178361.4,
                    StiffnesCoefficient_b = 7178891.2,
                    StiffnesCoefficient_c = 502272.7,
                    StiffnesCoefficient_d = 19178361.4,
                    StiffnesCoefficient_e = -7178891.2,
                    StiffnesCoefficient_f = 502272.7,
                    FMB = new BearingParametersColection()
                    {
                        ContactAngle = 18,
                        Z = 58,
                        Arm_a = 304.3
                    },
                    RMB = new BearingParametersColection()
                    {
                        ContactAngle = 18,
                        Z = 58,
                        Arm_a = 304.3
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