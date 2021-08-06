using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindDataProcessing;

namespace GAMESA_01
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the CSV file with Load Case Time Shares: ");
                Dictionary<int, Tuple<string, string, string>> pathSettings = new Dictionary<int, Tuple<string, string, string>>
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
                        )}
                };
                const int choosedSettings = 3;
                // @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1.csv"
                //@"\\Brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák\Vybrane_LC.csv";
                //@"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData\SG5x_LDD.csv";
                //Console.ReadLine();
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\Data_04_05_2021\OneDrive_2021-05-04\rates.csv";
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\testRates.csv";
                string loadCasesTimeShareFilePath = pathSettings[choosedSettings].Item1;
                Console.WriteLine($"You set: {loadCasesTimeShareFilePath}");
                Console.WriteLine("Path to the Project Directory: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1"
                //@"\\Brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák\Vybrané_LC";
                //@"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData\SG5x_LDD";
                //Console.ReadLine();
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\Data_04_05_2021\OneDrive_2021-05-04\SG5x.R2.1.PRJ-6076_MULTIPLE.INTERPOLATED_TIMESERIES";
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076\Test";
                string projectDirectoryPath = pathSettings[choosedSettings].Item2;
                Console.WriteLine($"You set: {projectDirectoryPath}");
                Console.WriteLine("Path to the Directory where results will be saved: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData"
                //@"\\Brn-fs-01\data _zkl\Data\ZKL VaV\Exchange\Novák";
                //@"C:\Users\miroslav.vaculka\source\repos\Mirabass\WindDataProcessor\WindDataProcessing\TestovaciData";
                //Console.ReadLine();
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\Data_04_05_2021\OneDrive_2021-05-04";
                //@"\\brn-fs-01\DATA _ZKL\Data\ZKL VaV\ZKL_dokumenty\PROJEKTY\Spanelsko\Gamesa - loziska hlavniho hridele\SG5x\OneDrive_2020-04-17\PRJ-6076";
                string resultsDirectoryPath = pathSettings[choosedSettings].Item3;
                Console.WriteLine($"You set: {resultsDirectoryPath}");
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath);
                dataProcessor.SourceDataType = Enums.SourceDataType.CSV;
                dataProcessor.SourceDataFirstLine = 19;
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
                    AxialPreload = 500000, //,500000
                    n = 10.0 / 3.0,
                    FMB = new BearingParametersColection()
                    {
                        ContactAngle = 19,
                        Z = 52
                    },
                    RMB = new BearingParametersColection()
                    {
                        ContactAngle = 11,
                        Z = 62
                    }
                };

                await dataProcessor.LDDlifesTester();
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