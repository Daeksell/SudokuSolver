using SudokuSolver;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;

namespace SolverTests
{
    public class UnitTests
    {

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        // Puzzles are from http://www.printable-sudoku-puzzles.com/sudoku_online/
        public void TestSolver(int difficulty)
        {
            var data = TestData.GetTestData(difficulty);
            var solver = new Solver();
            int solved = 0;
            int puzzleNumber = 1;
            foreach (var (puzzle, expectedAnswer) in data)
            {
                puzzleNumber++;
                var answer = solver.Solve(puzzle);
                try
                {
                    Assert.Equal(expectedAnswer, answer);
                    solved++;
                }
                catch (EqualException e)
                {
                    Debug.WriteLine($"Failed to solve puzzle {puzzleNumber} difficulty {difficulty}. {e.Message}");
                    continue;
                }
            }
            Assert.True(solved == data.Count(), $"Solved only {solved}/{data.Count()}");
        }

        [Fact]
        public void TestHomework()
        {
            var solver = new Solver();
            var data = TestData.GetTestData("puzzles-from-homework.txt", "puzzles-from-homework-solutions.txt");
            foreach (var (puzzle, expectedAnswer) in data)
            {
                var answer = solver.Solve(puzzle);
                Assert.Equal(expectedAnswer, answer);
            }
        }
        //https://abcnews.go.com/blogs/headlines/2012/06/can-you-solve-the-hardest-ever-sudoku
        [Fact]
        public void TestHardestSudoku()
        {
            string puzzleString = "800000000003600000070090200050007000000045700000100030001000068008500010090000400";
            string solutionString = "812753649943682175675491283154237896369845721287169534521974368438526917796318452";
            var solver = new Solver();
            var puzzle = TestData.ConvertToGrid(puzzleString);
            var expectedAnswer = TestData.ConvertToGrid(solutionString);

            var answer = solver.Solve(puzzle);
            Assert.Equal(expectedAnswer, answer);

        }
    }
}