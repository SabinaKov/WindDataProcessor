using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZpracovaniDat
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string loadCaseTimeShareFile = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData\PRJ1.csv";
                string projectDirectory = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData\PRJ1";
                string resultsDirectory = @"C:\Users\Mirek\source\repos\ZpracovaniDat\ZpracovaniDat\TestovaciData\TestovaciData";
                DataProcessor dataProcessor = new DataProcessor(loadCaseTimeShareFile, projectDirectory, resultsDirectory);
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
