using System;
using WindDataProcessing;

namespace GAMESA_01
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string loadCasesTimeShareFilePath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1.csv";
                string projectDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData\PRJ1";
                string resultsDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\WindDataProcessing\TestovaciData";
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath);
                dataProcessor.SourceDataType = Enums.SourceDataType.CSV;
                dataProcessor.SourceDataFirstLine = 19;
                dataProcessor.SourceDataColumn.FX = 2;
                dataProcessor.SourceDataColumn.FY = 3;
                dataProcessor.SourceDataColumn.FZ = 5;
                dataProcessor.SourceDataColumn.MY = 7;
                dataProcessor.SourceDataColumn.MZ = 9;
                dataProcessor.NumberOfLevels = 144;
                dataProcessor.Process();
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
