using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPoker
{
    /// <summary>
    /// Class representing a set of five Cards and the qualities those Cards may possess.
    /// 
    /// Functions come in two varieties: Is*** and Get***. "Is" functions are simple boolean checks to
    /// see if this subhand has that quality (e.g., IsStraight, IsTwoPair, etc...). "Get" functions on the other subhand
    /// retrieve a set of "holds" (indicating the positions of the matching Cards) that make up the minimum hold
    /// to maintain that quality (e.g., Get3oK provides the positions of the three Cards of the same rank).
    /// "Get" functions are more expensive than "Is" functions, because they need to do extra work to determine
    /// the positions. "Is" functions have the luxury of using statistics (like card counts) to give their answers,
    /// and so are much faster. If either will do, use the "Is" function.
    /// </summary>
    internal class Hand
    {
        private readonly bool[] DefaultHolds = new bool[] { false, false, false, false, false };
        private Lazy<Subhand[]> FourCardCombos;
        private Lazy<Subhand[]> ThreeCardCombos;
        private Lazy<Subhand[]> TwoCardCombos;
        private Lazy<Subhand[]> Singles;

        public readonly Card[] Cards;
        protected readonly int[] CountOfRank;
        public Hand(Card[] cards)
        {
            if (cards.Length > 5)
                throw new ArgumentException("Hand size must be at most 5.");

            this.Cards = cards;
            this.CountOfRank = new int[13] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (Card c in cards)
            {
                CountOfRank[(int)c.Rank]++;
            }
            this.FourCardCombos = new Lazy<Subhand[]>(() =>
                new Subhand[5]
                {
                    new Subhand(
                        new Card[4] { this.Cards[0], this.Cards[1], this.Cards[2], this.Cards[3] },
                        new bool[5] { true, true, true, true, false }),
                    new Subhand(
                        new Card[4] { this.Cards[0], this.Cards[1], this.Cards[2], this.Cards[4] },
                        new bool[5] { true, true, true, false, true }),
                    new Subhand(
                        new Card[4] { this.Cards[0], this.Cards[1], this.Cards[3], this.Cards[4] },
                        new bool[5] { true, true, false, true, true }),
                    new Subhand(
                        new Card[4] { this.Cards[0], this.Cards[2], this.Cards[3], this.Cards[4] },
                        new bool[5] { true, false, true, true, true }),
                    new Subhand(
                        new Card[4] { this.Cards[1], this.Cards[2], this.Cards[3], this.Cards[4] },
                        new bool[5] { false, true, true, true, true })
                });
            this.ThreeCardCombos = new Lazy<Subhand[]>(() =>
                new Subhand[10]
                {
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[1], this.Cards[2] },
                        new bool[5] { true, true, true, false, false }),
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[1], this.Cards[3] },
                        new bool[5] { true, true, false, true, false }),
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[1], this.Cards[4] },
                        new bool[5] { true, true, false, false, true }),
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[2], this.Cards[3] },
                        new bool[5] { true, false, true, true, false }),
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[2], this.Cards[4] },
                        new bool[5] { true, false, true, false, true }),
                    new Subhand(
                        new Card[3] { this.Cards[0], this.Cards[3], this.Cards[4] },
                        new bool[5] { true, false, false, true, true }),
                    new Subhand(
                        new Card[3] { this.Cards[1], this.Cards[2], this.Cards[3] },
                        new bool[5] { false, true, true, true, false }),
                    new Subhand(
                        new Card[3] { this.Cards[1], this.Cards[2], this.Cards[4] },
                        new bool[5] { false, true, true, false, true }),
                    new Subhand(
                        new Card[3] { this.Cards[1], this.Cards[3], this.Cards[4] },
                        new bool[5] { false, true, false, true, true }),
                    new Subhand(
                        new Card[3] { this.Cards[2], this.Cards[3], this.Cards[4] },
                        new bool[5] { false, false, true, true, true })
                });
            this.TwoCardCombos = new Lazy<Subhand[]>(() =>
                new Subhand[10]
                {
                    new Subhand(
                        new Card[2] { this.Cards[0], this.Cards[1] },
                        new bool[5] { true, true, false, false, false }),
                    new Subhand(
                        new Card[2] { this.Cards[0], this.Cards[2] },
                        new bool[5] { true, false, true, false, false }),
                    new Subhand(
                        new Card[2] { this.Cards[0], this.Cards[3] },
                        new bool[5] { true, false, false, true, false }),
                    new Subhand(
                        new Card[2] { this.Cards[0], this.Cards[4] },
                        new bool[5] { true, false, false, false, true }),
                    new Subhand(
                        new Card[2] { this.Cards[1], this.Cards[2] },
                        new bool[5] { false, true, true, false, false }),
                    new Subhand(
                        new Card[2] { this.Cards[1], this.Cards[3] },
                        new bool[5] { false, true, false, true, false }),
                    new Subhand(
                        new Card[2] { this.Cards[1], this.Cards[4] },
                        new bool[5] { false, true, false, false, true }),
                    new Subhand(
                        new Card[2] { this.Cards[2], this.Cards[3] },
                        new bool[5] { false, false, true, true, false }),
                    new Subhand(
                        new Card[2] { this.Cards[2], this.Cards[4] },
                        new bool[5] { false, false, true, false, true }),
                    new Subhand(
                        new Card[2] { this.Cards[3], this.Cards[4] },
                        new bool[5] { false, false, false, true, true }),
                });
            this.Singles = new Lazy<Subhand[]>(() =>
                new Subhand[5]
                {
                    new Subhand(
                        new Card[1] { this.Cards[0] },
                        new bool[5] { true, false, false, false, false }),
                    new Subhand(
                        new Card[1] { this.Cards[1]},
                        new bool[5] { false, true, false, false, false }),
                    new Subhand(
                        new Card[1] { this.Cards[2] },
                        new bool[5] { false, false, true, false, false }),
                    new Subhand(
                        new Card[1] { this.Cards[3] },
                        new bool[5] { false, false, false, true, false }),
                    new Subhand(
                        new Card[1] { this.Cards[4] },
                        new bool[5] { false, false, false, false, true })
                });
        }

        // Begin "Is" Functions Section.

        public virtual bool IsRoyalFlush()
        {
            var suit = this.Cards[0].Suit;
            if (this.Cards[1].Suit == suit &&
                this.Cards[2].Suit == suit &&
                this.Cards[3].Suit == suit &&
                this.Cards[4].Suit == suit)
            {
                var sortedCards = this.GetSortedCards();
                if (sortedCards[0].Rank == Rank.Ace &&
                    sortedCards[1].Rank == Rank.Ten &&
                    sortedCards[2].Rank == Rank.Jack &&
                    sortedCards[3].Rank == Rank.Queen &&
                    sortedCards[4].Rank == Rank.King)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFourOfAKind()
        {
            foreach (int count in this.CountOfRank)
                if (count == 4)
                    return true;

            return false;
        }

        public bool IsFullHouse()
        {
           bool hasThreeOfAKind = false;
           bool hasPair = false;
            foreach (int count in this.CountOfRank)
            {
                if (count == 3)
                {
                    if (hasPair)
                    {
                        return true;
                    }
                    hasThreeOfAKind = true;
                }
                else if (count == 2)
                {
                    if (hasThreeOfAKind)
                    {
                        return true;
                    }
                    hasPair = true;
                }
            }

            return false;
        }

        public virtual bool IsFlush()
        {
            var suit = this.Cards[0].Suit;
            return (this.Cards[1].Suit == suit &&
                this.Cards[2].Suit == suit &&
                this.Cards[3].Suit == suit &&
                this.Cards[4].Suit == suit);
        }

        public bool IsStraight()
        {
            var sortedCards = this.GetSortedCards();
            var lowestRank = sortedCards[0].Rank;

            if (lowestRank == Rank.Ace)
            {
                // Special case: Ace can be either low or high in a straight.
                // Low case is handled normally since by convention Ace is sorted lowest.
                // This case checkes for Ace-high straights.

                if (sortedCards[1].Rank == Rank.Ten &&
                    sortedCards[2].Rank == Rank.Jack &&
                    sortedCards[3].Rank == Rank.Queen &&
                    sortedCards[4].Rank == Rank.King)
                    return true;
            }

            return (sortedCards[1].Rank == (Rank)(lowestRank + 1) &&
                sortedCards[2].Rank == (Rank)(lowestRank + 2) &&
                sortedCards[3].Rank == (Rank)(lowestRank + 3) &&
                sortedCards[4].Rank == (Rank)(((int)lowestRank + 4) % 13));
        }

        public bool IsThreeOfAKind()
        {
            foreach (int count in this.CountOfRank)
                if (count == 3)
                    return true;

            return false;
        }

        public bool IsTwoPair()
        {
            int pairCount = 0;
            foreach (int count in this.CountOfRank)
                if (count == 2)
                    if (++pairCount == 2)
                        return true;

            return false;
        }

        public bool IsJacksOrBetter()
        {
            int[] jobRanks = new int[4] { 0, 10, 11, 12 };
            foreach (var rank in jobRanks)
                if (this.CountOfRank[rank] == 2)
                    return true;

            return false;
        }

        public bool IsLowPair()
        {
            int[] lowRanks = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            foreach (var rank in lowRanks)
                if (this.CountOfRank[rank] == 2)
                    return true;

            return false;
        }

        // Begin "Get" Functions Section.

        public bool Get4ToRF(out bool[] holds)
        {
            Subhand? subhand4ToRF = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsNToRoyalFlush(out _, out _, out _))
                {
                    subhand4ToRF = subhand;
                    break;
                }
            }

            holds = subhand4ToRF != null ? subhand4ToRF.Holds : DefaultHolds;
            return subhand4ToRF != null;
        }

        public bool Get3ToRF(out bool[] holds, out bool isTenAce3RF, out int suit, out Rank orcRank)
        {
            isTenAce3RF = false;
            suit = -1;
            orcRank = Rank.Two; // Some default

            Subhand? subhand3ToRF = null;
            foreach (Subhand subhand in this.ThreeCardCombos.Value)
            {
                if (subhand.IsNToRoyalFlush(out isTenAce3RF, out suit, out orcRank))
                {
                    subhand3ToRF = subhand;
                    break;
                }
            }

            holds = subhand3ToRF != null ? subhand3ToRF.Holds : DefaultHolds;
            return subhand3ToRF != null;
        }

        public bool Get4ToSF(out bool[] holds)
        {
            Subhand? subhand4ToSF = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsNToStraightFlush(out _, out _, out _))
                {
                    subhand4ToSF = subhand;
                    break;
                }
            }

            holds = subhand4ToSF != null ? subhand4ToSF.Holds : DefaultHolds;
            return subhand4ToSF != null;
        }

        public bool Get3ToSF(out bool[] holds, out bool threeToSFType2SpreadFiveWithOneHC, out Rank lowestRank, out Rank oscRank, StraightFlushType type)
        {
            lowestRank = Rank.Two; // Some default
            oscRank = Rank.Two; // Some default
            threeToSFType2SpreadFiveWithOneHC = false;
            Subhand? subhand3ToSF = null;
            foreach (Subhand subhand in this.ThreeCardCombos.Value)
            {
                if (subhand.IsNToStraightFlush(out threeToSFType2SpreadFiveWithOneHC, out lowestRank, out oscRank, type))
                {
                    subhand3ToSF = subhand;
                    break;
                }
            }

            holds = subhand3ToSF != null ? subhand3ToSF.Holds : DefaultHolds;
            return subhand3ToSF != null;
        }

        public bool Get4ToF(out bool[] holds)
        {
            Subhand? subhandF = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsAllSameSuit())
                {
                    subhandF = subhand;
                    break;
                }
            }

            holds = subhandF != null ? subhandF.Holds : DefaultHolds;
            return subhandF != null;
        }

        public bool Get3oK(out bool[] holds)
        {
            Subhand? subhand3oK = null;
            foreach (Subhand subhand in this.ThreeCardCombos.Value)
            {
                if (subhand.IsThreeOfAKind())
                {
                    subhand3oK = subhand;
                    break;
                }
            }

            holds = subhand3oK != null ? subhand3oK.Holds : DefaultHolds;
            return subhand3oK != null;
        }

        public bool GetTwoPair(out bool[] holds)
        {
            Subhand? subhandTwoPair = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsTwoPair())
                {
                    subhandTwoPair = subhand;
                    break;
                }
            }

            holds = subhandTwoPair != null ? subhandTwoPair.Holds : DefaultHolds;
            return subhandTwoPair != null;
        }

        public bool GetJoB(out bool[] holds)
        {
            Subhand? subhandJoB = null;
            foreach (Subhand subhand in this.TwoCardCombos.Value)
            {
                if (subhand.IsJacksOrBetter())
                {
                    subhandJoB = subhand;
                    break;
                }
            }

            holds = subhandJoB != null ? subhandJoB.Holds : DefaultHolds;
            return subhandJoB != null;
        }

        public bool GetUnsuitedTJQK(out bool[] holds)
        {
            Subhand? subhandUnsuitedTJQK = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsUnsuitedTJQK())
                {
                    subhandUnsuitedTJQK = subhand;
                    break;
                }
            }

            holds = subhandUnsuitedTJQK != null ? subhandUnsuitedTJQK.Holds : DefaultHolds;
            return subhandUnsuitedTJQK != null;
        }

        public bool GetLowPair(out bool[] holds)
        {
            Subhand? subhandLowPair = null;
            foreach (Subhand subhand in this.TwoCardCombos.Value)
            {
                if (subhand.IsLowPair())
                {
                    subhandLowPair = subhand;
                    break;
                }
            }

            holds = subhandLowPair != null ? subhandLowPair.Holds : DefaultHolds;
            return subhandLowPair != null;
        }

        public bool Get4ToOS(out bool[] holds)
        {
            Subhand? subhand4ToOS = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsNToOutideStraight())
                {
                    subhand4ToOS = subhand;
                    break;
                }
            }

            holds = subhand4ToOS != null ? subhand4ToOS.Holds : DefaultHolds;
            return subhand4ToOS != null;
        }

        public bool GetUnsuitedTwoRanks(out bool[] holds, Rank r1, Rank r2)
        {
            Subhand? subhandUnsuitedRanks = null;
            foreach (Subhand subhand in this.TwoCardCombos.Value)
            {
                if (subhand.IsUnsuitedRanks(r1, r2))
                {
                    subhandUnsuitedRanks = subhand;
                    break;
                }
            }

            holds = subhandUnsuitedRanks != null ? subhandUnsuitedRanks.Holds : DefaultHolds;
            return subhandUnsuitedRanks != null;
        }

        public bool GetSuitedTwoRanks(out bool[] holds, out int suit, Rank r1, Rank r2)
        {
            suit = -1;
            Subhand? subhandSuitedRanks = null;
            foreach (Subhand subhand in this.TwoCardCombos.Value)
            {
                if (subhand.IsSuitedRanks(out suit, r1, r2))
                {
                    subhandSuitedRanks = subhand;
                    break;
                }
            }

            holds = subhandSuitedRanks != null ? subhandSuitedRanks.Holds : DefaultHolds;
            return subhandSuitedRanks != null;
        }

        public bool GetUnsuitedThreeRanks(out bool[] holds, Rank r1, Rank r2, Rank r3)
        {
            Subhand? subhandSuitedRanks = null;
            foreach (Subhand subhand in this.ThreeCardCombos.Value)
            {
                if (subhand.IsUnsuitedRanks(r1, r2, r3))
                {
                    subhandSuitedRanks = subhand;
                    break;
                }
            }

            holds = subhandSuitedRanks != null ? subhandSuitedRanks.Holds : DefaultHolds;
            return subhandSuitedRanks != null;
        }

        public bool GetSingleRank(out bool[] holds, Rank r1)
        {
            Subhand? subhandSingleRank = null;
            foreach (Subhand subhand in this.Singles.Value)
            {
                if (subhand.Cards[0].Rank == r1)
                {
                    subhandSingleRank = subhand;
                    break;
                }
            }

            holds = subhandSingleRank != null ? subhandSingleRank.Holds : DefaultHolds;
            return subhandSingleRank != null;
        }

        public bool Get4ToIS(out bool[] holds, int highCardTarget)
        {
            Subhand? subhand4ToIS4HC = null;
            foreach (Subhand subhand in this.FourCardCombos.Value)
            {
                if (subhand.IsNToInsideStraight(highCardTarget))
                {
                    subhand4ToIS4HC = subhand;
                    break;
                }
            }

            holds = subhand4ToIS4HC != null ? subhand4ToIS4HC.Holds : DefaultHolds;
            return subhand4ToIS4HC != null;
        }

        // Begin protected helper functions Section.

        protected Card[] GetSortedCards()
        {
            return Cards.ToList().OrderBy(c => (int)c.Rank).ToArray();
        }

        protected int GetCountOfRank(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return CountOfRank[0];
                case Rank.Two:
                    return CountOfRank[1];
                case Rank.Three:
                    return CountOfRank[2];
                case Rank.Four:
                    return CountOfRank[3];
                case Rank.Five:
                    return CountOfRank[4];
                case Rank.Six:
                    return CountOfRank[5];
                case Rank.Seven:
                    return CountOfRank[6];
                case Rank.Eight:
                    return CountOfRank[7];
                case Rank.Nine:
                    return CountOfRank[8];
                case Rank.Ten:
                    return CountOfRank[9];
                case Rank.Jack:
                    return CountOfRank[10];
                case Rank.Queen:
                    return CountOfRank[11];
                case Rank.King:
                    return CountOfRank[12];
                default:
                    throw new Exception($"Unknown rank {rank}");
            }
        }

        protected bool Has(Rank rank)
        {
            return this.GetCountOfRank(rank) > 0;
        }
    }
}
