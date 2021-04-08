using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindDataProcessing
{
    public class DataProcessor
    {
        public DataProcessor(string loadCasesTimeShareFilePath, string projectDirectoryPath, string resultsDirectoryPath)
        {
            LoadCasesTimeShareFilePath = loadCasesTimeShareFilePath;
            ProjectDirectoryPath = projectDirectoryPath;
            ResultsDirectoryPath = resultsDirectoryPath;
            SourceDataColumn = new SourceDataColumnPosition();
        }

        public string LoadCasesTimeShareFilePath { get; }
        public string ProjectDirectoryPath { get; }
        public string ResultsDirectoryPath { get; }
        public Enums.SourceDataType SourceDataType { get; set; }
        public int SourceDataFirstLine { get; set; }
        public SourceDataColumnPosition SourceDataColumn { get; set; }
        public int NumberOfLevels { get; set; }

        public async Task Process()
        {
            List<LoadCase> loadCasesWithoutLoadStates = LoadLoadCaseData();
            List<LoadCase> loadCases = PopulateLoadCasesWithLoadStates(loadCasesWithoutLoadStates);
            (Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes) = FindMinsAndMaxes(loadCases);
            Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels = DefineLoadStateLevels(loadMins, loadMaxes);
            PopulateLoadStateLevelsByTimeShares(loadStateLevels, loadCases);
            await SaveExcelFile(loadStateLevels);
        }

        private List<LoadCase> LoadLoadCaseData()
        {
            Dictionary<Tuple<int,int>, string> dataInTimeShareFile = MV.FileProcessor.LoadDataFromFile_csvSemicolon(LoadCasesTimeShareFilePath);
            List<LoadCase> loadCases = new List<LoadCase>();
            foreach (KeyValuePair<Tuple<int,int>,string> timeShareItem in dataInTimeShareFile)
            {
                if (timeShareItem.Key.Item2 == 0)
                {
                    loadCases.Add(new LoadCase
                    {
                        Name = timeShareItem.Value
                    });
                }
                else if (timeShareItem.Key.Item2 == 1)
                {
                    double timeShare = Convert.ToDouble(timeShareItem.Value);
                    loadCases.LastOrDefault().TimeShare = timeShare;
                }
            }
            return loadCases;
        }
        private List<LoadCase> PopulateLoadCasesWithLoadStates(List<LoadCase> loadCasesWithoutLoadStates)
        {
            List<LoadCase> loadCases = new List<LoadCase>();
            switch (SourceDataType)
            {
                case Enums.SourceDataType.CSV:
                    {
                        foreach (LoadCase loadCase in loadCasesWithoutLoadStates)
                        {
                            string loadCaseFilePath = ProjectDirectoryPath + @"\" + loadCase.Name + ".csv";
                            Dictionary<Tuple<int, int>, string> loadStateData = MV.FileProcessor.LoadDataFromFile_csvSemicolon(loadCaseFilePath);
                            List<LoadState> loadStates = new List<LoadState>();
                            int lastRow = loadStateData.Select(_ => _.Key.Item1).Max();
                            loadCase.NumberOfLoadStates = lastRow;
                            for (int row = SourceDataFirstLine-1; row < lastRow; row++)
                            {
                                loadStates.Add(new LoadState
                                {
                                    FX = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FX)]),
                                    FY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FY)]),
                                    FZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FZ)]),
                                    MY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MY)]),
                                    MZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MZ)])
                                });
                            }
                            loadCase.LoadStates = loadStates;
                            loadCases.Add(loadCase);
                        }

                        break;
                    }
                default:
                    throw new Exception("Not set SourceDataType");
            }
            return loadCases;
        }

        private (Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes) FindMinsAndMaxes(List<LoadCase> loadCases)
        {
            Dictionary<Enums.LoadStateType, double> loadMins = new Dictionary<Enums.LoadStateType, double>();
            loadMins.Add(Enums.LoadStateType.FX, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FX)).Min());
            Console.WriteLine($"FX min: {loadMins[Enums.LoadStateType.FX]}");
            loadMins.Add(Enums.LoadStateType.FY, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FY)).Min());
            Console.WriteLine($"FY min: {loadMins[Enums.LoadStateType.FY]}");
            loadMins.Add(Enums.LoadStateType.FZ, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FZ)).Min());
            Console.WriteLine($"FZ min: {loadMins[Enums.LoadStateType.FZ]}");
            loadMins.Add(Enums.LoadStateType.MY, loadCases.SelectMany(x => x.LoadStates.Select(y => y.MY)).Min());
            Console.WriteLine($"MY min: {loadMins[Enums.LoadStateType.MY]}");
            loadMins.Add(Enums.LoadStateType.MZ, loadCases.SelectMany(x => x.LoadStates.Select(y => y.MZ)).Min());
            Console.WriteLine($"MZ min: {loadMins[Enums.LoadStateType.MZ]}");
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            Dictionary<Enums.LoadStateType, double> loadMaxes = new Dictionary<Enums.LoadStateType, double>();
            loadMaxes.Add(Enums.LoadStateType.FX, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FX)).Max());
            Console.WriteLine($"FX max: {loadMaxes[Enums.LoadStateType.FX]}");
            loadMaxes.Add(Enums.LoadStateType.FY, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FY)).Max());
            Console.WriteLine($"FY max: {loadMaxes[Enums.LoadStateType.FY]}");
            loadMaxes.Add(Enums.LoadStateType.FZ, loadCases.SelectMany(x => x.LoadStates.Select(y => y.FZ)).Max());
            Console.WriteLine($"FZ max: {loadMaxes[Enums.LoadStateType.FZ]}");
            loadMaxes.Add(Enums.LoadStateType.MY, loadCases.SelectMany(x => x.LoadStates.Select(y => y.MY)).Max());
            Console.WriteLine($"MY max: {loadMaxes[Enums.LoadStateType.MY]}");
            loadMaxes.Add(Enums.LoadStateType.MZ, loadCases.SelectMany(x => x.LoadStates.Select(y => y.MZ)).Max());
            Console.WriteLine($"MZ max: {loadMaxes[Enums.LoadStateType.MZ]}");
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            return (loadMins, loadMaxes);
        }
        private Dictionary<Enums.LoadStateType, List<Level>> DefineLoadStateLevels(Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes)
        {
            Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels = new Dictionary<Enums.LoadStateType, List<Level>>();
            foreach (Enums.LoadStateType loadStateType in (Enums.LoadStateType[])Enum.GetValues(typeof(Enums.LoadStateType)))
            {
                List<Level> levels = new List<Level>();
                double min = loadMins[loadStateType];
                double max = loadMaxes[loadStateType] + 0.1; // Becouse of populating with time shares.
                double range = (max - min) / NumberOfLevels;
                for (int l = 0; l < NumberOfLevels; l++)
                {
                    Level level = new Level();
                    level.Min = min + l * range;
                    level.Mean = level.Min + range / 2.0;
                    level.Max = level.Min + range;
                    levels.Add(level);
                }
                loadStateLevels.Add(loadStateType, levels);
            }
            Console.WriteLine("Load state levels defined.");
            return loadStateLevels;
        }
        private void PopulateLoadStateLevelsByTimeShares(Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels, List<LoadCase> loadCases)
        {
            foreach (Enums.LoadStateType loadStateType in (Enums.LoadStateType[])Enum.GetValues(typeof(Enums.LoadStateType)))
            {
                foreach (LoadCase loadCase in loadCases)
                {
                    foreach (LoadState loadState in loadCase.LoadStates)
                    {
                        double loadStateValue;
                        switch (loadStateType)
                        {
                            case Enums.LoadStateType.FX:
                                loadStateValue = loadState.FX;
                                break;
                            case Enums.LoadStateType.FY:
                                loadStateValue = loadState.FY;
                                break;
                            case Enums.LoadStateType.FZ:
                                loadStateValue = loadState.FZ;
                                break;
                            case Enums.LoadStateType.MY:
                                loadStateValue = loadState.MY;
                                break;
                            case Enums.LoadStateType.MZ:
                                loadStateValue = loadState.MZ;
                                break;
                            default:
                                throw new Exception("Wrong loadStateType");
                        }
                        loadStateLevels[loadStateType].Where(level => loadStateValue >= level.Min && loadStateValue < level.Max)
                                                      .ToList()
                                                      .ForEach(x => x.TimeShare += loadCase.TimeShare / loadCase.NumberOfLoadStates);
                    }
                }
                Console.WriteLine($"Time Shares of Load State Type {loadStateType} populated.");
                Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            }
            UnityValidation(loadStateLevels);
        }

        private void UnityValidation(Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels)
        {
            foreach (KeyValuePair<Enums.LoadStateType, List<Level>> levels in loadStateLevels)
            {
                double timeShare = 0;
                foreach (Level level in levels.Value)
                {
                    timeShare += level.TimeShare;
                }
                if (timeShare < 0.99 || timeShare > 1.01)
                {
                    throw new Exception($"Sum of Time Shares at Load State Type: {levels.Key} is not equal to 1, but is {timeShare}.");
                }
            }
        }
        private async Task SaveExcelFile(Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo file = new FileInfo(ResultsDirectoryPath + @"\Levels.xlsx");
            DeleteIfExists(file);
            using var package = new ExcelPackage(file);
            var ws = package.Workbook.Worksheets.Add("Levels");
            int i = 0;
            foreach (KeyValuePair<Enums.LoadStateType, List<Level>> loadStateLevel in loadStateLevels)
            {
                int baseColumn = 2 + i * 6;
                var header = ws.Cells[2, baseColumn].Value = loadStateLevel.Key;
                var range = ws.Cells[3, baseColumn].LoadFromCollection(loadStateLevels[loadStateLevel.Key], true);
                range.AutoFitColumns();
                i++;
            }
            await package.SaveAsync();
            Console.WriteLine("Excel file with Levels and their Time Shares was saved to the Results Directory:");
            Console.WriteLine(file.DirectoryName);
        }

        private void DeleteIfExists(FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}
