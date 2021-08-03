using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MV
{
    public class FileProcessor
    {
        public static Dictionary<Tuple<int, int>, string> LoadTableDataFromFile(string filePath, char separator)
        {
            Dictionary<Tuple<int, int>, string> data = new Dictionary<Tuple<int, int>, string>();
            List<string> lines = File.ReadAllLines(filePath).ToList();
            int row = 0, column = 0;
            foreach (string line in lines)
            {
                string[] cells = line.Split(separator);
                foreach (string cell in cells)
                {
                    data.Add(new Tuple<int, int>(row, column++), cell);
                }
                row++;
                column = 0;
            }

            return data;
        }

        public static Dictionary<Tuple<int, int>, string> LoadDataFromFile_csvSemicolon(string filePath)
        {
            return LoadTableDataFromFile(filePath, ';');
        }

        public static Dictionary<Tuple<int, int>, string> LoadDataFromFile_tableWithTabs(string filePath)
        {
            return LoadTableDataFromFile(filePath, '\t');
        }

        public static void ExportCSV(List<List<string>> exportTabulka, string pracovniAdresar, string nazevSouboru)
        {
            if (pracovniAdresar == null)
            {
                throw new Exception();
            }
            if (!Directory.Exists(pracovniAdresar))
            {
                throw new Exception();
            }
            nazevSouboru += ".csv";

            string cestaSouboru = pracovniAdresar + "\\" + nazevSouboru;
            StreamWriter streamWriter = new StreamWriter(cestaSouboru, false, Encoding.UTF8);
            for (int row = 0; row < exportTabulka[0].Count; row++)
            {
                string radek = "";
                for (int column = 0; column < exportTabulka.Count; column++)
                {
                    string bunka = exportTabulka[column][row];
                    radek += bunka;
                    if (column != exportTabulka.Count - 1)
                    {
                        radek += ";";
                    }
                }
                streamWriter.WriteLine(radek);
            }
            streamWriter.Close();
        }
    }
}