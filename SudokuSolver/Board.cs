using System.Collections.ObjectModel;
using System.Text;

namespace SudokuSolver
{
    public class Board
    {
        public int[,] Grid { get; private set; }
        public static readonly ReadOnlyCollection<((int min, int max) xBounds, (int min, int max) yBounds)> BoxBounds;
        internal Dictionary<(int x, int y), List<int>> MapOfCandidates { get; set; } = new Dictionary<(int, int), List<int>>(81);

        private Stack<int[,]> GridStates { get; } = new Stack<int[,]>();
        private Stack<Dictionary<(int x, int y), List<int>>> MapOfCandidatesStates { get; } = new Stack<Dictionary<(int x, int y), List<int>>>();
        private Dictionary<(int x, int y), List<(int x, int y)>> MapOfPeers { get; } = new Dictionary<(int x, int y), List<(int x, int y)>>(81);

        public int EmptySquaresCount => MapOfCandidates.Count;

        static Board()
        {
            var list = new List<((int xMin, int xMax), (int yMin, int yMax))>(9);

            for (int x = 0; x < 9; x += 3)
            {
                for (int y = 0; y < 9; y += 3)
                {
                    list.Add(((x, x + 2), (y, y + 2)));
                }
            }
            BoxBounds = list.AsReadOnly();
        }

        public int this[int x, int y]
        {
            get
            {
                return Grid[x, y];
            }
            internal set
            {
                Grid[x, y] = value;
                var index = (x, y);
                MapOfCandidates.Remove(index);
                RecalculateDependentCandidates(x, y, value);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    sb.AppendJoin(' ', this[x,y]);
                    if (y == (y / 3) + 2 && y != 8)
                    {
                        sb.Append('|');
                    }
                }
                if (x == (x / 3) + 2 && x != 8)
                {
                    sb.AppendLine("------+------+------");
                }
            }
            return sb.ToString();
        }

        internal Board() : this(new int[9, 9])
        {

        }
        public Board(int[,] grid)
        {
            ValidateInput(grid);
            this.Grid = grid;

            // Intitialize candidates for each empty square
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (this[i, j] == 0)
                    {
                        var currentCandidates = new List<int>(9) { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                        foreach (var value in GetAllClues(i, j))
                        {
                            currentCandidates.Remove(value);
                        }

                        var index = (i, j);
                        if (!MapOfCandidates.ContainsKey(index))
                        {
                            MapOfCandidates.Add(index, currentCandidates);
                        }
                    }
                }
            }

        }

        private static void ValidateInput(int[,] grid)
        {
            _ = grid ?? throw new ArgumentNullException(nameof(grid));

            if (grid.GetLength(0) != 9 && grid.GetLength(1) != 9)
            {
                throw new ArgumentException($"Only 9*9 grids are supported.");
            }

        }


        internal IEnumerable<IEnumerable<(int, int)>> GetEmptySquaresInBoxes()
        {
            foreach (var (xBounds, yBounds) in Board.BoxBounds)
            {
                var candidatesFromBox = MapOfCandidates.Where(kvp =>
                {
                    var (x, y) = kvp.Key;
                    return xBounds.min <= x && x <= xBounds.max
                        && yBounds.min <= y && y <= yBounds.max;
                });

                if (candidatesFromBox.Any())
                {
                    yield return candidatesFromBox.Select(kvp => kvp.Key);
                }
            }
        }

        internal IEnumerable<IEnumerable<(int x, int y)>> GetEmptySquaresInRow()
        {
            for (int x = 0; x < 9; x++)
            {
                var candidatesFromRow = MapOfCandidates.Where(kvp => kvp.Key.x == x);
                if (candidatesFromRow.Any())
                {
                    yield return candidatesFromRow.Select(kvp => kvp.Key);
                }
            }
        }
        internal IEnumerable<IEnumerable<(int, int)>> GetEmptySquaresInColumn()
        {
            for (int y = 0; y < 9; y++)
            {
                var candidatesFromColumn = MapOfCandidates.Where(kvp => kvp.Key.y == y);

                if (candidatesFromColumn.Any())
                {
                    yield return candidatesFromColumn.Select(kvp => kvp.Key);
                }
            }
        }
        private void RecalculateDependentCandidates(int x, int y, int valueToRemove)
        {
            IEnumerable<(int x, int y)> dependentIndices = GetDependentIndices(x, y);

            foreach (var key in dependentIndices)
            {
                if (!MapOfCandidates.TryGetValue(key, out var candidatesList))
                {
                    continue;
                }
                if (candidatesList.Count == 1)
                {
                    var candidate = candidatesList[0];
                    this[key.x, key.y] = candidate;
                }
                candidatesList.Remove(valueToRemove);
            }
        }

        private List<(int x, int y)> GetDependentIndices(int x, int y)
        {
            if (MapOfPeers.TryGetValue((x, y), out var dependentIndices))
            {
                return dependentIndices;
            }
            else
            {
                // Box Bounds
                int xMin = 3 * (x / 3); int xMax = xMin + 2;
                int yMin = 3 * (y / 3); int yMax = yMin + 2;

                List<(int x, int y)> indices = new List<(int x, int y)>();
                //Fill box
                for (int xIterator = xMin; xIterator <= xMax; xIterator++)
                {
                    for (int yIterator = yMin; yIterator <= yMax; yIterator++)
                    {
                        if (x == xIterator && y == yIterator)
                        {
                            continue;
                        }
                        var index = (xIterator, yIterator);

                        indices.Add(index);

                    }
                }
                //Fill column
                for (int xIterator = 0; xIterator < 9; xIterator++)
                {
                    if (xMin <= xIterator  && xIterator <= xMax)
                    {
                        continue;
                    }
                    var index = (xIterator, y);
                   
                    indices.Add(index);
                    
                }
                // Fill row
                for (int yIterator = 0; yIterator < 9; yIterator++)
                {
                    if (yMin <= yIterator && yIterator <= yMax)
                    {
                        continue;
                    }
                    var index = (x, yIterator);
                    indices.Add(index);
                }
               

                MapOfPeers.Add((x, y), indices);
                dependentIndices = indices;
            }
            return dependentIndices;
        }



        internal IEnumerable<int> GetAllClues(int x, int y)
        {
            int xMin = 3 * (x / 3); int xMax = xMin + 2;
            int yMin = 3 * (y / 3); int yMax = yMin + 2;

            for (int i = xMin; i <= xMax; i++)
            {
                for (int j = yMin; j <= yMax; j++)
                {
                    if (Grid[i, j] != 0)
                    {
                        yield return Grid[i, j];
                    }
                }
            }
            for (int i = 0; i < 9; i++)
            {
                if (xMin <= i && xMax >= i)
                {
                    continue;
                }

                if (Grid[i, y] != 0)
                {
                    yield return Grid[i, y];
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (yMin <= i && yMax >= i)
                {
                    continue;
                }

                if (Grid[x, i] != 0)
                {
                    yield return Grid[x, i];
                }
            }

        }


        internal bool Validate()
        {
            if (MapOfCandidates.Values.Where(l => l.Count == 0).Any())
            {
                return false;
            }
            if (EmptySquaresCount > 0)
            {
                return true;
            }
           
            const int sumOfNumbersInRegion = 45;
            int sum = 0;
            foreach (var (xBounds, yBounds) in BoxBounds)
            {
                for (int x = xBounds.min; x <= xBounds.max; x++)
                {
                    for (int y = yBounds.min; y <= yBounds.max; y++)
                    {
                        sum += this[x, y];
                    }
                }
                if (sumOfNumbersInRegion != sum)
                {
                    return false;
                }
                sum = 0;
            }
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    sum += this[x, y];
                }
                if (sumOfNumbersInRegion != sum)
                {
                    return false;
                }
                sum = 0;
            }
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    sum+= this[x, y];
                }
                if (sumOfNumbersInRegion != sum)
                {
                    return false;
                }
                sum = 0;
            }

            return true;
        }

        internal void RemovePairs(IGrouping<(int, int), (int x, int y)> group, IEnumerable<(int x, int y)> emptySquaresFromRegion)
        {
            var values = group.Key;
            foreach (var (x, y) in group)
            {
                foreach (var key in emptySquaresFromRegion.Except(group))
                {
                    if (!MapOfCandidates.TryGetValue(key, out var candidatesList))
                    {
                        continue;
                    }
                    candidatesList.Remove(values.Item1);
                    candidatesList.Remove(values.Item2);

                }

            }
        }

        internal void SaveState()
        {
            GridStates.Push((int[,])Grid.Clone());
            var mapOfCandidatesCopy = MapOfCandidates.ToDictionary(kvp => kvp.Key, kvp => new List<int>(kvp.Value));
            MapOfCandidatesStates.Push(mapOfCandidatesCopy);
        }

        internal void RestoreState()
        {
            Grid = GridStates.Pop();
            MapOfCandidates = MapOfCandidatesStates.Pop();
        }
    }
}
