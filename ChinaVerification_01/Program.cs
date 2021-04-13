using System;
using System.Threading.Tasks;
using WindDataProcessing;

namespace ChinaVerification_01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the TXT file with Load Case Time Shares: ");
                // @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1.csv"
                string loadCasesTimeShareFilePath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ChinaVerification_01\DataTesting\TimeShares.csv";//Console.ReadLine();
                Console.WriteLine($"You set: {loadCasesTimeShareFilePath}");
                Console.WriteLine("Path to the Project Directory: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1"
                string projectDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ChinaVerification_01\DataTesting\PRJ";//Console.ReadLine();
                Console.WriteLine($"You set: {projectDirectoryPath}");
                Console.WriteLine("Path to the Directory where results will be saved: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData"
                string resultsDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ChinaVerification_01\DataTesting";//Console.ReadLine();
                Console.WriteLine($"You set: {resultsDirectoryPath}");
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath);
                dataProcessor.SourceDataType = Enums.SourceDataType.TXT;
                dataProcessor.SourceDataFirstLine = 2;
                dataProcessor.SourceDataColumn.FX = 5;
                dataProcessor.SourceDataColumn.FY = 6;
                dataProcessor.SourceDataColumn.FZ = 7;
                dataProcessor.SourceDataColumn.MY = 3;
                dataProcessor.SourceDataColumn.MZ = 4;
                dataProcessor.SourceDataColumn.Speed = 14;
                dataProcessor.ConvertSpeedMultiplyBy = 60.0 / (2 * Math.PI); // rad/s --> rev/min
                dataProcessor.NumberOfLevels = 144;
                await dataProcessor.Process();
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
