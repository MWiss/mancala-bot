using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mancala
{
    class mcw33Player : Player
    {
        private Position position;
        private Tuple<int, int> offsets; // my offset is Item1, opponent offset is Item2

        public mcw33Player(Position pos, int timeLimit) : base(pos, "Bot", timeLimit)
        {
            position = pos;
            offsets = (position == Position.Top) ? Tuple.Create(7, 0) : Tuple.Create(0, 7);
        }

        public override String getImage() { return "https://vignette.wikia.nocookie.net/uncyclopedia/images/6/6b/Statue-Thinker.jpg/revision/latest?cb=20140403114710"; }

        public override string gloat()
        {
            return "You lose!";
        }

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

            Console.WriteLine("Best score: {0}", res.Item2);
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
            eval += b.stonesAt(offsets.Item1 + 6) - b.stonesAt(offsets.Item2 + 6);
            // Hog pieces
            for (int i = 0; i <= 5; i++)
            {
                eval += (b.board[i + offsets.Item1] - b.board[i + offsets.Item2]) * 2;
            }
            return eval;
        }

        public bool miniMaxCompare(int score, int best, bool myTurn)
        {
            return myTurn ? score > best : score < best;
        }
    }
}

