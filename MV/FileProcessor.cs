using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    }
}
