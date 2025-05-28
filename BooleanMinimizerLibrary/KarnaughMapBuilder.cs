using System;
using System.Collections.Generic;
using System.Linq;

namespace BooleanMinimizerLibrary
{
    public class KarnaughMapBuilder
    {
        public List<List<string>> Build(Node root, List<string> variables = null)
        {
            if (variables == null)
            {
                if (root.Type == NodeType.Vector && root.Variables != null)
                    variables = root.Variables;
                else
                    variables = GetVariables(root).OrderBy(v => v).ToList();
            }
            int varCount = variables.Count;
            if (varCount < 2 || varCount > 4)
                throw new Exception("Карта Карно поддерживает от 2 до 4 переменных");
            string vector = new FunctionVectorBuilder().BuildVector(root);
            if (varCount == 2)
                return BuildForTwoVariables(vector, variables);
            else if (varCount == 3)
                return BuildForThreeVariables(vector, variables);
            else // 4 variables
                return BuildForFourVariables(vector, variables);
        }

        private List<List<string>> BuildForTwoVariables(string vector, List<string> variables)
        {
            return new List<List<string>>
            {
                new List<string> { variables[0] + "\\" + variables[1], "0", "1" },
                new List<string> { "0", vector[0].ToString(), vector[1].ToString() },
                new List<string> { "1", vector[2].ToString(), vector[3].ToString() }
            };
        }

        private List<List<string>> BuildForThreeVariables(string vector, List<string> variables)
        {
            return new List<List<string>>
            {
                new List<string> { $"{variables[0]} \\ {variables[1]}{variables[2]}", "00", "01", "11", "10" },
                new List<string>
                    { "0", vector[0].ToString(), vector[1].ToString(), vector[3].ToString(), vector[2].ToString() },
                new List<string>
                    { "1", vector[4].ToString(), vector[5].ToString(), vector[7].ToString(), vector[6].ToString() }
            };
        }

        private List<List<string>> BuildForFourVariables(string vector, List<string> variables)
        {
            return new List<List<string>>
            {
                new List<string>
                    { variables[0] + variables[1] + "\\" + variables[2] + variables[3], "00", "01", "11", "10" },
                new List<string>
                    { "00", vector[0].ToString(), vector[1].ToString(), vector[3].ToString(), vector[2].ToString() },
                new List<string>
                    { "01", vector[4].ToString(), vector[5].ToString(), vector[7].ToString(), vector[6].ToString() },
                new List<string>
                {
                    "11", vector[12].ToString(), vector[13].ToString(), vector[15].ToString(), vector[14].ToString()
                },
                new List<string>
                    { "10", vector[8].ToString(), vector[9].ToString(), vector[11].ToString(), vector[10].ToString() }
            };
        }

        private HashSet<string> GetVariables(Node node)
        {
            var variables = new HashSet<string>();
            if (node == null) return variables;
            if (node.Type == NodeType.Vector && node.Variables != null)
            {
                foreach (var varName in node.Variables)
                    variables.Add(varName);
            }
            else if (node.Type == NodeType.Variable)
            {
                variables.Add(node.Value);
            }
            else
            {
                variables.UnionWith(GetVariables(node.Left));
                variables.UnionWith(GetVariables(node.Right));
            }
            return variables;
        }

        public List<Area> FindAllMaximalAreas(List<List<string>> map)
        {
            int rowCount = map.Count - 1;       // Без заголовков
            int colCount = map[0].Count - 1;
            var allAreas = new List<Area>();
            var ones = new HashSet<(int, int)>();
        
            // Собираем все позиции с "1"
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (map[r + 1][c + 1] == "1")
                        ones.Add((r, c));
                }
            }
        
            // Все возможные прямоугольные области
            var possibleHeights = GetPowersOfTwoUpTo(rowCount);
            var possibleWidths = GetPowersOfTwoUpTo(colCount);
            possibleHeights.Reverse();
            possibleWidths.Reverse();
        
            foreach (int height in possibleHeights)
            {
                foreach (int width in possibleWidths)
                {
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            var area = new Area(r, c, height, width);
                            var covered = area.GetCoveredCells(rowCount, colCount).ToList();
                            if (covered.All(pos => ones.Contains(pos)))
                            {
                                allAreas.Add(area);
                            }
                        }
                    }
                }
            }
        
            // Жадно выбираем области, покрывающие максимум новых единиц
            var result = new List<Area>();
            var coveredCells = new HashSet<(int, int)>();
        
            while (coveredCells.Count < ones.Count)
            {
                Area bestArea = null;
                int maxNew = 0;
        
                foreach (var area in allAreas)
                {
                    var cells = area.GetCoveredCells(rowCount, colCount).ToList();
                    int newCount = cells.Count(c => !coveredCells.Contains(c));
                    if (newCount > maxNew)
                    {
                        maxNew = newCount;
                        bestArea = area;
                    }
                }
        
                if (bestArea == null) break; // Не осталось новых областей
        
                result.Add(bestArea);
                foreach (var cell in bestArea.GetCoveredCells(rowCount, colCount))
                    coveredCells.Add(cell);
            }
        
            return result;
        }
        
        private bool IsValidAreaWithWrap(List<List<string>> map, bool[,] covered, int startRow, int startCol, int height, int width)
        {
            int rowCount = map.Count - 1;
            int colCount = map[0].Count - 1;

            for (int dr = 0; dr < height; dr++)
            {
                int r = (startRow + dr) % rowCount;
                for (int dc = 0; dc < width; dc++)
                {
                    int c = (startCol + dc) % colCount;

                    if (map[r + 1][c + 1] != "1") return false;
                    if (covered[r, c]) return false;
                }
            }

            return true;
        }

        private List<int> GetPowersOfTwoUpTo(int n)
        {
            var result = new List<int>();
            int power = 1;
            while (power <= n)
            {
                result.Add(power);
                power *= 2;
            }
            return result;
        }

        private static bool IsValidArea(List<List<string>> map, bool[,] covered,
            int startRow, int startColMap, int height, int width,
            int rows, int cols)
        {
            for (int dr = 0; dr < height; dr++)
            {
                int r = startRow + dr;
                if (r >= rows) return false;
                for (int dc = 0; dc < width; dc++)
                {
                    int c0 = startColMap - 1 + dc; // индекс в covered
                    int mapCol = c0 + 1; // индекс для map
                    if (mapCol >= map[r + 1].Count) return false; // Проверка границы столбца
                    if (map[r + 1][mapCol] != "1" || covered[r, c0])
                        return false;
                }
            }
            return true;
        }

        private void MarkAreaWithWrap(bool[,] covered, Area area, int rowCount, int colCount)
        {
            for (int dr = 0; dr < area.Height; dr++)
            {
                int r = (area.StartRow + dr) % rowCount;
                for (int dc = 0; dc < area.Width; dc++)
                {
                    int c = (area.StartCol + dc) % colCount;
                    covered[r, c] = true;
                }
            }
        }

        public class Area
        {
            public int StartRow { get; }
            public int StartCol { get; }
            public int Height { get; }
            public int Width { get; }

            public Area(int startRow, int startCol, int height, int width)
            {
                StartRow = startRow;
                StartCol = startCol;
                Height = height;
                Width = width;
            }

            public override string ToString()
            {
                return $"Area: row={StartRow}, col={StartCol}, height={Height}, width={Width}";
            }

            public IEnumerable<(int Row, int Col)> GetCoveredCells(int totalRows, int totalCols)
            {
                for (int dr = 0; dr < Height; dr++)
                {
                    int r = (StartRow + dr) % totalRows;
                    for (int dc = 0; dc < Width; dc++)
                    {
                        int c = (StartCol + dc) % totalCols;
                        yield return (r, c);
                    }
                }
            }
        }
    }
}
