using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPoker
{
    internal static class LegacyHelpers
    {
        public static bool Get4ToRF(Card[] cards, out bool[] holds)
        {
            Card[,] fourCardCombos =
            {
                { cards[0], cards[1], cards[2], cards[3] },
                { cards[0], cards[1], cards[2], cards[4] },
                { cards[0], cards[1], cards[3], cards[4] },
                { cards[0], cards[2], cards[3], cards[4] },
                { cards[1], cards[2], cards[3], cards[4] }
            };

            int iContaining4RF = -1;
            for (int i = 0; i < fourCardCombos.GetLength(0); i++)
            {
                // Go through the current row of 4 Cards.
                int suit = -1;
                bool hasT = false, hasJ = false, hasQ = false, hasK = false, hasA = false;
                for (int j = 0; j < fourCardCombos.GetLength(1); j++)
                {
                    var current = fourCardCombos[i, j];
                    if (j == 0) // If first card, set suit.
                        suit = current.Suit;
                    else if (suit != current.Suit) // If the suit changed, this isn't a 4RF.
                        break;

                    // Check off the box for this rank.
                    hasT |= (current.Rank == Rank.Ten);
                    hasJ |= (current.Rank == Rank.Jack);
                    hasQ |= (current.Rank == Rank.Queen);
                    hasK |= (current.Rank == Rank.King);
                    hasA |= (current.Rank == Rank.Ace);
                }

                // After going through the row, if one of TJQA; TJQK; TJKA; TQKA; JQKA is there, we have a 4RF.
                if ((hasT && hasJ && hasQ && hasA) ||
                    (hasT && hasJ && hasQ && hasK) ||
                    (hasT && hasJ && hasK && hasA) ||
                    (hasT && hasQ && hasK && hasA) ||
                    (hasJ && hasQ && hasK && hasA))
                {
                    iContaining4RF = i;
                    break;
                }
            }

            holds = new bool[5] { false, false, false, false, false };
            if (iContaining4RF == -1)
                return false;

            if (iContaining4RF == 0)
                holds = new bool[5] { true, true, true, true, false };
            else if (iContaining4RF == 1)
                holds = new bool[5] { true, true, true, false, true };
            else if (iContaining4RF == 2)
                holds = new bool[5] { true, true, false, true, true };
            else if (iContaining4RF == 3)
                holds = new bool[5] { true, false, true, true, true };
            else if (iContaining4RF == 4)
                holds = new bool[5] { false, true, true, true, true };

            return true;
        }
    }
}
