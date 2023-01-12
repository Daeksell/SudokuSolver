using SudokuSolver;

string puzzle = "002000041000082070000040009200079300010000080006810004100090000060430000850000400";

var solver = new Solver();
var grid = ConvertToGrid(puzzle);

solver.Solve(grid);

Console.WriteLine(solver.Board);








  int[,] ConvertToGrid(string line)
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