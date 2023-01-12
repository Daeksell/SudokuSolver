using System.Data;

namespace SudokuSolver
{
    public class Solver
    {

        public Board Board { get; private set; } = new Board();



        public int[,] Solve(int[,] puzzle)
        {
            Board = new Board((int[,])puzzle.Clone());
            ApplyRulesThenSearch();

            return Board.Grid;
        }

        private bool ApplyRulesThenSearch()
        {
            while (true)
            {
                if (ApplyNakedSingle())
                { continue; }

                if (ApplyHiddenSingle())
                { continue; }

                if (ApplyNakedPair())
                { continue; }

                // we could utilize more methods before resorting to search

                return Search();


            }
        }

        private bool Search()
        {
            if (!Board.Validate())
            {
                return false;
            }
            if (Board.EmptySquaresCount == 0)
            {
                return true;
            }
            var minCandidatesCount = Board.MapOfCandidates.Min(kvp => kvp.Value.Count);
            var squareWithMinCandidates = Board.MapOfCandidates.First(kvp => kvp.Value.Count == minCandidatesCount);

            var index = squareWithMinCandidates.Key;
            var candidatesList = squareWithMinCandidates.Value.ToList();
            foreach (var candidate in candidatesList)
            {
                Board.SaveState();
                Board.MapOfCandidates[index].RemoveAll(x => x != candidate);
                if (ApplyRulesThenSearch())
                {
                    return true;
                }
                else
                {
                    Board.RestoreState();
                }
            }

            return false;
        }

        private bool ApplyNakedPair()
        {
            if (Board.EmptySquaresCount == 0)
            {
                return false;
            }
            int candidatesCountBeforeApply = Board.MapOfCandidates.Values.Sum(candidates => candidates.Count);
            foreach (var emptySquaresInBox in Board.GetEmptySquaresInBoxes())
            {
                FindPairInRegion(emptySquaresInBox);
            }
            foreach (var emptySquaresInRow in Board.GetEmptySquaresInRow())
            {
                FindPairInRegion(emptySquaresInRow);
            }
            foreach (var emptySquaresInColumn in Board.GetEmptySquaresInColumn())
            {
                FindPairInRegion(emptySquaresInColumn);
            }
            int candidatesAfterApply = Board.MapOfCandidates.Values.Sum(candidates => candidates.Count);
            return candidatesAfterApply < candidatesCountBeforeApply;
        }

        private bool ApplyNakedSingle()
        {
            int emptySquaresBeforeApply = Board.EmptySquaresCount;
            if (emptySquaresBeforeApply == 0)
            {
                return false;
            }
            var singleCandidates = Board.MapOfCandidates.Where(kvp => kvp.Value.Count == 1).ToArray();

            foreach (var (index, values) in singleCandidates)
            {

                if (values.Count != 1)
                {
                    return false;
                }
                int value = values[0];
                Board[index.x, index.y] = value;
            }


            return emptySquaresBeforeApply > Board.EmptySquaresCount && Board.EmptySquaresCount > 0;

        }

        private bool ApplyHiddenSingle()
        {
            int emptySquaresBeforeApply = Board.EmptySquaresCount;
            if (emptySquaresBeforeApply == 0)
            {
                return false;
            }
            //Check for hidden Single in each region
            foreach (var candidatesFromBox in Board.GetEmptySquaresInBoxes())
            {
                FindSinglesInRegion(candidatesFromBox);
            }

            foreach (var candidatesFromRow in Board.GetEmptySquaresInRow())
            {
                FindSinglesInRegion(candidatesFromRow);
            }

            foreach (var candidatesFromColumn in Board.GetEmptySquaresInColumn())
            {
                FindSinglesInRegion(candidatesFromColumn);
            }

            return emptySquaresBeforeApply > Board.EmptySquaresCount && Board.EmptySquaresCount > 0;
        }

        private void FindSinglesInRegion(IEnumerable<(int x, int y)> emptySquaresInRegion)
        {
            Dictionary<int, int> candidatesCount = CountCandidates(emptySquaresInRegion);

            if (candidatesCount.Where(kvp => kvp.Value == 1).Count() != 1)
            {
                return;
            }

            int value = candidatesCount.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).Single();
            var indexEnumerator = emptySquaresInRegion.Where(index => Board.MapOfCandidates[index].Contains(value));
            if (!indexEnumerator.Any())
            {
                return;
            }
            var (x, y) = indexEnumerator.Single();
            Board[x, y] = value;

        }

        private void FindPairInRegion(IEnumerable<(int x, int y)> emptySqauresInRegion)
        {
            var candidatesInRegion = emptySqauresInRegion.Select(index => (index, candidates: Board.MapOfCandidates[index]));
            var pairs = candidatesInRegion.Where(x => x.candidates.Count == 2)
                                            .GroupBy(x => (x.candidates[0], x.candidates[1]), x => x.index)
                                            .Where(group => group.Count() == 2);

            foreach (var group in pairs)
            {
                Board.RemovePairs(group, emptySqauresInRegion);
            }
        }
        private Dictionary<int, int> CountCandidates(IEnumerable<(int x, int y)> emptySquaresInRegion)
        {
            var candidatesCount = new Dictionary<int, int>(9);
            foreach (var index in emptySquaresInRegion)
            {
                Board.MapOfCandidates[index].ForEach(value =>
                {
                    if (candidatesCount.ContainsKey(value))
                    {
                        candidatesCount[value]++;
                    }
                    else
                    {
                        candidatesCount.Add(value, 1);
                    }
                });
            }

            return candidatesCount;
        }




    }
}
