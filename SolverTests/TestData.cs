using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SolverTests
{
    internal static class TestData
    {
        
        private static readonly string Folder = Path.Combine(Directory.GetCurrentDirectory(), $"TestPuzzles{Path.DirectorySeparatorChar}");

       public static IEnumerable<(int[,], int[,])> GetTestData(int difficulty)
        {
            return GetTestData($"difficulty-{difficulty}.txt", $"difficulty-{difficulty}-solutions.txt");
        }
        public static IEnumerable<(int[,], int[,])> GetTestData(string puzzlesFile, string solutionsFile)
        {
            string puzzlesPath = Path.Combine(Folder, puzzlesFile);
            string answersPath = Path.Combine(Folder, solutionsFile);
            if (!File.Exists(puzzlesPath) || !File.Exists(answersPath))
            {
                throw new Exception($"Either {puzzlesPath} or {answersPath} files are missing.");
            }

            using var puzzlesReader = new StreamReader(puzzlesPath);
            using var answersReader = new StreamReader(answersPath);
            while(!puzzlesReader.EndOfStream || !answersReader.EndOfStream)
            {
                string puzzle = puzzlesReader.ReadLine()!;
                string answers = answersReader.ReadLine()!;

                yield return ((ConvertToGrid(puzzle), ConvertToGrid(answers)));

            }
        }

        public static int[,] ConvertToGrid(string line)
        {
            var grid = new int[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int index = i * 9 + j;
                    
                    grid[i, j] = (int)char.GetNumericValue(line[index]);
                }
            }
            return grid;
        }
    }
}
