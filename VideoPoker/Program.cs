using System.Collections.Generic;
using VideoPoker;

// Init mode
Console.WriteLine("VIDEO POKER.\nSelect Mode by number:\n0: FreePlay\n1: Quiz\n2: Simulation\n3: SimulationDebug\n4: Evaluation");
var selectedMode = Console.ReadLine();
Console.WriteLine();
var mode = (GameMode)int.Parse(selectedMode!);
var evalHandCounts = new Dictionary<HandScore, int>();

// Init deck
IList<Card> deck = new List<Card>();
for (int i = 0; i < 13; i++)
    for (int j = 0; j < 4; j++)
        deck.Add(new Card((Rank)i, j));

int itrCount = 0;
while (true)
{
    ConsoleHelper.WriteLine("Hand #" + ++itrCount, mode);
    deck.Shuffle();
    var deckEnumerator = deck.GetEnumerator();

    List<Card> cards;
    if (mode != GameMode.Evaluation)
    {
        // Select the first 5 Cards, that's the starting hand.
        cards = new List<Card>();
        for (int i = 0; i < 5; i++)
        {
            deckEnumerator.MoveNext();
            cards.Add(deckEnumerator.Current);
        }

        ConsoleHelper.WriteLine(string.Join(" ", cards), mode);
    }
    else
    {
        var handInput = Console.ReadLine()!;
        // Turn input into cards
        var handSplit = handInput.Split(" ");
        if (handSplit.Length != 5)
        {
            ConsoleHelper.WriteLine("Invalid cards.", mode);
            continue;
        }

        cards = new List<Card>();
        foreach (var cardStr in handSplit)
        {
            Card card;
            if (!Card.ParseCard(cardStr, out card))
            {
                ConsoleHelper.WriteLine("Invalid cards.", mode);
                continue;
            }
            cards.Add(card);
        }
    }

    if (mode == GameMode.Quiz || mode == GameMode.FreePlay)
    {
        // User types 1 to hold card 1, 2 to hold card 2, etc...
        var holds = Console.ReadLine()!;
        if (mode == GameMode.Quiz)
        {
            // Compare the user's holds to the cpu player.
            var cpuHolds = Player.Play(new Hand(cards.ToArray()));
            var cpuHoldsStr = ConsoleHelper.HoldsToString(cpuHolds);
            if (holds.Equals(cpuHoldsStr))
                ConsoleHelper.WriteLine("Correct.", mode);
            else
                ConsoleHelper.WriteLine($"ERROR! (Optimal play: {cpuHoldsStr})", mode);
        }

        if (!holds.Contains("1"))
        {
            deckEnumerator.MoveNext();
            cards[0] = deckEnumerator.Current;
        }
        if (!holds.Contains("2"))
        {
            deckEnumerator.MoveNext();
            cards[1] = deckEnumerator.Current;
        }
        if (!holds.Contains("3"))
        {
            deckEnumerator.MoveNext();
            cards[2] = deckEnumerator.Current;
        }
        if (!holds.Contains("4"))
        {
            deckEnumerator.MoveNext();
            cards[3] = deckEnumerator.Current;
        }
        if (!holds.Contains("5"))
        {
            deckEnumerator.MoveNext();
            cards[4] = deckEnumerator.Current;
        }
    }    
    else
    {
        if (mode == GameMode.SimulationDebug)
            _ = Console.ReadLine();

        var holds = Player.Play(new Hand(cards.ToArray()));
        if (!holds[0])
        {
            deckEnumerator.MoveNext();
            cards[0] = deckEnumerator.Current;
        }
        if (!holds[1])
        {
            deckEnumerator.MoveNext();
            cards[1] = deckEnumerator.Current;
        }
        if (!holds[2])
        {
            deckEnumerator.MoveNext();
            cards[2] = deckEnumerator.Current;
        }
        if (!holds[3])
        {
            deckEnumerator.MoveNext();
            cards[3] = deckEnumerator.Current;
        }
        if (!holds[4])
        {
            deckEnumerator.MoveNext();
            cards[4] = deckEnumerator.Current;
        }

        if (mode != GameMode.Simulation)
        {
            ConsoleHelper.WriteLine(ConsoleHelper.HoldsToString(holds), mode);
        }
    }

    if (mode != GameMode.Evaluation)
    {
        ConsoleHelper.WriteLine(string.Join(" ", cards), mode);

        var handScore = Card.EvalHand(new Hand(cards.ToArray()));
        ConsoleHelper.WriteLine(handScore.ToString(), mode);

        int count;
        if (evalHandCounts.TryGetValue(handScore, out count))
            evalHandCounts[handScore] = count + 1;
        else
            evalHandCounts[handScore] = 1;
    }

    if (itrCount % 1_000_000 == 0)
    {
        long totalPlays = evalHandCounts.Values.Sum();
        ConsoleHelper.WriteLine($"TotalPlays: {totalPlays}", mode, isScore: true);
        string s = string.Join(Environment.NewLine, evalHandCounts.Select(kvp => $"{kvp.Key}:{kvp.Value} ({((double)kvp.Value)/((double)totalPlays)})"));
        ConsoleHelper.WriteLine(s, mode, isScore: true);
        long income =
            evalHandCounts[HandScore.RoyalFlush] * 4000 +
            evalHandCounts[HandScore.StraightFlush] * 250 +
            evalHandCounts[HandScore.FourOfAKind] * 125 +
            evalHandCounts[HandScore.FullHouse] * 45 +
            evalHandCounts[HandScore.Flush] * 30 +
            evalHandCounts[HandScore.Straight] * 20 +
            evalHandCounts[HandScore.ThreeOfAKind] * 15 +
            evalHandCounts[HandScore.TwoPair] * 10 +
            evalHandCounts[HandScore.JacksOrBetter] * 5;
        ConsoleHelper.WriteLine($"Income: {income}", mode, isScore: true);
        long cost = 5 * totalPlays;
        ConsoleHelper.WriteLine($"Cost  : {cost}", mode, isScore: true);
        double rateOfReturn = ((double) income) / ((double) cost);
        ConsoleHelper.WriteLine($"RoR   : {rateOfReturn}", mode, isScore: true);
        ConsoleHelper.WriteLine("------------------------------", mode, isScore: true);

        if (itrCount == 100_000_000)
            break;
    }

    ConsoleHelper.WriteLine("------------------------------", mode);
}