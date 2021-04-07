using System;

namespace WindDataProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string loadCasesTimeShareFilePath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData\PRJ1.csv";
                string projectDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData\PRJ1";
                string resultsDirectoryPath = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData";
                DataProcessor dataProcessor = new DataProcessor(loadCasesTimeShareFilePath, projectDirectoryPath, resultsDirectoryPath);
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
