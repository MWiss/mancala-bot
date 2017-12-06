/**************************************************************************
* Board.cs: a board for the game of Mankalah. 
*
* A board looks like this :
*
*        board[12] board[11] board[10] board[9] board[8] board[7]
*board[13]                                                     [board6]
*        board[0]  board[1]  board[2]  board[3] board[4] board[5]
*
* TOP player moves from locations 7..12 toward location 13. 
* BOTTOM moves from 0..5 toward 6.
*
**************************************************************************/
using System;

namespace Mancala
{
    public enum Position : byte { Top, Bottom, Invalid }

    public class Board
    {
        private Position playerToMove;
        public int[] board = new int[14];           // public for performance reasons, but
                                                    // really should be private....

        public Board(Position toMove) 				// create new board
        {
            for (int i = 0; i < 13; i++) board[i] = 4;
            board[6] = 0;
            board[13] = 0;
            playerToMove = toMove;
        }

        public Board(Board b) 					// copy constructor
        {
            for (int i = 0; i < 14; i++) board[i] = b.board[i];
            playerToMove = b.playerToMove;
        }

        public void copy(Board b)
        {
            for (int i = 0; i < 14; i++) board[i] = b.board[i];
            playerToMove = b.playerToMove;
        }

        public int stonesAt(int pos) { return board[pos]; }

        public Position whoseMove() { return playerToMove; }

        public bool legalMove(int move)
        {
            if (playerToMove == Position.Top && move >= 7 && move <= 12 &&
                board[move] != 0) return true;
            if (playerToMove == Position.Bottom && move >= 0 && move <= 5 &&
                board[move] != 0) return true;
            return false;
        }

        public bool gameOver()
        {
            if (board[0] == 0 && board[1] == 0 && board[2] == 0 &&
                board[3] == 0 && board[4] == 0 && board[5] == 0)
                return true;
            if (board[7] == 0 && board[8] == 0 && board[9] == 0 &&
                board[10] == 0 && board[11] == 0 && board[12] == 0)
                return true;
            return false;
        }

        public Position winner()
        {
            int player1Count = 0;
            for (int i = 7; i <= 13; i++) player1Count += board[i];
            int player2Count = 0;
            for (int i = 0; i <= 6; i++) player2Count += board[i];
            if (player1Count > player2Count) return Position.Top;
            if (player2Count > player1Count) return Position.Bottom;
            return Position.Invalid;
        }

        public int scoreTop()
        {
            int score = 0;
            for (int i = 7; i <= 13; i++) score += board[i];
            return score;
        }

        public int scoreBot()
        {
            int score = 0;
            for (int i = 0; i <= 6; i++) score += board[i];
            return score;
        }

        /*
         * makeMove modifies a board by making a move and updating playerToMove.
         * It returns the number of stones captured.  
         * If chatter is true, it prints informational messages.
         *
         * This function will exit with an error message if an illegal move is attempted.
         */
        public int makeMove(int move, bool chatter)
        {
            if (!legalMove(move))
            {
                string err = String.Format("Player {0} cheated! (Attempted illegal move {1})",
                                           playerToMove, move);
                Console.WriteLine(err);
                Console.Read();
                //Environment.Exit(1);
                throw new ArgumentException(err);
            }

            // pick up the stones
            int stones = board[move];
            board[move] = 0;
            int capture = 0;

            // distribute the stones
            int pos;
            for (pos = move + 1; stones > 0; pos++)
            {
                if (playerToMove == Position.Top && pos == 6) pos++;   // don't add stone to
                if (playerToMove == Position.Bottom && pos == 13) pos++;   // opponent's kalah
                if (pos == 14) pos = 0;
                board[pos]++;
                stones--;
            }
            pos--;

            // was there a capture by TOP? 
            if (playerToMove == Position.Top && pos > 6 && pos < 13 && board[pos] == 1 && board[12 - pos] > 0)
            {
                capture = board[12 - pos] + 1;
                board[13] += board[12 - pos];
                board[12 - pos] = 0;
                board[13]++;
                board[pos] = 0;
                if (chatter) Console.WriteLine("TOP captured {0} stones!", capture);
            }

            // was there a capture by BOTTOM? 
            if (playerToMove == Position.Bottom && pos >= 0 && pos < 6 && board[pos] == 1 && board[12 - pos] > 0)
            {
                capture = board[12 - pos] + 1;
                board[6] += board[12 - pos];
                board[12 - pos] = 0;
                board[6]++;
                board[pos] = 0;
                if (chatter) Console.WriteLine("BOTTOM captured {0} stones!", capture);
            }

            // who gets the next move?
            if (playerToMove == Position.Top)
            {
                if (pos != 13)
                    playerToMove = Position.Bottom;
                else
                    if (chatter) Console.WriteLine("TOP goes again.");
            }
            else
                if (pos != 6)
                playerToMove = Position.Top;
            else
                    if (chatter) Console.WriteLine("BOTTOM goes again.");

            return capture;
        }

        /* 
         * Display the kalah board. (Should this be graphical?)
         */
        public void display()
        {
            Console.Write("\n    ");
            for (int i = 12; i >= 7; i--)
                Console.Write(board[i] + "  ");
            Console.WriteLine("");
            Console.WriteLine(board[13] + "                     " + board[6]);
            Console.Write("    ");
            for (int i = 0; i <= 5; i++)
                Console.Write(board[i] + "  ");
            Console.WriteLine("");
        }
    }
}
