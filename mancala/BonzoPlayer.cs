using System;

namespace mancala
{
    /*****************************************************************/
    /*
    /* A Dumb Mankalah player.  This player always takes the first
    /* first available go-again, if there is one. If not, it takes 
    /* the first available move.
    /*
    /*****************************************************************/
    public class BonzoPlayer : Player
    {

        public BonzoPlayer(Position pos, int timeLimit) : base(pos, "Bonzo", timeLimit) { }

        public override string gloat()
        {
            return "I WIN! YOU'RE DUMBER THAN BONZO!!";
        }

        public override int chooseMove(Board b)
        {
            if (b.whoseMove() == Position.Top)
            {
                for (int i = 12; i >= 7; i--)               // try first go-again
                    if (b.stonesAt(i) == 13 - i) return i;
                for (int i = 12; i >= 7; i--)               // otherwise, first
                    if (b.stonesAt(i) > 0) return i;        // available move
            }
            else
            {
                for (int i = 5; i >= 0; i--)
                    if (b.stonesAt(i) == 6 - i) return i;
                for (int i = 5; i >= 0; i--)
                    if (b.stonesAt(i) > 0) return i;
            }
            return -1;		        // an illegal move if there aren't any legal ones
        }			        // this can't happen unless game is over

        public override String getImage() { return "Bonzo.png"; }

    }
}
