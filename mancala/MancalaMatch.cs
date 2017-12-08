using System;

namespace Mancala
{
    /********************************************************************/
    /* This class creates two Players and runs a pair of Kalah games,
    /* one with each player starting. The match score is reported.
    /********************************************************************/

    public class MancalaMatch
    {
        private static int timeLimit = 4000;						                // turn time in msec

        private static Player pTop = new BonzoPlayer(Position.Top, timeLimit);	// TOP player (MAX)
        private static Player pBot = new mcw33Player(Position.Bottom, timeLimit);	// BOTTOM player	
        private static Board b;			                                // playing surface
        private static int move;


        /* 
         * Play one Kalah game with the two given players, with firstPlayer
         * starting. This function returns TOP's score.
         */
        public static int playGame(Player pTop, Player pBot, Position firstPlayer)
        {
            b = new Board(firstPlayer);

            if (firstPlayer == Position.Top)
                Console.WriteLine("Player " + pTop.getName() + " starts.");
            else
                Console.WriteLine("Player " + pBot.getName() + " starts.");

            b.display();

            while (!b.gameOver())
            {
                Console.WriteLine();
                if (b.whoseMove() == Position.Top)
                {
                    move = pTop.chooseMove(b);
                    Console.WriteLine(pTop.getName() + " chooses move " + move);
                }
                else
                {
                    move = pBot.chooseMove(b);
                    Console.WriteLine(pBot.getName() + " chooses move " + move);
                }

                b.makeMove(move, true);		// last parameter says to be chatty
                b.display();

                if (b.gameOver())
                {
                    if (b.winner() == Position.Top)
                        Console.WriteLine("Player " + pTop.getName() +
                        " (TOP) wins " + b.scoreTop() + " to " + b.scoreBot());
                    else if (b.winner() == Position.Bottom)
                        Console.WriteLine("Player " + pBot.getName() +
                        " (BOTTOM) wins " + b.scoreBot() + " to " + b.scoreTop());
                    else Console.WriteLine("A tie!");
                }
                else
                    if (b.whoseMove() == Position.Top)
                    Console.WriteLine(pTop.getName() + " to move.");
                else
                    Console.WriteLine(pBot.getName() + " to move.");
            }
            return b.scoreTop();
        }

        public static void Main(String[] args)
        {
            int topScore;

            Console.WriteLine("\n================ Game 1 ================");
            topScore = playGame(pTop, pBot, Position.Bottom);

            Console.WriteLine("\n================ Game 2 ================");
            topScore += playGame(pTop, pBot, Position.Top);

            Console.WriteLine("\n========================================");
            Console.Write("Match result: ");

            int botScore = 96 - topScore;
            if (topScore > 48)
            {
                Console.WriteLine(pTop.getName() + " wins " + topScore + " to " + botScore);
                pTop.gloat();
            }
            else if (botScore > 48)
            {
                Console.WriteLine(pBot.getName() + " wins " + botScore + " to " + topScore);
                pBot.gloat();
            }
            else
                Console.WriteLine("Match was a tie, 48-48!");

            Console.Read();
        }
    }
}
