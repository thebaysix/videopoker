using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPoker
{
    /// <summary>
    /// Subclass of Hand where handsize is less than five. Offers some
    /// overriding "flush" and "straight" checks.
    /// 
    /// TODO: Merge the "Is" functions here into Hand.cs by generalizing those Hand functions
    /// (assuming we can do so without sacrificing performance).
    /// </summary>
    internal class Subhand : Hand
    {
        public bool[] Holds { get; private set; }

        public Subhand(Card[] cards, bool[] holds)
            : base(cards)
        {
            this.Holds = holds;

            if (cards.Length < 1 || cards.Length > 4)
                throw new ArgumentException($"{nameof(Subhand)} Cards must be length 1-4.");
        }

        /// <summary>
        /// If the N cards that make up this subhand represent N to a RoyalFlush.
        /// </summary>
        /// <param name="isTenAce3RF">Checks for 3 to a royal that contains Ten and Ace.
        /// This can be important for certain exception scenarios.</param>
        /// <param name="suit">The suit that the N to the royal is in.
        /// This can be important for certain exception scenarios.</param>
        /// <param name="orcRank">The rank of the "other royal card" (ORC) in Ten-Ace scenarios.
        /// This can be important for certain exception scenarios.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsNToRoyalFlush(out bool isTenAce3RF, out int suit, out Rank orcRank)
        {
            isTenAce3RF = false;
            suit = -1;
            orcRank = Rank.Two; // Some default

            bool hasT = false, hasJ = false, hasQ = false, hasK = false, hasA = false;
            foreach (var card in this.Cards)
            {
                if (suit == -1) // If first card, set suit.
                    suit = card.Suit;
                else if (suit != card.Suit) // If the suit changed, this isn't a 4RF.
                    break;

                // Check off the box for this rank.
                hasT |= (card.Rank == Rank.Ten);
                hasJ |= (card.Rank == Rank.Jack);
                hasQ |= (card.Rank == Rank.Queen);
                hasK |= (card.Rank == Rank.King);
                hasA |= (card.Rank == Rank.Ace);
            }

            if (Cards.Length == 4)
            {
                // TJQA; TJQK; TJKA; TQKA; JQKA
                // After going through the row, if one of the above is there, we have a 4RF.
                return 
                    ((hasT && hasJ && hasQ && hasA) ||
                    (hasT && hasJ && hasQ && hasK) ||
                    (hasT && hasJ && hasK && hasA) ||
                    (hasT && hasQ && hasK && hasA) ||
                    (hasJ && hasQ && hasK && hasA));
            }
            else if (Cards.Length == 3)
            {
                // TJA; TJQ; TJK; TQA; TQK; TKA; JQA; JQK; JKA; QKA
                // After going through the row, if one of the above is there, we have a 3RF.
                if (hasT && hasA)
                {
                    isTenAce3RF = true;
                    if (hasJ)
                        orcRank = Rank.Jack;
                    else if (hasQ)
                        orcRank = Rank.Queen;
                    else if (hasK)
                        orcRank = Rank.King;
                }

                return
                    ((hasT && hasJ && hasA) ||
                    (hasT && hasJ && hasQ) ||
                    (hasT && hasJ && hasK) ||
                    (hasT && hasQ && hasA) ||
                    (hasT && hasQ && hasK) ||
                    (hasT && hasK && hasA) ||
                    (hasJ && hasQ && hasA) ||
                    (hasJ && hasQ && hasK) ||
                    (hasJ && hasK && hasA) ||
                    (hasQ && hasK && hasA));
            }
            else if (Cards.Length == 2)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// If the N cards that make up this subhand represent N to a StraightFlush.
        /// </summary>
        /// <param name="threeToSFType2SpreadFiveWithOneHC">Checks for a very specific situation, namely
        /// if there's 3 cards to a SF (type 2), spread 5, with 1 High Card.
        /// Important for exception scenario.</param>
        /// <param name="lowestRank">The lowest rank, only well-defined in the 32SFT2S5W1HC scenario.
        /// Important for exception scenario.</param>
        /// <param name="oscRank">The other suited rank (not the lowest or the highest), only well-defined in the 32SFT2S5W1HC scenario.
        /// Important for exception scenario.</param>
        /// <param name="type">Type 1,2,3 of straight flush to check for</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsNToStraightFlush(
            out bool threeToSFType2SpreadFiveWithOneHC,
            out Rank lowestRank,
            out Rank oscRank,
            StraightFlushType type = StraightFlushType.None)
        {
            lowestRank = Rank.Two; // Some default
            oscRank = Rank.Two; // Some default
            threeToSFType2SpreadFiveWithOneHC = false;
            if (!this.IsAllSameSuit())
                return false;

            if (Cards.Length == 4)
            {
                //// A234; A235; A245; A345; 2345; 2346; 2356; 2456; 3456; 3457; 3467; 3567;
                //// 4567; 4568; 4578; 4678; 5678; 5679; 5689; 5789; 6789; 678T; 679T; 689T;
                //// 789T; 789J; 78TJ; 79TJ; 89TJ; 89TQ; 89JQ; 8TJQ; 9TJQ; 9TJK; 9TQK; 9JQK

                // Look for no-gap 4ToSF patterns.
                if ((Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Four)) ||
                    (Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Five)) ||
                    (Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Six)) ||
                    (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Seven)) ||
                    (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                    (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                    (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Ten)) ||
                    (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                    (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.Queen)))
                    return true;

                // Look for 1_345 4ToSF patterns.
                if ((Has(Rank.Ace) && Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Five)) ||
                    (Has(Rank.Two) && Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Six)) ||
                    (Has(Rank.Three) && Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Seven)) ||
                    (Has(Rank.Four) && Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                    (Has(Rank.Five) && Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                    (Has(Rank.Six) && Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Ten)) ||
                    (Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                    (Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.Queen)) ||
                    (Has(Rank.Nine) && Has(Rank.Jack) && Has(Rank.Queen) && Has(Rank.King)))
                    return true;

                // Look for 12_45 4ToSF patterns.
                if ((Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Four) && Has(Rank.Five)) ||
                    (Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Five) && Has(Rank.Six)) ||
                    (Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Six) && Has(Rank.Seven)) ||
                    (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                    (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                    (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Ten)) ||
                    (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                    (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Jack) && Has(Rank.Queen)) ||
                    (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Queen) && Has(Rank.King)))
                    return true;

                // Look for 123_5 4ToSF patterns.
                if ((Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Five)) ||
                    (Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Six)) ||
                    (Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Seven)) ||
                    (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Eight)) ||
                    (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Nine)) ||
                    (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Ten)) ||
                    (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Jack)) ||
                    (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Queen)) ||
                    (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.King)))
                    return true;

                return false;
            }
            else if (Cards.Length == 3)
            {
                if (type == StraightFlushType.T1)
                {
                    // 345; 456; 567; 678; 789; 89T; 89J; 8TJ; 8JQ; 9TJ; 9TQ; 9JQ; 9JK; 9QK
                    return
                        ((Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Five)) ||
                        (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Six)) ||
                        (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Seven)) ||
                        (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                        (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                        (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Ten)) ||
                        (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Jack)) ||
                        (Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                        (Has(Rank.Eight) && Has(Rank.Jack) && Has(Rank.Queen)) ||
                        (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                        (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.Queen)) ||
                        (Has(Rank.Nine) && Has(Rank.Jack) && Has(Rank.Queen)) ||
                        (Has(Rank.Nine) && Has(Rank.Jack) && Has(Rank.King)) ||
                        (Has(Rank.Nine) && Has(Rank.Queen) && Has(Rank.King)));
                }
                else if (type == StraightFlushType.T2)
                {
                    // Be sure to mark threeToSFType2SpreadFiveWithOneHC (abbr. 32SFT2S5W1HC)
                    // A23; A24; A25 (32SFT2S5W1HC); A34; A35 (32SFT2S5W1HC); A45 (32SFT2S5W1HC); 234; 235; 245; 346; 356; 457; 467; 568; 578;
                    // 679; 689; 78T; 78J (32SFT2S5W1HC); 79T; 79J (32SFT2S5W1HC); 7TJ (32SFT2S5W1HC); 89Q (32SFT2S5W1HC); 8TQ (32SFT2S5W1HC); 9TK (32SFT2S5W1HC)

                    threeToSFType2SpreadFiveWithOneHC = 
                        (Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Three) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Four) && Has(Rank.Five)) ||
                        (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                        (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Queen)) ||
                        (Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Queen)) ||
                        (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.King));

                    // In the 32SFT2S5W1HC scenario, we need to know the lowestRank and the oscRank as well.
                    if ((Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Three) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Four) && Has(Rank.Five)))
                    {
                        lowestRank = Rank.Ace;
                        if (Has(Rank.Two))
                            oscRank = Rank.Two;
                        else if (Has(Rank.Three))
                            oscRank = Rank.Three;
                        else
                            oscRank = Rank.Four;
                    }

                    if ((Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Ten) && Has(Rank.Jack)))
                    {
                        lowestRank = Rank.Seven;
                        if (Has(Rank.Eight))
                            oscRank = Rank.Eight;
                        else if (Has(Rank.Nine))
                            oscRank = Rank.Nine;
                        else
                            oscRank = Rank.Ten;
                    }

                    if ((Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Queen)) ||
                        (Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Queen)))
                    {
                        lowestRank = Rank.Eight;
                        if (Has(Rank.Nine))
                            oscRank = Rank.Nine;
                        else
                            oscRank = Rank.Ten;
                    }

                    if ((Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.King)))
                    {
                        lowestRank = Rank.Nine;
                        oscRank = Rank.Ten;
                    }

                    return
                        ((Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Three)) ||
                        (Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Four)) ||
                        (Has(Rank.Ace) && Has(Rank.Two) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Three) && Has(Rank.Four)) ||
                        (Has(Rank.Ace) && Has(Rank.Three) && Has(Rank.Five)) ||
                        (Has(Rank.Ace) && Has(Rank.Four) && Has(Rank.Five)) ||
                        (Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Four)) ||
                        (Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Five)) ||
                        (Has(Rank.Two) && Has(Rank.Four) && Has(Rank.Six)) ||
                        (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Seven)) ||
                        (Has(Rank.Four) && Has(Rank.Six) && Has(Rank.Seven)) ||
                        (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Eight)) ||
                        (Has(Rank.Five) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                        (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Nine)) ||
                        (Has(Rank.Six) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                        (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Ten)) ||
                        (Has(Rank.Seven) && Has(Rank.Eight) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Ten)) ||
                        (Has(Rank.Seven) && Has(Rank.Nine) && Has(Rank.Jack)) ||
                        (Has(Rank.Seven) && Has(Rank.Ten) && Has(Rank.Jack)) ||
                        (Has(Rank.Eight) && Has(Rank.Nine) && Has(Rank.Queen)) ||
                        (Has(Rank.Eight) && Has(Rank.Ten) && Has(Rank.Queen)) ||
                        (Has(Rank.Nine) && Has(Rank.Ten) && Has(Rank.King)));
                }
                else if (type == StraightFlushType.T3)
                {
                    // 236; 246; 256; 347; 357; 367; 458; 468; 478; 569; 579; 589; 67T; 68T; 69T
                    return
                        ((Has(Rank.Two) && Has(Rank.Three) && Has(Rank.Six)) ||
                        (Has(Rank.Two) && Has(Rank.Four) && Has(Rank.Six)) ||
                        (Has(Rank.Two) && Has(Rank.Five) && Has(Rank.Six)) ||
                        (Has(Rank.Three) && Has(Rank.Four) && Has(Rank.Seven)) ||
                        (Has(Rank.Three) && Has(Rank.Five) && Has(Rank.Seven)) ||
                        (Has(Rank.Three) && Has(Rank.Six) && Has(Rank.Seven)) ||
                        (Has(Rank.Four) && Has(Rank.Five) && Has(Rank.Eight)) ||
                        (Has(Rank.Four) && Has(Rank.Six) && Has(Rank.Eight)) ||
                        (Has(Rank.Four) && Has(Rank.Seven) && Has(Rank.Eight)) ||
                        (Has(Rank.Five) && Has(Rank.Six) && Has(Rank.Nine)) ||
                        (Has(Rank.Five) && Has(Rank.Seven) && Has(Rank.Nine)) ||
                        (Has(Rank.Five) && Has(Rank.Eight) && Has(Rank.Nine)) ||
                        (Has(Rank.Six) && Has(Rank.Seven) && Has(Rank.Ten)) ||
                        (Has(Rank.Six) && Has(Rank.Eight) && Has(Rank.Ten)) ||
                        (Has(Rank.Six) && Has(Rank.Nine) && Has(Rank.Ten)));
                }
                else
                {
                    throw new Exception("Invalid StraightFlushType.");
                }
            }
            else if (Cards.Length == 2)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool IsUnsuitedTJQK()
        {
            // TJQK

            return this.Cards.Length == 4 && Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.Queen) && Has(Rank.King);
        }
        public bool IsUnsuitedRanks(Rank r1, Rank r2, Rank r3)
        {
            return this.Cards.Length == 3 && Has(r1) && Has(r2) && Has(r3);
        }

        public bool IsUnsuitedRanks(Rank r1, Rank r2)
        {
            return this.Cards.Length == 2 && Has(r1) && Has(r2);
        }

        public bool IsSuitedRanks(out int suit, Rank r1, Rank r2)
        {
            suit = this.Cards[0].Suit;
            return this.Cards.Length == 2 && Has(r1) && Has(r2) && this.Cards[0].Suit == this.Cards[1].Suit;
        }

        public bool IsNToInsideStraight(int highCardTarget)
        {
            if (this.Cards.Length == 4)
            {
                if (highCardTarget == 4)
                {
                    // JQKA
                    return Has(Rank.Jack) && Has(Rank.Queen) && Has(Rank.King) && Has(Rank.Ace);
                }
                else if (highCardTarget == 3)
                {
                    // 9JQK; TJQA; TJKA; TQKA
                    return
                        ((Has(Rank.Nine) && Has(Rank.Jack) && Has(Rank.Queen) && Has(Rank.King)) ||
                        (Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.Queen) && Has(Rank.Ace)) ||
                        (Has(Rank.Ten) && Has(Rank.Jack) && Has(Rank.King) && Has(Rank.Ace)) ||
                        (Has(Rank.Ten) && Has(Rank.Queen) && Has(Rank.King) && Has(Rank.Ace)));
                }
            }

            throw new Exception("Never checked in optimal play.");
        }

        internal bool IsNToOutideStraight()
        {
            // 2345; 3456; 4567; 5678; 6789; 789T; 89TJ; 9TJQ

            var sortedCards = this.GetSortedCards();
            var lowestRank = sortedCards[0].Rank;
            if (lowestRank == Rank.Ace || lowestRank == Rank.Ten || lowestRank == Rank.Jack || lowestRank == Rank.Queen || lowestRank == Rank.King)
                return false;

            for (int i = 1; i < sortedCards.Length; i++)
                if (sortedCards[i].Rank != (Rank)(lowestRank + i))
                    return false;

            return true;
        }

        public bool IsAllSameSuit()
        {
            var suit = this.Cards[0].Suit;
            return this.Cards.All(c => c.Suit == suit);
        }
    }
}
