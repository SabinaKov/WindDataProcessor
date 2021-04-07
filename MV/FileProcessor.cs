using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MV
{
    public class FileProcessor
    {
        public static Dictionary<Tuple<int, int>, string> LoadDataFromFile_csvSemicolon(string filePath)
        {
            Dictionary<Tuple<int, int>, string> data = new Dictionary<Tuple<int, int>, string>();
            List<string> lines = File.ReadAllLines(filePath).ToList();
            int row = 0, column = 0;
            foreach (string line in lines)
            {
                string[] cells = line.Split(';');
                foreach (string cell in cells)
                {
                    data.Add(new Tuple<int, int>(row, column++), cell);
                }
                row++;
                column = 0;
            }

            return data;
        }
    }
}
