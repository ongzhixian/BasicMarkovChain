// Discrete-Time Markov Chain (DTMC) for strings.
using System.Data;
using System.Linq;

internal class MarkovModel
{
    // The order of the markov model (number of previous states to consider)
    private int order;

    private const char STX = (char)2;
    private const char EOT = (char)3;


    private Random random = new Random();

    // A dictionary that maps a state (a list of words) to a list of possible next words and their frequencies as a tuple
    private readonly Dictionary<List<char>, List<(char, int)>> stateTransitions;

    // A constructor that initializes a markov model with a given order
    public MarkovModel(int order)
    {
        this.order = order;

        stateTransitions = new Dictionary<List<char>, List<(char, int)>>(new ListComparer());

        //this.initialState = new string(STX, order).ToList(); // Which is faster: Enumerable.Repeat(STX, order).ToList();



        //stateTransitions.Add(initialState, new List<(char, int)>());
    }

    internal void Train(string[] trainingData)
    {
        foreach (var data in trainingData)
        {
            Train(data.Trim());
        }
    }

    private void Train(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        // Normalize data into list of tokens with STX and EOT
        var tokens = data.ToList();
        tokens.Insert(0, STX);
        tokens.Add(EOT);

        for (int i = 0; i < tokens.Count - order; i++)
        {
            var currentState = tokens.GetRange(i, order);

            // Get the next value after the state 
            var nextValue = tokens[i + order];

            // Add or update the state transition with the next word and its frequency 
            updateStateTransitionsFrequencyMap(currentState, nextValue);
        }
    }

    private void updateStateTransitionsFrequencyMap(List<char> currentState, char nextValue)
    {
        

        if (!stateTransitions.ContainsKey(currentState))
        {
            stateTransitions[currentState] = new List<(char, int)>();
        }

        // Get the state transitions frequency map (STFM) for current state

        var index = stateTransitions[currentState].FindIndex(x => x.Item1 == nextValue);

        // Add to STFM if it does not already exists; else increment frequency count

        if (index == -1)
        {
            Stats.Track("Add");
            stateTransitions[currentState].Add((nextValue, 1));
        }
        else
        {
            Stats.Track("Incr");
            stateTransitions[currentState][index] = (nextValue, stateTransitions[currentState][index].Item2 + 1);
        }
    }

    internal void DumpStateTransitions()
    {
        foreach (var stateTransition in stateTransitions)
        {
            Console.Write("Key: [");
            Console.Write(stateTransition.Key.ToArray());
            Console.WriteLine("]; Values:");

            foreach (var item in stateTransition.Value)
            {
                Console.WriteLine($"  {item}");
            }
        }
    }

    internal string GenerateValue()
    {
        var sentence = new List<char>();

        var state = Enumerable.Repeat(STX, order).ToList();

        while (true)
        {
            // Get the possible next words and their frequencies for the current state
            // Get frequency count for next values
            var nextValueFrequencyMap = stateTransitions[state];

            // Choose a random next word based on its frequency
            var totalFrequency = nextValueFrequencyMap.Sum(x => x.Item2);
            var randomNumber = random.Next(totalFrequency);
            var cumulativeFrequency = 0;
            char nextValue = '\0';
            foreach (var (value, frequency) in nextValueFrequencyMap)
            {
                cumulativeFrequency += frequency;
                if (randomNumber < cumulativeFrequency)
                {
                    nextValue = value;
                    break;
                }
            }

            // If the next word is an end token, stop generating
            if (nextValue == EOT)
            {
                break;
            }

            // Add the next word to the sentence
            sentence.Add(nextValue);

            // Update the current state by removing the first word and adding the next word
            state.RemoveAt(0);
            state.Add(nextValue);
        }

        // Return the sentence as a string with spaces between words and a period at the end
        return string.Join(string.Empty, sentence);
    }
}

public class ListComparer : IEqualityComparer<List<char>>
{
    public bool Equals(List<char> x, List<char> y)
    {
        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<char> obj)
    {
        int hash = 17;
        foreach (var item in obj)
        {
            hash = hash * 23 + item.GetHashCode();
        }
        return hash;
    }
}

public static class Stats
{
    private static Dictionary<string, int> stats = new Dictionary<string, int>();


    public static void Track(string key)
    {
        if (stats.ContainsKey(key))
        {
            stats[key] += 1;
            Console.WriteLine($"{key}: {stats[key]}");
        }
        else
        {
            stats[key] = 1;
            Console.WriteLine($"{key}: 1");
        }
    }
}