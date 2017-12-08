using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mancala
{
    class mcw33Player : Player
    {
        public Position position;
        public Tuple<int, int> offsets; // my offset is Item1, opponent offset is Item2
        public Dictionary<string, int> weights = new Dictionary<string, int>()
        {
            {"mancala", 5},     // Difference between mancalas
            {"pit", 2},         // Difference between number of stones on each side of board
            {"capture", 4},     // Difference between potential captures
            {"turn", 15}        // Potentially getting another turn
        };
        public List<string> gloats = new List<string>() // Some gloats
        {
            "Yes! I won!",
            "You Lose!",
            "Good game",
            "Hey, that was pretty good",
            "Nice try",
        };

        public mcw33Player(Position pos, int timeLimit) : base(pos, "mcw33", timeLimit)
        {
            position = pos;
            offsets = (position == Position.Top) ? Tuple.Create(7, 0) : Tuple.Create(0, 7);
        }

        public override String getImage() { return "https://vignette.wikia.nocookie.net/uncyclopedia/images/6/6b/Statue-Thinker.jpg/revision/latest?cb=20140403114710"; }

        public override string gloat() { return gloats[new Random().Next(gloats.Count)]; }

        public override int chooseMove(Board b)
        {
            // Create a token
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            // Run a task so that we can cancel from another thread.
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(getTimePerMove());
                cts.Cancel();
            });

            // Run the threads
            Tuple<int, int> res = Tuple.Create(-1, 0);
            try
            {
                Parallel.For(1, 100, po, i =>
                {
                    res = minimaxVal(b, i, int.MinValue, int.MaxValue, po);
                });
            }
            catch (OperationCanceledException) { }
            finally { cts.Dispose(); }
            return res.Item1;
        }

        public Tuple<int, int> minimaxVal(Board b, int d, int alpha, int beta, ParallelOptions po)
        {
            // Base case
            if (b.gameOver() || d == 0)
                return Tuple.Create(0, evaluate(b));

            // Cancel process if time is up
            po.CancellationToken.ThrowIfCancellationRequested();

            // Minimaxing
            bool myTurn = b.whoseMove() == position;
            int bestMove = -1;
            int offset = myTurn ? offsets.Item1 : offsets.Item2;
            int bestVal = myTurn ? int.MinValue : int.MaxValue;

            for (int move = 0 + offset; move <= 5 + offset; move++)
            {
                if (b.legalMove(move))
                {
                    Board b1 = new Board(b);
                    b1.makeMove(move, false);
                    Tuple<int, int> res = minimaxVal(b1, d - 1, alpha, beta, po);
                    if (miniMaxCompare(res.Item2, bestVal, myTurn))
                    {
                        bestMove = move;
                        bestVal = res.Item2;
                    }
                    // Update alpha or beta
                    if (myTurn) alpha = Math.Max(alpha, bestVal);
                    else beta = Math.Min(beta, bestVal);
                    // Prune the tree
                    if (beta <= alpha) break;
                }
            }

            return Tuple.Create(bestMove, bestVal);
        }

        public override int evaluate(Board b)
        {
            int eval = 0;
            // Check scores of mancalas
            eval += (b.stonesAt(6 + offsets.Item1) - b.stonesAt(6 + offsets.Item2)) * weights["mancala"];
            for (int i = 0; i <= 5; i++)
            {
                int myPit = i + offsets.Item1;
                int enemyPit = i + offsets.Item2;
                // Add all other conditions to the eval
                eval += ((b.stonesAt(myPit) - b.stonesAt(enemyPit)) * weights["pit"])
                     + ((checkCapture(b, myPit, offsets.Item1) - checkCapture(b, enemyPit, offsets.Item2)) * weights["capture"])
                     + ((goAgain(b, myPit, offsets.Item1) - goAgain(b, enemyPit, offsets.Item2)) * weights["turn"]);
            }
            return eval;
        }

        public int checkCapture(Board b, int pit, int offset)
        {
            // Check for potential capture next turn
            int endPit = (pit + b.stonesAt(pit)) % 13;
            return (b.stonesAt(endPit) != 0 && endPit >= 0 + offset && endPit <= 5 + offset) ? b.stonesAt(12 - endPit) : 0;
        }

        public int goAgain(Board b, int pit, int offset)
        {
            // Check for potential go again for next turn
            return (((pit + b.stonesAt(pit)) % 13) == 6 + offset) ? 1 : 0;
        }

        public bool miniMaxCompare(int score, int best, bool myTurn)
        {
            return myTurn ? score > best : score < best;
        }
    }
}

