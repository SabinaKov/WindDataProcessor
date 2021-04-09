using System;
using System.Threading.Tasks;
using WindDataProcessing;

namespace GAMESA_01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Path to the CSV file with Load Case Time Shares: ");
                // @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1.csv"
                string loadCasesTimeShareFilePath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1.csv";//Console.ReadLine();
                Console.WriteLine($"You set: {loadCasesTimeShareFilePath}");
                Console.WriteLine("Path to the Project Directory: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1"
                string projectDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1";//Console.ReadLine();
                Console.WriteLine($"You set: {projectDirectoryPath}");
                Console.WriteLine("Path to the Directory where results will be saved: ");
                //@"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData"
                string resultsDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData";//Console.ReadLine();
                Console.WriteLine($"You set: {resultsDirectoryPath}");
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath);
                dataProcessor.SourceDataType = Enums.SourceDataType.CSV;
                dataProcessor.SourceDataFirstLine = 19;
                dataProcessor.SourceDataColumn.FX = 2;
                dataProcessor.SourceDataColumn.FY = 3;
                dataProcessor.SourceDataColumn.FZ = 5;
                dataProcessor.SourceDataColumn.MY = 7;
                dataProcessor.SourceDataColumn.MZ = 9;
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
