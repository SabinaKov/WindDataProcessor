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
        public double ConvertSpeedMultiplyBy { get; set; } = 1.0;
        public CalculationParametersCollection CP { get; set; }

        public async Task LDDlifesTester()
        {
            Console.WriteLine("Process started!");
            List<LoadCase> loadCasesWithoutLoadStates = LoadLoadCaseData();
            List<LoadCase> loadCases = await PopulateLoadCasesWithLoadStatesAsync(loadCasesWithoutLoadStates);
            await CalculateLoadCaseAverageSpeeds(loadCases);
            await CalculateRadialReactions(loadCases);
            await CalculateAxialReactions(loadCases);
        }

        public async Task Process()
        {
            Console.WriteLine("Process started!");
            List<LoadCase> loadCasesWithoutLoadStates = LoadLoadCaseData();
            List<LoadCase> loadCases = await PopulateLoadCasesWithLoadStatesAsync(loadCasesWithoutLoadStates);
            await CalculateLoadCaseAverageSpeeds(loadCases);
            (Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes) = await FindMinsAndMaxes(loadCases);
            Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels = DefineLoadStateLevels(loadMins, loadMaxes);
            await PopulateLoadStateLevelsByTimeAndRevSharesAsync(loadStateLevels, loadCases);
            await SaveExcelFile(loadStateLevels);
        }

        private List<LoadCase> LoadLoadCaseData()
        {
            Dictionary<Tuple<int, int>, string> dataInTimeShareFile = MV.FileProcessor.LoadDataFromFile_csvSemicolon(LoadCasesTimeShareFilePath);
            List<LoadCase> loadCases = new List<LoadCase>();
            foreach (KeyValuePair<Tuple<int, int>, string> timeShareItem in dataInTimeShareFile)
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

        private async Task<List<LoadCase>> PopulateLoadCasesWithLoadStatesAsync(List<LoadCase> loadCasesWithoutLoadStates)
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
                            int lastRow = loadStateData.Select(_ => _.Key.Item1).Max() + 1;
                            loadCase.NumberOfLoadStates = lastRow - SourceDataFirstLine + 1;
                            for (int row = SourceDataFirstLine - 1; row < lastRow; row++)
                            {
                                loadStates.Add(new LoadState
                                {
                                    FX = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FX - 1)]),
                                    FY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FY - 1)]),
                                    FZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FZ - 1)]),
                                    MY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MY - 1)]),
                                    MZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MZ - 1)]),
                                    Speed = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.Speed - 1)]) * ConvertSpeedMultiplyBy
                                });
                            }
                            loadCase.LoadStates = loadStates;
                            loadCases.Add(loadCase);
                        }
                        await Task.WhenAll();
                        break;
                    }
                case Enums.SourceDataType.TXT:
                    {
                        foreach (LoadCase loadCase in loadCasesWithoutLoadStates)
                        {
                            string loadCaseFilePath = ProjectDirectoryPath + @"\" + loadCase.Name + ".txt";
                            Dictionary<Tuple<int, int>, string> loadStateData = MV.FileProcessor.LoadDataFromFile_tableWithTabs(loadCaseFilePath);
                            List<LoadState> loadStates = new List<LoadState>();
                            int lastRow = loadStateData.Select(_ => _.Key.Item1).Max() + 1;
                            loadCase.NumberOfLoadStates = lastRow - SourceDataFirstLine + 1;
                            for (int row = SourceDataFirstLine - 1; row < lastRow; row++)
                            {
                                loadStates.Add(new LoadState
                                {
                                    FX = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FX - 1)]),
                                    FY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FY - 1)]),
                                    FZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.FZ - 1)]),
                                    MY = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MY - 1)]),
                                    MZ = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.MZ - 1)]),
                                    Speed = Convert.ToDouble(loadStateData[new Tuple<int, int>(row, SourceDataColumn.Speed - 1)]) * ConvertSpeedMultiplyBy
                                });
                            }
                            loadCase.LoadStates = loadStates;
                            loadCases.Add(loadCase);
                        }
                        await Task.WhenAll();
                        break;
                    }
                default:
                    throw new Exception("Not set SourceDataType");
            }
            Console.WriteLine($"Load Cases Loaded.");
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            return loadCases;
        }

        private async Task CalculateRadialReactions(List<LoadCase> loadCases)
        {
            const double B_ = 1825 / 1000.0;
            const double E_ = 2397 / 1000.0;
            const double a1 = 361.6 / 1000.0;
            const double b1 = 185 / 1000.0;
            const double a2 = 208.2 / 1000.0;
            const double b2 = 150 / 1000.0;
            const double A = a1 - b1 / 2.0;
            const double B = B_ + a2 - b2 / 2.0;
            const double C = 637.5 / 1000.0;
            const double E = E_ - a2 + b2 / 2.0;
            double FgShaftZ = -CP.FgShaft * MV.MathOperation.Cosd(6);
            double FgGearboxZ = -CP.FgGearbox * MV.MathOperation.Cosd(6);
            foreach (LoadCase loadCase in loadCases)
            {
                foreach (LoadState loadState in loadCase.LoadStates)
                {
                    double Fy1 = -loadState.FY + (loadState.MZ + loadState.FY * A) / (A + B);
                    double Fy2 = (-loadState.MZ - loadState.FY * A) / (A + B);
                    double Fz2 = (loadState.MY - loadState.FZ * A - FgShaftZ * (A + C) - FgGearboxZ * (A + B + E)) / (A + B);
                    double Fz1 = -loadState.FZ - FgShaftZ - Fz2 - FgGearboxZ;
                    double Fr1 = MV.MathOperation.LengthOfHypotenuse(Fy1, Fz1);
                    double Fr2 = MV.MathOperation.LengthOfHypotenuse(Fy2, Fz2);
                    loadState.FMBState = new BearingState()
                    {
                        FY = Fy1,
                        FZ = Fz1,
                        FR = Fr1
                    };
                    loadState.RMBState = new BearingState()
                    {
                        FY = Fy2,
                        FZ = Fz2,
                        FR = Fr2
                    };
                }
            }
            await Task.WhenAll();
        }

        private async Task CalculateAxialReactions(List<LoadCase> loadCases)
        {
            foreach (LoadCase loadCase in loadCases)
            {
                foreach (LoadState loadState in loadCase.LoadStates)
                {
                    double p = CalculateAxialForceRatio(loadState);
                    double sumFa = loadState.FX + CP.FgShaft * MV.MathOperation.Sind(6) + CP.FgGearbox * MV.MathOperation.Sind(6);
                    bool FaIsPossitive = sumFa >= 0;
                    sumFa = Math.Abs(sumFa);
                    double externalFaFMB = p * sumFa;
                    double externalFaRMB = sumFa - externalFaFMB;
                    double notInfluencedFaFMB; // neovlivněno druhým ložiskem
                    double notInfluencedFaRMB;
                    if (FaIsPossitive)
                    {
                        notInfluencedFaFMB = CP.AxialPreload + externalFaFMB;
                        notInfluencedFaRMB = CP.AxialPreload - externalFaRMB;
                    }
                    else if (!FaIsPossitive)
                    {
                        notInfluencedFaFMB = CP.AxialPreload - externalFaFMB;
                        notInfluencedFaRMB = CP.AxialPreload + externalFaRMB;
                    }
                    else
                    {
                        throw new Exception();
                    }
                    double generatedFaFromRMBFr = CalculateGeneratedAxialForce(CP.RMB, loadState.RMBState.FR, notInfluencedFaRMB);
                    double generatedFaFromFMBFr = CalculateGeneratedAxialForce(CP.FMB, loadState.FMBState.FR, notInfluencedFaFMB);
                    if (FaIsPossitive)
                    {
                        if (generatedFaFromRMBFr >= generatedFaFromFMBFr && sumFa >= 0)
                        {
                            loadState.RMBState.FA = generatedFaFromRMBFr + notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromRMBFr + notInfluencedFaFMB;
                        }
                        else if (generatedFaFromRMBFr < generatedFaFromFMBFr && sumFa >= generatedFaFromFMBFr - generatedFaFromRMBFr)
                        {
                            loadState.RMBState.FA = generatedFaFromRMBFr + notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromRMBFr + notInfluencedFaFMB;
                        }
                        else if (generatedFaFromRMBFr < generatedFaFromFMBFr && sumFa < generatedFaFromFMBFr - generatedFaFromRMBFr)
                        {
                            loadState.RMBState.FA = generatedFaFromFMBFr - notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromFMBFr + notInfluencedFaFMB;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else if (!FaIsPossitive)
                    {
                        if (generatedFaFromRMBFr <= generatedFaFromFMBFr && sumFa >= 0)
                        {
                            loadState.RMBState.FA = generatedFaFromFMBFr + notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromFMBFr + notInfluencedFaFMB;
                        }
                        else if (generatedFaFromRMBFr > generatedFaFromFMBFr && sumFa >= generatedFaFromFMBFr - generatedFaFromRMBFr)
                        {
                            loadState.RMBState.FA = generatedFaFromFMBFr + notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromFMBFr + notInfluencedFaFMB;
                        }
                        else if (generatedFaFromRMBFr > generatedFaFromFMBFr && sumFa < generatedFaFromFMBFr - generatedFaFromRMBFr)
                        {
                            loadState.RMBState.FA = generatedFaFromRMBFr + notInfluencedFaRMB;
                            loadState.FMBState.FA = generatedFaFromRMBFr - notInfluencedFaFMB;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }

            await Task.WhenAll();
        }

        private double CalculateAxialForceRatio(LoadState loadState)
        {
            return 1.0;
        }

        private double CalculateGeneratedAxialForce(BearingParametersColection bearing, double FR, double FA)
        {
            return 0.5 * FR / bearing.Y1;
        }

        private async Task CalculateLoadCaseAverageSpeeds(List<LoadCase> loadCases)
        {
            loadCases.ForEach(loadCase => loadCase.AverageSpeed = loadCase.LoadStates.Select(loadState => loadState.Speed).Average());
            await Task.WhenAll();
        }

        private async Task<(Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes)> FindMinsAndMaxes(List<LoadCase> loadCases)
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
            await Task.WhenAll();
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            return (loadMins, loadMaxes);
        }

        private Dictionary<Enums.LoadStateType, List<Level>> DefineLoadStateLevels(Dictionary<Enums.LoadStateType, double> loadMins, Dictionary<Enums.LoadStateType, double> loadMaxes)
        {
            Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels = new Dictionary<Enums.LoadStateType, List<Level>>();
            foreach (Enums.LoadStateType loadStateType in (Enums.LoadStateType[])Enum.GetValues(typeof(Enums.LoadStateType)))
            {
                if (loadStateType == Enums.LoadStateType.Speed)
                {
                    continue;
                }
                List<Level> levels = new List<Level>();
                double min = loadMins[loadStateType];
                double max = loadMaxes[loadStateType] + 0.1; // Because of populating with time shares.
                double range = Math.Abs(max - min) / NumberOfLevels;
                int numberOfNegativeLevels;
                if (max > 0.0)
                {
                    numberOfNegativeLevels = (int)Math.Floor(Math.Abs(min / range));
                }
                else
                {
                    numberOfNegativeLevels = NumberOfLevels;
                }
                double lastNegativeMax = -1.0;
                if (min < 0)
                {
                    for (int l = 0; l < numberOfNegativeLevels; l++)
                    {
                        Level level = new Level();
                        level.Min = min + l * range;
                        level.Mean = level.Min + range / 2.0;
                        level.Max = level.Min + range;
                        levels.Add(level);
                        if (l == numberOfNegativeLevels - 1)
                        {
                            lastNegativeMax = level.Max;
                        }
                    }
                }
                else
                {
                    numberOfNegativeLevels = -1;
                }
                if (max > 0)
                {
                    double positiveRange = max / (NumberOfLevels - numberOfNegativeLevels - 1);
                    if (min < 0)
                    {
                        Level maxNegativeLevel = new Level();
                        maxNegativeLevel.Min = lastNegativeMax;
                        maxNegativeLevel.Mean = lastNegativeMax / 2.0;
                        maxNegativeLevel.Max = 0.0;
                        levels.Add(maxNegativeLevel);
                        min = 0.0;
                    }
                    else
                    {
                        positiveRange = range;
                    }
                    for (int l = 0; l < NumberOfLevels - numberOfNegativeLevels - 1; l++)
                    {
                        Level level = new Level();
                        level.Min = min + l * positiveRange;
                        level.Mean = level.Min + positiveRange / 2.0;
                        level.Max = level.Min + positiveRange;
                        levels.Add(level);
                    }
                }
                loadStateLevels.Add(loadStateType, levels);
            }
            Console.WriteLine("Load state levels defined.");
            return loadStateLevels;
        }

        private async Task PopulateLoadStateLevelsByTimeAndRevSharesAsync(Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels, List<LoadCase> loadCases)
        {
            foreach (Enums.LoadStateType loadStateType in (Enums.LoadStateType[])Enum.GetValues(typeof(Enums.LoadStateType)))
            {
                if (loadStateType == Enums.LoadStateType.Speed)
                {
                    continue;
                }
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
                                                      .ForEach(x =>
                                                        {
                                                            x.TimeShare += loadCase.TimeShare / loadCase.NumberOfLoadStates;
                                                            x.RevShare += loadState.Speed * loadCase.TimeShare / (loadCase.NumberOfLoadStates * loadCase.AverageSpeed);
                                                        });
                    }
                }
                Console.WriteLine($"Time Shares and Revolution Shares of Load State Type {loadStateType} populated.");
                Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            }
            await Task.WhenAll();
            UnityValidation(loadStateLevels);
        }

        private void UnityValidation(Dictionary<Enums.LoadStateType, List<Level>> loadStateLevels)
        {
            foreach (KeyValuePair<Enums.LoadStateType, List<Level>> levels in loadStateLevels)
            {
                double timeShare = 0;
                double revShare = 0;
                foreach (Level level in levels.Value)
                {
                    timeShare += level.TimeShare;
                    revShare += level.RevShare;
                }
                if (timeShare < 0.99 || timeShare > 1.01)
                {
                    throw new Exception($"Sum of Time Shares at Load State Type: {levels.Key} is not equal to 1, but is {timeShare}.");
                }
                if (revShare < 0.95 || revShare > 1.05)
                {
                    throw new Exception($"Sum of Revolution Shares at Load State Type: {levels.Key} is not equal to 1, but is {revShare}.");
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
            Console.WriteLine("Excel file with Levels and their Time and Revolution Shares was saved to the Results Directory:");
            Console.WriteLine(file.DirectoryName);
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
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