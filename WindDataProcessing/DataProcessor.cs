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
        public DataProcessor(string loadCasesTimeShareFilePath, string projectDirectoryPath, string resultsDirectoryPath, string stifnessesFMBFilePath, string stifnessesRMBFilePath)
        {
            LoadCasesTimeShareFilePath = loadCasesTimeShareFilePath;
            ProjectDirectoryPath = projectDirectoryPath;
            ResultsDirectoryPath = resultsDirectoryPath;
            StiffnessFMBDataFilePath = stifnessesFMBFilePath;
            StiffnessRMBDataFilePath = stifnessesRMBFilePath;
            SourceDataColumn = new SourceDataColumnPosition();
        }

        /// <summary>
        /// Cesta k souboru se seznamem LoadCase a jejich četností (rates)
        /// </summary>
        public string LoadCasesTimeShareFilePath { get; }

        /// <summary>
        /// Adresář s daty
        /// </summary>
        public string ProjectDirectoryPath { get; }

        /// <summary>
        /// Adresář, kam se budou ukládat výsledky
        /// </summary>
        public string ResultsDirectoryPath { get; }

        public Enums.SourceDataType SourceDataType { get; set; }

        /// <summary>
        /// První řádek LTS souboru, kde začínají číselná data. Řádky začínají číslem 1.
        /// </summary>
        public int SourceDataFirstLine { get; set; }

        public SourceDataColumnPosition SourceDataColumn { get; set; }

        /// <summary>
        /// Pro potřeby převodu na LDD. Nastavte počet pásem, které se mají vytvořit.
        /// </summary>
        public int NumberOfLevels { get; set; }

        public double ConvertSpeedMultiplyBy { get; set; } = 1.0;
        public CalculationParametersCollection CP { get; set; }
        public string StiffnessFMBDataFilePath { get; set; }
        public string StiffnessRMBDataFilePath { get; set; }

        /// <summary>
        /// Vypočítá reakce v ložiscích
        /// </summary>
        /// <returns></returns>
        public async Task BearingReactions()
        {
            Console.WriteLine("Process started!");
            // V první řadě je třeba načíst data z disku do RAM.
            // Nejdřív se připraví List of Load Case na základě seznamu Load Case:
            List<LoadCase> loadCasesWithoutLoadStates = LoadLoadCaseData();
            // Následně se do Listu Load Case načtou jednotivé zátěžné stavy (Load State):
            List<LoadCase> loadCases = await PopulateLoadCasesWithLoadStatesAsync(loadCasesWithoutLoadStates);
            Console.WriteLine("Load states loaded. Radial reaction calculation started.");
            await CalculateRadialReactions(loadCases);
            Console.WriteLine("Radial reactions calculated. Axial reaction calculation started.");
            await CalculateAxialReactions(loadCases);
            TestFaReactions(loadCases);
            await CalculateEquivalentForces(loadCases);
            List<List<string>> exportData = PrepareDataToExport(loadCases);
            MV.FileProcessor.ExportCSV(exportData, ResultsDirectoryPath, @"results");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Hotovo.");
            Console.WriteLine();
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            Console.Beep();
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

        public async Task LDDlifesTester()
        {
            Console.WriteLine("Process started!");
            List<LoadCase> loadCasesWithoutLoadStates = LoadLoadCaseData();
            List<LoadCase> loadCases = await PopulateLoadCasesWithLoadStatesAsync(loadCasesWithoutLoadStates);
            Console.WriteLine("Load states loaded. Radial reaction calculation started.");
            await CalculateRadialReactions(loadCases);
            Console.WriteLine("Radial reactions calculated. Axial reaction calculation started.");
            await CalculateAxialReactions(loadCases);
            TestFaReactions(loadCases);
            await CalculateEquivalentForces(loadCases);
            List<List<string>> exportData = PrepareDataToExport(loadCases);
            MV.FileProcessor.ExportCSV(exportData, ResultsDirectoryPath, @"results");
            Console.WriteLine("Hotovo.");
            Console.WriteLine($"Elapsed time: {MV.SystemProcessor.GetElapsedTimeSinceApplicationStarted()}");
            Console.Beep();
        }

        private void TestFaReactions(List<LoadCase> loadCases)
        {
            List<double> FA_FMB = new List<double>();
            List<double> FA_RMB = new List<double>();
            foreach (LoadCase loadCase in loadCases)
            {
                foreach (LoadState loadState in loadCase.LoadStates)
                {
                    FA_FMB.Add(loadState.FMBState.FA);
                    FA_RMB.Add(loadState.RMBState.FA);
                }
            }
        }

        private List<List<string>> PrepareDataToExport(List<LoadCase> loadCases)
        {
            List<string> nameRow = new List<string>();
            List<string> FReqFMBRow = new List<string>();
            List<string> FAeqFMBRow = new List<string>();
            List<string> FReqRMBRow = new List<string>();
            List<string> FAeqRMBRow = new List<string>();
            List<string> NoFirstConditionRow = new List<string>();
            List<string> NoSecondConditionRow = new List<string>();
            List<string> NoThirdConditionRow = new List<string>();
            List<string> NoFourthConditionRow = new List<string>();
            List<string> NoFifthConditionRow = new List<string>();
            List<string> NoSixthConditionRow = new List<string>();
            foreach (LoadCase loadCase in loadCases)
            {
                nameRow.Add(loadCase.Name);
                FReqFMBRow.Add(loadCase.FReqFMB.ToString());
                FAeqFMBRow.Add(loadCase.FAeqFMB.ToString());
                FReqRMBRow.Add(loadCase.FReqRMB.ToString());
                FAeqRMBRow.Add(loadCase.FAeqRMB.ToString());
                NoFirstConditionRow.Add(loadCase.NoFirstCondition.ToString());
                NoSecondConditionRow.Add(loadCase.NoSecondCondition.ToString());
                NoThirdConditionRow.Add(loadCase.NoThirdCondition.ToString());
                NoFourthConditionRow.Add(loadCase.NoFourthCondition.ToString());
                NoFifthConditionRow.Add(loadCase.NoFifthCondition.ToString());
                NoSixthConditionRow.Add(loadCase.NoSixthCondition.ToString());
            }
            List<List<string>> data = new List<List<string>>
            {
                nameRow,
                FReqFMBRow,
                FAeqFMBRow,
                FReqRMBRow,
                FAeqRMBRow,
                NoFirstConditionRow,
                NoSecondConditionRow,
                NoThirdConditionRow,
                NoFourthConditionRow,
                NoFifthConditionRow,
                NoSixthConditionRow
            };
            return data;
        }

        private List<LoadCase> LoadLoadCaseData()
        {
            Dictionary<Tuple<int, int>, string> dataInTimeShareFile = MV.FileProcessor.LoadDataFromFile_csvSemicolon(LoadCasesTimeShareFilePath);
            List<LoadCase> loadCases = new List<LoadCase>();
            int loadCaseNumerator = 1;
            foreach (KeyValuePair<Tuple<int, int>, string> timeShareItem in dataInTimeShareFile)
            {
                if (timeShareItem.Key.Item2 == 0)
                {
                    loadCases.Add(new LoadCase
                    {
                        Position = loadCaseNumerator++,
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
            int loadCasesNumerator = 0;
            int nLoadCases = loadCasesWithoutLoadStates.Count;
            Console.WriteLine();
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
                            for (int row = SourceDataFirstLine - 1; row < lastRow - 1; row++)
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
                            Console.Write($"\rLoading LTS to RAM: Load case {loadCasesNumerator++} of {nLoadCases} loaded.    ");
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
                            Console.Write($"\rLoading LTS to RAM: Load case {loadCasesNumerator++} of {nLoadCases} loaded.    ");
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

        /// <summary>
        /// Výpočet radiálních reakcí je odvozen z rovnic rovnováhy.
        /// Z hlediska radiálních reakcí se jedná o staticky určitou úlohu.
        /// </summary>
        /// <param name="loadCases"></param>
        /// <returns></returns>
        private async Task CalculateRadialReactions(List<LoadCase> loadCases)
        {
            const double A = 2330 / 1000.0;
            const double B = 2380 / 1000.0;
            const double C = 1735 / 1000.0;
            double a = CP.FMB.Arm_a / 1000.0;
            const double b = 176 / 1000.0;
            double E = a - b / 2.0;
            double D = A - E;
            const double F = 585 / 1000.0;
            double G = E;
            double H = C - G;
            double FgShaftZ = -CP.FgShaft * MV.MathOperation.Cosd(6);
            double FgGearboxZ = -CP.FgGearbox * MV.MathOperation.Cosd(6);
            //const double B_ = 1825 / 1000.0;                   --- GAMESA
            //const double E_ = 2397 / 1000.0;
            //double a1 = CP.FMB.Arm_a / 1000.0;
            //const double b1 = 185 / 1000.0;
            //double a2 = CP.RMB.Arm_a / 1000.0;
            //const double b2 = 150 / 1000.0;
            //double A = a1 - b1 / 2.0;
            //double B = B_ + a2 - b2 / 2.0;
            //const double C = 637.5 / 1000.0;
            //double E = E_ - a2 + b2 / 2.0;
            //double FgShaftZ = -CP.FgShaft * MV.MathOperation.Cosd(6);
            //double FgGearboxZ = -CP.FgGearbox * MV.MathOperation.Cosd(6);
            foreach (LoadCase loadCase in loadCases)
            {
                foreach (LoadState loadState in loadCase.LoadStates)
                {
                    double Fy1 = -loadState.FY - (-loadState.MZ + loadState.FY * D) / (A + B + G - D); // Nanjing
                    double Fy2 = (-loadState.MZ + loadState.FY * D) / (A + B + G - D);
                    double Fz2 = (-loadState.MY - (FgShaftZ + FgGearboxZ + loadState.FZ) * D + FgShaftZ * (A + F) + FgGearboxZ * (A + B + C)) / (D - A - B - G);
                    double Fz1 = -FgShaftZ - FgGearboxZ - loadState.FZ - Fz2;

                    //double Fy1 = -loadState.FY + (loadState.MZ + loadState.FY * A) / (A + B);                         --- GAMESA
                    //double Fy2 = (-loadState.MZ - loadState.FY * A) / (A + B);
                    //double Fz2 = (loadState.MY - loadState.FZ * A - FgShaftZ * (A + C) - FgGearboxZ * (A + B + E)) / (A + B);
                    //double Fz1 = -loadState.FZ - FgShaftZ - Fz2 - FgGearboxZ;
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

        /// <summary>
        /// Výpočet axiálních reakcí zohledňuje, že ložiska jsou k sobě předepjata známou hodnotou silového předpětí.
        /// Zároveň počítá a zohledňuje axiální sílu, která se generuje působením radiální síly.
        /// Výpočet je verifikován dle mnoha jiných výpočtů pomocí komerčního software Mesys.
        /// </summary>
        /// <param name="loadCases"></param>
        /// <returns></returns>
        private async Task CalculateAxialReactions(List<LoadCase> loadCases)
        {
            foreach (LoadCase loadCase in loadCases)
            {
                foreach (LoadState loadState in loadCase.LoadStates)
                {
                    // V první řadě se vypočíta suma vnějšíxh axiálních sil, které půosbí na hřídeli:
                    double sumFa = loadState.FX + CP.FgShaft * MV.MathOperation.Sind(6) + CP.FgGearbox * MV.MathOperation.Sind(6);
                    AxialReactionCalculator axialReactionCalculator = new AxialReactionCalculator(StiffnessFMBDataFilePath, StiffnessRMBDataFilePath);
                    double FaFMB = axialReactionCalculator.CaluclateFaFMB(loadState.FMBState.FR, loadState.RMBState.FR, sumFa);
                    double FaRMB = FaFMB - sumFa;
                    loadState.FMBState.FA = FaFMB;
                    loadState.RMBState.FA = FaRMB;
                }
                Console.Write($"\rLoad case {loadCase.Position} axial reaction calculation finished.     ");
            }

            await Task.WhenAll();
        }

        private double CalculateGeneratedAxialForceOptimized(BearingParametersColection bearing, double FR, double NI)
        {
            double forceRatio = FR * MV.MathOperation.Tand(bearing.ContactAngle) / NI;
            if (forceRatio <= bearing.ForceGenerationCoef_a02)
            {
                return 0.0;
            }
            double C = bearing.ForceGenerationCoef_a01 * Math.Atan(forceRatio - bearing.ForceGenerationCoef_a02);
            return C * FR / bearing.Y1;
        }

        private double _FaGen;

        private Task<double> CalculateGeneratedAxialForceSimple(BearingParametersColection bearing, double FR, double notInfluencedFA)
        {
            //double forceRatio = MV.Algorithm.MetodaPuleniIntervalu(GeneratedAxialForceFunction, new List<object> { bearing, FR, notInfluencedFA }, 0.00001, 0.85, 0.00001, 10000);
            //return Task.FromResult(_FaGen);
            double forceRatio = 0.0001;
            double krok = 0.01;
            Dictionary<double, double> odchylky = new Dictionary<double, double>();
            for (int i = 0; i < 100; i++)
            {
                forceRatio += krok;
                double odchylka = GeneratedAxialForceFunction(forceRatio, new List<object> { bearing, FR, notInfluencedFA });
                double FaGen = _FaGen;
                odchylky.Add(FaGen, odchylka);
            }
            throw new NotImplementedException();
        }

        private double GeneratedAxialForceFunction(double forceRatio, List<object> objects)
        {
            BearingParametersColection bearing = (BearingParametersColection)objects[0];
            double FR = (double)objects[1];
            double NI = (double)objects[2];
            double FA = FR * MV.MathOperation.Tand(bearing.ContactAngle) / forceRatio;
            double C = -0.5553 * forceRatio + 0.9432;
            _FaGen = C * FR / bearing.Y1;
            return FA - _FaGen;
        }

        /// <summary>
        /// Výpočet části axiální síly z celkové vnější, která bude působit na přední ložisko, zohledňující předpětí.
        /// V komerčním software Mesys byl vytvořen kompletní model sestavy. Ložiska byla předepjata konkrétní axiální silou.
        /// Sestava byla zatěžována vnější axiální silou od záporných hodnot do kladných a pro každé zatížení byly odečteny axiální deformace obou ložisek.
        /// Z těchto hodnot vznikl graf, jako je na obr. 8.6 na str. 182 v knize Essential Concepts of Bearing Technology od Harrise. Kniha je v adresáři.
        /// Jedná se tedy o dvě tuhostní křivky deformace-síla (pro každé ložisko jedna).
        /// Tyto dvě křivky byly proloženy kvadratickou funkcí. Ta je popsána koeficienty a - f.
        /// Tyto koeficienty jsou vstupem do výpočtu a s každou změnou tuhosti sestavy je třeba zjistit koeficienty nové.
        /// Následně se provede odečtení těchto dvou funkcí od sebe.
        /// Dosadí se vnější zatížení a vyjde pak nové zatížení, což je poměrná část, která působí na jedno z ložisek.
        /// </summary>
        /// <param name="sumFa">Celková vnější axiální síla</param>
        /// <returns></returns>
        private double CalculateFMBPartOfAxialForce(double sumFa)
        {
            double a_d = CP.StiffnesCoefficient_a - CP.StiffnesCoefficient_d;
            double b_e = CP.StiffnesCoefficient_b - CP.StiffnesCoefficient_e;
            double c_f_FA = CP.StiffnesCoefficient_c - CP.StiffnesCoefficient_f - sumFa;
            //double determinant = Math.Sqrt(Math.Pow(b_e, 2.0) - 4 * a_d * c_f_FA);            --- GAMESA
            //if (double.IsNaN(determinant))
            //{
            //    throw new Exception("Determinant není číslo");
            //}
            //double x = (-b_e + determinant) / (2 * a_d);
            double x = sumFa / b_e;
            double result = CP.StiffnesCoefficient_a * Math.Pow(x, 2.0) + CP.StiffnesCoefficient_b * x + CP.StiffnesCoefficient_c;
            if (result < 0)
            {
                throw new Exception("Axiální síla na FMB je menší než 0");
            }
            return result;
        }

        /// <summary>
        /// Výpočet části axiální síly z celkové vnější, která bude působit na přední ložisko, zohledňující předpětí.
        /// V komerčním software Mesys byl vytvořen kompletní model sestavy. Ložiska byla předepjata konkrétní axiální silou.
        /// Sestava byla zatěžována vnější axiální silou od záporných hodnot do kladných a pro každé zatížení byly odečteny axiální deformace obou ložisek.
        /// Z těchto hodnot vznikl graf, jako je na obr. 8.6 na str. 182 v knize Essential Concepts of Bearing Technology od Harrise. Kniha je v adresáři.
        /// Jedná se tedy o dvě tuhostní křivky deformace-síla (pro každé ložisko jedna).
        /// Tyto dvě křivky byly proloženy kvadratickou funkcí. Ta je popsána koeficienty a - f.
        /// Tyto koeficienty jsou vstupem do výpočtu a s každou změnou tuhosti sestavy je třeba zjistit koeficienty nové.
        /// Následně se provede odečtení těchto dvou funkcí od sebe.
        /// Dosadí se vnější zatížení a vyjde pak nové zatížení, což je poměrná část, která působí na jedno z ložisek.
        /// </summary>
        /// <param name="sumFa">Celková vnější axiální síla</param>
        /// <returns></returns>
        private double CalculateRMBPartOfAxialForce(double sumFa)
        {
            double a_d = CP.StiffnesCoefficient_a - CP.StiffnesCoefficient_d;
            double b_e = CP.StiffnesCoefficient_b - CP.StiffnesCoefficient_e;
            double c_f_FA = CP.StiffnesCoefficient_c - CP.StiffnesCoefficient_f - sumFa;
            //double determinant = Math.Sqrt(Math.Pow(b_e, 2.0) - 4 * a_d * c_f_FA);                 --- GAMESA
            //if (double.IsNaN(determinant))
            //{
            //    throw new Exception("Determinant není číslo");
            //}
            //double x = (-b_e + determinant) / (2 * a_d);
            double x = sumFa / b_e;
            double result = CP.StiffnesCoefficient_d * Math.Pow(x, 2.0) + CP.StiffnesCoefficient_e * x + CP.StiffnesCoefficient_f;
            if (result < 0)
            {
                throw new Exception("Axiální síla na FMB je menší než 0");
            }
            return result;
        }

        /// <summary>
        /// Výpočet generované axiální síly působením radiální síly. Zohledňuje působení vnější axiální síly včetně předpětí.
        /// Bylo zjištěno (Mesys), že působením radiální síly se generuje různá axiální síla v závislosti na tom, jaká už na ložisko působí jiná axiální síla.
        /// Proto je potřeba, aby vstupem od funkce byla kromě informací o ložisku a aplikované radiální síly i vnější (notInfluenced) axiální síla.
        /// Pro výpočet axiální síly je využito přístupu popsaném v kapitole 7.5.1 SINGLE-ROW BEARINGS
        /// - Bearings Under Combined Radial and Thrust Load, od str. 165, Harris, Essential Concepts of Bearing Technology.
        /// Podstatné je využití tabulky 7.4 na str. 170, která dává do souvislosti:
        ///     -   parametr epsilon (rozložení sil přes valivé elementy)
        ///     -   poměr radiální a axiální síly Fr*tg(alpha)/Fa a
        ///     -   Jr a Ja integrály
        /// Dále pak využití vzorců 7.70 a 7.71, které říkají, že s využitím patřičných J integrálů lze vypočítát sílu na maximálně zatížený valivý element oběma způsoby,
        /// tedy přes radiální sílu i přes axiální sílu.
        /// Iterací jednoho z parametrů (např. poměru sil od 0 do 1) lze zjistit jeho správná hodnota,
        /// což je ta, při které vychází stejná maximální síla na element Qmax z obou vzorců 7.70 i 7.71.
        /// Touto iterací lze naléz více řešení.
        /// Na základě spousty výpočtů bylo zjišteno, že tato řešení jsou vždy stejná, nezávisle na působících silách či geometrii ložiska.
        /// Tato řešení jsou znázorněna na obrázku v dokumentu "Kořeny algoritmu řešení generovaných axiálních sil.pdf", přiložen v adresáři.
        /// Tato řešení jsou nahrána jako Values of Dictionary "forceRatiosAccToLimits".
        /// Na základě působící vnější axiální síly a působící radiální reakce jsou odhadnuty startovací hodnoty silového poměru.
        /// Dle Keys of Dictionary "forceRatiosAccToLimits" se rozhodne, která výsledná hodnota silového poměru bude zvolena. Tyto Keys jsou meze,
        /// které byly určeny odchylkou mezi dvěma silami Qmax(Fr) a Qmax(Fa).
        /// Dle získaného silového poměru lze dopočítat axiální síla působicí na ložisko.
        /// Rozdíl této síly a vnější axiální síly (notInfluencedFA) je generovaná axiální síla.
        /// Nikdy nemůže být generovaná axiální síla menší, než 0.
        /// </summary>
        /// <param name="bearing">Ložisko s informacemi o kontaktním úhlu a počtu valivých elementů.</param>
        /// <param name="FR">Aplikovaná radiální síla</param>
        /// <param name="notInfluencedFA">Přítomná axiální síla včetně předpětí. Neovlivněná pouze generovanou axiální silou, která se má teprve vypočítat.</param>
        /// <returns>Generovaná axiální síla působením radiální</returns>
        private Task<double> CalculateGeneratedAxialForce(BearingParametersColection bearing, double FR, double notInfluencedFA)
        {
            // Pokud je ložisko zevnějšku zatíženo pouze radiálně a nedisponuje vůlí, pak musí být dle tab. 7.4 silový poměr roven 0.7939 (eps = 0.5)
            const double forceRatioAtEps0_5 = 0.7939;
            // Pro tento stav je vypočítána generovaná axiální síla:
            double FAproEps0_5 = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / forceRatioAtEps0_5;
            // Startovací hodnota je vypočítána dosazením větší hodnoty axiální síly.
            // Vybírá se mezi generovanou axiální silou pro eps = 0.5 a vnější axiální silou včetně předpětí (notInfluencedFA):
            double startingForceRatio = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / Math.Max(notInfluencedFA, FAproEps0_5);
            if (startingForceRatio > 1)
            {
                startingForceRatio = 0.99999;
            }
            // Pro následující proběhl výzkum a s mnoha výpočty. Iteračně byly nalezeny následující meze (keys) a jejich hodnoty (values):
            Dictionary<double, double> forceRatiosAccToLimits = new Dictionary<double, double>
            {
                {0.80, 0.879067460},
                {0.43, 0.654691500},
                {0.06, 0.275388920},
                {0.00, 0.032643341}
            };
            foreach (var item in forceRatiosAccToLimits)
            {
                if (startingForceRatio > item.Key)
                {
                    double forceRatio = item.Value;
                    double ratioDifference = Math.Abs(forceRatio - startingForceRatio);
                    double FA = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / forceRatio;
                    if (FA - notInfluencedFA < 0)
                    {
                        return Task.FromResult(0.0);
                    }
                    return Task.FromResult(FA - notInfluencedFA);
                }
            }
            throw new Exception("Chyba ve výpočtu silového poměru");
        }

        private Task<double> CalculateGeneratedAxialForce2(BearingParametersColection bearing, double FR, double notInfluencedFA)
        {
            Dictionary<double, double> deltaQmaxResults = new Dictionary<double, double>();
            const double silovyPomerProEps0_5 = 0.7939;
            double FAproEps0_5 = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / silovyPomerProEps0_5;
            double silovyPomer = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / Math.Max(notInfluencedFA, FAproEps0_5); /*(FAproEps0_5 + notInfluencedFA);*/
            if (silovyPomer > 1)
            {
                silovyPomer = 0.99999;
            }
            //double final = MV.Algorithm.MetodaPuleniIntervalu(CalculateAxialForceResiduum, new List<object> { bearing.ContactAngle, bearing.Z, FR, notInfluencedFA }, 0.86, 0.88, 0.000000001, 1000000);

            double krok = -0.01;
            for (; silovyPomer > 0; silovyPomer += krok)
            {
                double deltaQmax0 = CalculateDeltaQmaxDleSilovehoPomeru2(bearing.Z, bearing.ContactAngle, FR, silovyPomer);
                deltaQmaxResults.Add(silovyPomer, deltaQmax0);
            }

            //if (FA - notInfluencedFA < 0)
            //{
            //    return Task.FromResult(0.0);
            //}
            //else
            //{
            //    return Task.FromResult(FA - notInfluencedFA);
            //}
            throw new NotImplementedException();
        }

        private Task<double> CalculateGeneratedAxialForce3(BearingParametersColection bearing, double FR, double notInfluencedFA)
        {
            //double silovyPomer = MV.Algorithm.MetodaPuleniIntervalu(CalculateAxialForceResiduum, new List<object> { bearing.ContactAngle, bearing.Z, FR, notInfluencedFA }, 0.0, 1.0, 0.0001, 100000);
            const double silovyPomerProEps0_5 = 0.7939;
            const double correction = 1.0;
            double FAproEps0_5 = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / silovyPomerProEps0_5;
            double silovyPomer = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / Math.Max(notInfluencedFA, FAproEps0_5); /*(FAproEps0_5 + notInfluencedFA);*/
            double startovaciSilovyPomer = silovyPomer;
            if (silovyPomer > 1)
            {
                silovyPomer = 0.99999;
            }
            double krok = -0.000001;
            double deltaQmax0 = CalculateDeltaQmaxDleSilovehoPomeru(bearing.Z, bearing.ContactAngle, FR, silovyPomer);
            silovyPomer += krok;
            double deltaQmax1 = CalculateDeltaQmaxDleSilovehoPomeru(bearing.Z, bearing.ContactAngle, FR, silovyPomer);
            if (deltaQmax1 > deltaQmax0)
            {
                krok = +0.000001; // VyŘešit problém, co když to vyroste přes 1.
                silovyPomer += 2 * krok;
            }
            double FA0 = 0;
            double FA = FA0;
            Dictionary<double, double> deltaQmaxResults = new Dictionary<double, double>();
            bool ukoncitVeChviliKdyPoroste = false;
            for (int i = 0; silovyPomer > 0 && silovyPomer < 1; i++)
            {
                double FA1 = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / silovyPomer;
                double deltaQmaxNext = CalculateDeltaQmax(bearing.Z, bearing.ContactAngle, FR, FA1);
                deltaQmaxResults.Add(silovyPomer, deltaQmaxNext);
                if (deltaQmaxNext > deltaQmax0)
                {
                    FA = FA0;
                    break;
                }
                else
                {
                    FA0 = FA1;
                    deltaQmax0 = deltaQmaxNext;
                    silovyPomer += krok;
                }
                //if (deltaQmaxNext > deltaQmax0)
                //{
                //    deltaQmax0 = deltaQmaxNext;
                //    silovyPomer += krok;
                //    if (ukoncitVeChviliKdyPoroste)
                //    {
                //        FA = FA0;
                //        break;
                //    }
                //    else
                //    {
                //        FA0 = FA1;
                //    }
                //}
                //else
                //{
                //    FA0 = FA1;
                //    deltaQmax0 = deltaQmaxNext;
                //    silovyPomer += krok;
                //    ukoncitVeChviliKdyPoroste = true;
                //}
            }
            if (silovyPomer >= 1)
            {
                throw new ForceRatioOutOfRangeException($"Silový poměr vyšel větší, než 1: {silovyPomer}");
            }
            if (silovyPomer > 0.7)
            {
                throw new Exception();
            }
            if (FA - notInfluencedFA < 0)
            {
                return Task.FromResult(0.0);
            }
            else
            {
                return Task.FromResult(FA - notInfluencedFA);
            }
            //double FA = (FR * MV.MathOperation.Tand(bearing.ContactAngle)) / silovyPomer;
            //return Task.FromResult(FA - notInfluencedFA);
        }

        private double CalculateDeltaQmax(double Z, double contactAngle, double FR, double FA)
        {
            double silovyPomer = (FR * MV.MathOperation.Tand(contactAngle)) / FA;
            double Jr = CalculateJrIntegral(silovyPomer, Z);
            double Ja = CalculateJaIntegral(silovyPomer, Z);
            double Qmax_FR = FR / (Z * Jr * MV.MathOperation.Cosd(contactAngle));
            double Qmax_FA = FA / (Z * Ja * MV.MathOperation.Sind(contactAngle));
            double deltaQmax = Math.Abs(Qmax_FR - Qmax_FA);
            return deltaQmax;
        }

        private double CalculateDeltaQmax2(double Z, double contactAngle, double FR, double FA)
        {
            double silovyPomer = (FR * MV.MathOperation.Tand(contactAngle)) / FA;
            double Jr = CalculateJrIntegral(silovyPomer, Z);
            double Ja = CalculateJaIntegral(silovyPomer, Z);
            double Qmax_FR = FR / (Z * Jr * MV.MathOperation.Cosd(contactAngle));
            double Qmax_FA = FA / (Z * Ja * MV.MathOperation.Sind(contactAngle));
            double deltaQmax = Qmax_FR - Qmax_FA;
            return deltaQmax;
        }

        private double CalculateDeltaQmaxDleSilovehoPomeru(double Z, double contactAngle, double FR, double silovyPomer)
        {
            double FA = (FR * MV.MathOperation.Tand(contactAngle)) / silovyPomer;
            return CalculateDeltaQmax(Z, contactAngle, FR, FA);
        }

        private double CalculateDeltaQmaxDleSilovehoPomeru2(double Z, double contactAngle, double FR, double silovyPomer)
        {
            double FA = (FR * MV.MathOperation.Tand(contactAngle)) / silovyPomer;
            return CalculateDeltaQmax2(Z, contactAngle, FR, FA);
        }

        private double CalculateAxialForceResiduum(double silovyPomer, List<object> parameters)
        {
            double contactAngle = (double)parameters[0];
            double Z = (double)parameters[1];
            double FR = (double)parameters[2];
            double notInfluencedFA = (double)parameters[3];
            double FA = (FR * MV.MathOperation.Tand(contactAngle)) / silovyPomer;
            double Jr = CalculateJrIntegral(silovyPomer, Z);
            double Ja = CalculateJaIntegral(silovyPomer, Z);
            double eps = CalculateEpsilon(silovyPomer);
            double Qmax_extRA = FR / (Z * Jr * MV.MathOperation.Cosd(contactAngle));
            double Qmax_extFA = (FA) / (Z * Ja * MV.MathOperation.Sind(contactAngle));
            return Qmax_extRA - Qmax_extFA;
            //double Qmax = Qmax_extRA + Qmax_extFA;
            //List<double> Qpsi = new List<double>();
            //double psi_krok = 360 / Z;
            //for (int z = 0; z < Z; z++)
            //{
            //    double psi = z * psi_krok;
            //    double Qpsi_i = Qmax * Math.Pow(1 - 1 / (2 * eps) * (1 - MV.MathOperation.Cosd(psi)), CP.n);
            //    if (double.IsNaN(Qpsi_i))
            //    {
            //        Qpsi_i = 0;
            //    }
            //    Qpsi.Add(Qpsi_i);
            //}
            //double RA = 0;
            //for (int z = 0; z < Z; z++)
            //{
            //    RA += Qpsi[z] * MV.MathOperation.Sind(contactAngle);
            //}
            //return RA - FA;
        }

        private double CalculateEpsilon(double silovyPomer)
        {
            const double a01 = 1.834977077;
            const double a02 = 5.726495998;
            const double a03 = 9.142795181;
            const double a04 = 0.821406651;
            const double a05 = 12.90083314;
            const double a06 = 40.35269018;
            double absolutniClen = Math.Abs(silovyPomer - 1);
            double prvniClen = a01 * Math.Pow(absolutniClen, a04);
            double druhyClen = a02 * Math.Pow(absolutniClen, a05);
            double tretiClen = a03 * Math.Pow(absolutniClen, a06);
            return prvniClen + druhyClen + tretiClen;
        }

        private double CalculateJrIntegral(double silovyPomer, double Z)
        {
            if (silovyPomer < 0.9215)
            {
                const double a01 = -0.84823;
                const double a02 = -0.29649;
                const double a03 = -0.6217;
                const double a04 = 0.266937;
                const double a05 = -0.61334;
                return a01 * Math.Pow(silovyPomer + a03, 2.0) + a04 + a02 * Math.Pow(silovyPomer + a05, 3.0);
            }
            else
            {
                return MV.Algorithm.LinearniInterpolace(0.9215, 0.1737, 1.0, 1 / Z, silovyPomer);
            }
        }

        private double CalculateJaIntegral(double silovyPomer, double Z)
        {
            if (silovyPomer < 0.9215)
            {
                const double a01 = -0.334732428;
                const double a02 = -1.44886561;
                const double a03 = 0.221814449;
                const double a04 = 0.663496962;
                const double a05 = -0.622728653;
                return a01 * Math.Pow(silovyPomer + a03, 2.0) + a04 + a02 * Math.Pow(silovyPomer + a05, 3.0);
            }
            else
            {
                return MV.Algorithm.LinearniInterpolace(0.9215, 0.1737, 1.0, 1 / Z, silovyPomer);
            }
        }

        private async Task CalculateEquivalentForces(List<LoadCase> loadCases)
        {
            int lcIterator = 0;
            foreach (LoadCase loadCase in loadCases)
            {
                List<double> tini = new List<double>();
                List<double> FR_RMB_tini = new List<double>();
                List<double> FA_RMB_tini = new List<double>();
                List<double> FR_FMB_tini = new List<double>();
                List<double> FA_FMB_tini = new List<double>();
                try
                {
                    foreach (LoadState loadState in loadCase.LoadStates)
                    {
                        double tini_i = loadState.Speed * 0.04;
                        tini.Add(tini_i);
                        FR_FMB_tini.Add(Math.Pow(loadState.FMBState.FR, CP.n) * tini_i);
                        FR_RMB_tini.Add(Math.Pow(loadState.RMBState.FR, CP.n) * tini_i);
                        FA_FMB_tini.Add(Math.Pow(loadState.FMBState.FA, CP.n) * tini_i);
                        FA_RMB_tini.Add(Math.Pow(loadState.RMBState.FA, CP.n) * tini_i);
                    }
                    double sum_tini = tini.Sum();
                    double sum_FR_FMB = FR_FMB_tini.Sum();
                    double sum_FR_RMB = FR_RMB_tini.Sum();
                    double sum_FA_FMB = FA_FMB_tini.Sum();
                    double sum_FA_RMB = FA_RMB_tini.Sum();
                    double ratio_FR_FMB = sum_FR_FMB / sum_tini;
                    double ratio_FR_RMB = sum_FR_RMB / sum_tini;
                    double ratio_FA_FMB = sum_FA_FMB / sum_tini;
                    double ratio_FA_RMB = sum_FA_RMB / sum_tini;
                    loadCase.FReqFMB = Math.Pow(ratio_FR_FMB, 1 / CP.n);
                    loadCase.FReqRMB = Math.Pow(ratio_FR_RMB, 1 / CP.n);
                    loadCase.FAeqFMB = Math.Pow(ratio_FA_FMB, 1 / CP.n);
                    loadCase.FAeqRMB = Math.Pow(ratio_FA_RMB, 1 / CP.n);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Problém v loadCase {lcIterator}: {loadCase.Name}");
                    throw;
                }
                lcIterator++;
            }

            await Task.WhenAll();
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