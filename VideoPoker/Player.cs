using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoPoker
{
    internal static class Player
    {
        /// <summary>
        /// Make the optimal (for JoB) set of holds for a given hand.
        /// 
        /// TODO: Exceptions A-F
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static bool[] Play(Hand hand)
        {
            bool[] holds;
            var dealtVal = Card.EvalHand(hand);

            // Part 1: Scoring Hands Checks - Use the evaluated HandScore to run an abbreviated/cheaper
            // version of the optimal play checks; no need to run through the whole set of hand possibilities.
            switch (dealtVal)
            {
                case HandScore.RoyalFlush:
                case HandScore.StraightFlush:
                case HandScore.FourOfAKind:
                case HandScore.FullHouse:
                    return new bool[5] { true, true, true, true, true };
                case HandScore.Flush:
                case HandScore.Straight:
                    // If you have a Flush or Straight, only throw away in favor of 4RF.
                    if (hand.Get4ToRF(out holds))
                        return holds;
                    else
                        return new bool[5] { true, true, true, true, true };
                case HandScore.ThreeOfAKind:
                    if (hand.Get3oK(out holds))
                        return holds;
                    else
                        throw new Exception("Get fn contradicted Is fn (three of a kind).");
                case HandScore.TwoPair:
                    if (hand.GetTwoPair(out holds))
                        return holds;
                    else
                        throw new Exception("Get fn contradicted Is fn (two pair).");
                case HandScore.JacksOrBetter:
                    // If you have a High Pair, only throw away in favor of 4RF or 4SF
                    if (hand.Get4ToRF(out holds))
                        return holds;
                    else if (hand.Get4ToSF(out holds))
                        return holds;
                    else if (hand.GetJoB(out holds))
                        return holds;
                    else
                        throw new Exception("Get fn contracticted Is fn (jacks or better).");
                case HandScore.None:
                    // Figure out non-scoring hands below.
                    break;
                default:
                    throw new Exception("Unrecognized HandScore.");
            }

            // Part 2: Non-scoring hands checks.

            // 4 to a royal flush
            if (hand.Get4ToRF(out holds))
                return holds;

            // 4 to a straight flush
            if (hand.Get4ToSF(out holds))
                return holds;

            // 3 to a royal flush |EXCEPTION A| (don't return right away in TenAce3RF scenario, need to check 4ToF)
            Rank orcRank;
            int suit;
            bool isTenAce3RF;
            bool[] holds3ToRF;
            if (hand.Get3ToRF(out holds3ToRF, out isTenAce3RF, out suit, out orcRank) && !isTenAce3RF)
                return holds3ToRF;

            // 4 to a flush
            if (hand.Get4ToF(out holds))
            {
                // Check for EXCEPTION A scenario.
                // 4 to a flush beats 3 to a royal if royal includes a ten and ace,
                // and the unsuited card is a 10 or straight penalty card.
                if (isTenAce3RF)
                {
                    // If we are here, we know the hand is made up like so (order may differ):
                    // 1. Ace suited
                    // 2. Ten suited
                    // 3. King/Queen/Jack suited (a.k.a. other royal card - ORC)
                    // 4. 2-9 suited (a.k.a. fourth suited card - FSC)
                    // 5. Unsuited card (UC)

                    // Get the one unsuited card.
                    var unsuitedCards = hand.Cards.Where(c => c.Suit != suit).ToList();
                    if (unsuitedCards.Count() != 1)
                        throw new Exception("There should be exactly 1 unsuited card in a 4ToF.");
                    var unsuitedCard = unsuitedCards.First();

                    // Check for straight penalty card (SPC). If the UC fills a straight "gap" in the 3toRF it's a SPC.
                    // In this case, since we have a Ten-to-Ace straight draw with one other suited royal card (the ORC), that means the
                    // UC is a SPC if and only if UC is a King/Queen/Jack that is NOT EQUAL in rank to the ORC.
                    bool unsuitedCardIsStraightPenaltyCard =
                        (unsuitedCard.Rank == Rank.King || unsuitedCard.Rank == Rank.Queen || unsuitedCard.Rank == Rank.Jack)
                        && unsuitedCard.Rank != orcRank;

                    // If unsuitedCard is a 10 or a straight penalty card, we're in the EXCEPTION A Scenario: fall through to the 4ToF.
                    // Otherwise, stick with the 3ToRF.
                    bool exceptionA = unsuitedCard.Rank == Rank.Ten || unsuitedCardIsStraightPenaltyCard;
                    if (!exceptionA)
                        return holds3ToRF;
                }

                return holds;
            }
            else if (isTenAce3RF)
            {
                // No four to a flush, so if we had a TenAce3RF take that.
                return holds3ToRF;
            }

            // Unsuited TJQK (a.k.a. 4 to an outside straight (3 hc))
            if (hand.GetUnsuitedTJQK(out holds))
                return holds;

            // Low Pair
            if (hand.GetLowPair(out holds))
                return holds;

            // 4 to an outside straight (0-2 hc)
            if (hand.Get4ToOS(out holds))
                return holds;

            // 3 to a straight flush (type 1)
            if (hand.Get3ToSF(out holds, out _, out _, out _, StraightFlushType.T1))
                return holds;

            // Std. QJ |EXCEPTION B| (don't return right away, need to check 4ToIS)
            bool[] holdsSuitedQJ;
            bool hasSuitedQJ = hand.GetSuitedTwoRanks(out holdsSuitedQJ, out suit, Rank.Queen, Rank.Jack);

            // 4 to an inside straight, 4 hc
            if (hand.Get4ToIS(out holds, highCardTarget: 4))
            {
                if (hasSuitedQJ)
                {
                    // Check for EXCEPTION B Scenario
                    // 4 to an inside straight beats suited jack and queen with 9 or flush penalty card.
                    bool exceptionB =
                        hand.Cards.Any(c => c.Rank == Rank.Nine) ||
                        hand.Cards.Any(c => c.Suit == suit && c.Rank != Rank.Queen && c.Rank != Rank.Jack);

                    // In the EXCEPTION B Scenario, fall through to 4ToIS (4HC).
                    // Otherwise, stick with the suited QJ.
                    if (!exceptionB)
                        return holdsSuitedQJ;
                }

                return holds;
            }
            else if (hasSuitedQJ)
            {
                // No inside straight draw so just take the suited QJ.
                return holdsSuitedQJ;
            }    

            // Std. KQ, S KJ
            if (hand.GetSuitedTwoRanks(out holds, out _, Rank.King, Rank.Queen))
                return holds;
            if (hand.GetSuitedTwoRanks(out holds, out _, Rank.King, Rank.Jack))
                return holds;

            // Std. AK, AQ, AJ
            if (hand.GetSuitedTwoRanks(out holds, out _, Rank.Ace, Rank.King))
                return holds;
            if (hand.GetSuitedTwoRanks(out holds, out _, Rank.Ace, Rank.Queen))
                return holds;
            if (hand.GetSuitedTwoRanks(out holds, out _, Rank.Ace, Rank.Jack))
                return holds;

            // 4 to an inside straight, 3 hc |EXCEPTION C| (don't return right away, need to check 3 to a straight flush (type 2))
            bool[] holds4ToIS3HC;
            bool is4ToIS3HC = hand.Get4ToIS(out holds4ToIS3HC, highCardTarget: 3);

            // 3 to a straight flush (type 2)
            bool threeToSFType2SpreadFiveWithOneHC;
            bool[] holds3ToSF;
            Rank lowestRank;
            Rank oscRank;
            if (hand.Get3ToSF(out holds3ToSF, out threeToSFType2SpreadFiveWithOneHC, out lowestRank, out oscRank, StraightFlushType.T2))
            {
                if (is4ToIS3HC && threeToSFType2SpreadFiveWithOneHC)
                {
                    // Check for EXCEPTION C Scenario
                    // 3 to a straight flush, spread 5, with 1 high card vs. 4 to an inside straight, with 3 high cards:
                    // Play the 3 to a straight flush if there is no straight penalty card.

                    // If we are here, we know the hand is made up like so (order may vary):
                    // 1. Suited Lowest Rank (LR)
                    // 2. Suited Highest Rank (rank == LR + 4)
                    // 3. LR+1/LR+2/LR+3 Suited (a.k.a. other suited card - OSC)
                    // 4. Fourth card in the inside straight (FCIS)
                    // 5. Other card (OC)

                    // The exception scenario is when either the FCIS or OC has a rank that fills in one of the
                    // two gaps formed by the LR-OSC-LR+4 cards.
                    Func<Card, bool> cardIsStraightPenaltyCard = (Card c) =>
                    {
                        int rankVal = (int)c.Rank;
                        int lowestRankVal = (int)lowestRank;
                        return rankVal > lowestRankVal && rankVal < lowestRankVal + 4 && c.Rank != oscRank;
                    };

                    // In the EXCEPTION C Scenario, fall through to 3ToSF.
                    // Otherwise, stick with the 4 to the inside straight.
                    if (hand.Cards.Any(cardIsStraightPenaltyCard))
                        return holds4ToIS3HC;
                }

                return holds3ToSF;
            }
            else if (is4ToIS3HC)
            {
                // No 3ToSF so if we have 4 to IS3HC take that.
                return holds4ToIS3HC;
            }

            // JQK
            if (hand.GetUnsuitedThreeRanks(out holds, Rank.Jack, Rank.Queen, Rank.King))
                return holds;

            // JQ
            if (hand.GetUnsuitedTwoRanks(out holds, Rank.Jack, Rank.Queen))
                return holds;

            // Std. TJ |EXCEPTION D| (don't return right away, need to check unsuited JK)
            bool[] holdsSuitedTJ;
            bool suitedTJ = hand.GetSuitedTwoRanks(out holdsSuitedTJ, out suit, Rank.Ten, Rank.Jack);

            // QK, JK
            if (hand.GetUnsuitedTwoRanks(out holds, Rank.Jack, Rank.King))
            {
                if (suitedTJ)
                {
                    // Check for EXCEPTION D Scenario
                    // Suited 10 and jack vs. an unsuited jack and king — If there is a flush penalty card,
                    // keep the jack and king. Otherwise, stick with 10 and jack.
                    bool exceptionD = hand.Cards.Any(c => c.Suit == suit && c.Rank != Rank.Ten && c.Rank != Rank.Jack);

                    // In the EXCEPTION D Scenario, fall through to Unsuited JK.
                    // Otherwise, stick with the suited TJ.
                    if (!exceptionD)
                        return holdsSuitedTJ;
                }

                return holds;
            }
            else if (suitedTJ)
            {
                // No unsuited JK so just take the Suited TJ.
                return holdsSuitedTJ;
            }

            if (hand.GetUnsuitedTwoRanks(out holds, Rank.Queen, Rank.King))
                return holds;

            // Std. TQ |EXCEPTION E|
            bool[] holdsSuitedTQ;
            bool suitedTQ = hand.GetSuitedTwoRanks(out holdsSuitedTQ, out suit, Rank.Ten, Rank.Queen);

            // KA, QA, JA
            if (hand.GetUnsuitedTwoRanks(out holds, Rank.Queen, Rank.Ace))
            {
                if (suitedTQ)
                {
                    // Check for EXCEPTION E Scenario
                    // Suited 10 and queen vs. an unsuited queen and ace — If there is a flush penalty card,
                    // keep the queen and ace. Otherwise sticking with the 10 and queen is the better play. 
                    bool exceptionE = hand.Cards.Any(c => c.Suit == suit && c.Rank != Rank.Ten && c.Rank != Rank.Queen);

                    // In the EXCEPTION E Scenario, fall through to Unsuited QA.
                    // Otherwise, stick with the suited TQ.
                    if (!exceptionE)
                        return holdsSuitedTQ;
                }

                return holds;
            }
            else if (suitedTQ)
            {
                // No unsuited QA so just take the Suited TQ.
                return holdsSuitedTQ;
            }

            if (hand.GetUnsuitedTwoRanks(out holds, Rank.King, Rank.Ace))
                return holds;
            if (hand.GetUnsuitedTwoRanks(out holds, Rank.Jack, Rank.Ace))
                return holds;

            // J
            if (hand.GetSingleRank(out holds, Rank.Jack))
                return holds;

            // Std. TK |EXCEPTION F|
            bool[] holdsSuitedTK;
            bool suitedTK = hand.GetSuitedTwoRanks(out holdsSuitedTK, out suit, Rank.Ten, Rank.King);

            // Q
            if (hand.GetSingleRank(out holds, Rank.Queen))
                return holds;

            // K
            if (hand.GetSingleRank(out holds, Rank.King))
            {
                if (suitedTK)
                {
                    // Check for EXCEPTION F Scenario
                    // Suited 10, king vs. king only: Normally the suited ten and king is better than the king alone,
                    // however, if you must discard a 9 and a flush penalty card, then hold the king only.
                    bool exceptionF = hand.Cards.Any(c => c.Rank == Rank.Nine) && hand.Cards.Any(c => c.Suit == suit && c.Rank != Rank.Ten && c.Rank != Rank.King);

                    // In the EXCEPTION F Scenario, fall through to K only.
                    // Otherwise, stick with the suited TK.
                    if (!exceptionF)
                        return holdsSuitedTK;
                }

                return holds;
            }

            // A
            if (hand.GetSingleRank(out holds, Rank.Ace))
                return holds;

            // 3 to a straight flush (type 3)
            if (hand.Get3ToSF(out holds, out _, out _, out _, StraightFlushType.T3))
                return holds;

            // Garbage
            return new bool[5] { false, false, false, false, false };
        }
    }
}
