using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    public class DataProcessor
    {
        public DataProcessor(string loadCaseTimeShareFile, string projectDirectory, string resultsDirectory)
        {
            LoadCaseTimeShareFile = loadCaseTimeShareFile;
            ProjectDirectory = projectDirectory;
            ResultsDirectory = resultsDirectory;
        }

        public string LoadCaseTimeShareFile { get; }
        public string ProjectDirectory { get; }
        public string ResultsDirectory { get; }

        public void Process()
        {
            List<LoadCase> loadCases = LoadData();
        }

        private List<LoadCase> LoadData()
        {
            List<List<string>> dataInTimeShareFile = 
        }
    }
}
