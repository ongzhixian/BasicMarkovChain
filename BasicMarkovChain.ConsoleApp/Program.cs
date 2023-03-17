string[] trainingData = new string[] { "bag", "bat", "ate" };

// Define generic markov model with order 1 (monograms)
var model = new MarkovModel(1);

model.Train(trainingData);

model.DumpStateTransitions();

// Generate 5 random sentences from the model
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(model.GenerateValue());
}