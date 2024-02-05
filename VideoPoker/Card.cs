namespace VideoPoker
{
    internal class Card
    {
        public Rank Rank { get; set; }

        /// <summary>
        /// 0 => clubs
        /// 1 => diamonds
        /// 2 => hearts
        /// 3 => spades
        /// </summary>
        public int Suit { get; set; }

        public int Code => (Suit * 13) + (int)Rank;

        public Card(Rank rank, int suit)
        {
            this.Rank = rank;
            this.Suit = suit;
        }

        public override string ToString()
        {
            string strVal = string.Empty;
            switch ((int)this.Rank)
            {
                case 0:
                    strVal += "A";
                    break;
                case 1:
                    strVal += "2";
                    break;
                case 2:
                    strVal += "3";
                    break;
                case 3:
                    strVal += "4";
                    break;
                case 4:
                    strVal += "5";
                    break;
                case 5:
                    strVal += "6";
                    break;
                case 6:
                    strVal += "7";
                    break;
                case 7:
                    strVal += "8";
                    break;
                case 8:
                    strVal += "9";
                    break;
                case 9:
                    strVal += "T";
                    break;
                case 10:
                    strVal += "J";
                    break;
                case 11:
                    strVal += "Q";
                    break;
                case 12:
                    strVal += "K";
                    break;
            }

            switch (this.Suit)
            {
                case 0:
                    strVal += "c";
                    break;
                case 1:
                    strVal += "d";
                    break;
                case 2:
                    strVal += "h";
                    break;
                case 3:
                    strVal += "s";
                    break;
            }

            return strVal;
        }

        /// <summary>
        /// Given a hand, score it.
        /// </summary>
        /// <param name="hand"></param>
        /// <returns>
        /// -1 => Loss
        /// 0 => JoB
        /// 1 => Two Pair
        /// 2 => Three of a Kind
        /// 3 => Straight
        /// 4 => Flush
        /// 5 => Full House
        /// 6 => Four of a Kind
        /// 7 => Straight Flush
        /// 8 => Royal Flush
        /// </returns>
        public static HandScore EvalHand(Hand hand)
        {
            // Group A: Straight/Flush hands. Order matters within this group.
            bool isFlush = hand.IsFlush();
            bool isStraight = hand.IsStraight();
            if (isFlush && isStraight)
            {
                if (hand.IsRoyalFlush())
                {
                    return HandScore.RoyalFlush;
                }

                return HandScore.StraightFlush;
            }
            else if (isFlush)
            {
                return HandScore.Flush;
            }
            else if (isStraight)
            {
                return HandScore.Straight;
            }

            // Group B: Multiples of the same rank hands. Order matters within this group.
            // Here we frontload rank counting since this computation is shared by the below checks.
            if (hand.IsFourOfAKind())
            {
                return HandScore.FourOfAKind;
            }
            else if (hand.IsFullHouse())
            {
                return HandScore.FullHouse;
            }
            else if (hand.IsThreeOfAKind())
            {
                return HandScore.ThreeOfAKind;
            }
            else if (hand.IsTwoPair())
            {
                return HandScore.TwoPair;
            }
            else if (hand.IsJacksOrBetter())
            {
                return HandScore.JacksOrBetter;
            }

            return HandScore.None;
        }

        public static bool ParseCard(string cardStr, out Card card)
        {
            card = new Card(Rank.Ace, 0); // Ac default

            if (cardStr.Length != 2)
                return false;

            Rank rank;
            if (cardStr[0] == 'A')
                rank = Rank.Ace;
            else if (cardStr[0] == '2')
                rank = Rank.Two;
            else if (cardStr[0] == '3')
                rank = Rank.Three;
            else if (cardStr[0] == '4')
                rank = Rank.Four;
            else if (cardStr[0] == '5')
                rank = Rank.Five;
            else if (cardStr[0] == '6')
                rank = Rank.Six;
            else if (cardStr[0] == '7')
                rank = Rank.Seven;
            else if (cardStr[0] == '8')
                rank = Rank.Eight;
            else if (cardStr[0] == '9')
                rank = Rank.Nine;
            else if (cardStr[0] == 'T')
                rank = Rank.Ten;
            else if (cardStr[0] == 'J')
                rank = Rank.Jack;
            else if (cardStr[0] == 'Q')
                rank = Rank.Queen;
            else if (cardStr[0] == 'K')
                rank = Rank.King;
            else
                return false;

            int suit;
            if (cardStr[1] == 'c')
                suit = 0;
            else if (cardStr[1] == 'd')
                suit = 1;
            else if (cardStr[1] == 'h')
                suit = 2;
            else if (cardStr[1] == 's')
                suit = 3;
            else
                return false;

            card = new Card(rank, suit);
            return true;
        }
    }
}
