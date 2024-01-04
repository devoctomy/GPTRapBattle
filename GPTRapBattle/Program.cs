using OpenAI_API.Chat;
using System.Text;

var id = Guid.NewGuid();
var date = DateTime.Now.ToString("ddd, dd MMM yyy HH':'mm':'ss");

var contestantAName = "Tobin 'The Tax Collector' Mathews";
var contestantBName = "Tim 'Master' Weeks";
var subject = "Tax Returns";

Directory.CreateDirectory($"output/{id}");

var openAiApiKey = Environment.GetEnvironmentVariable("OpenAiApiKey", EnvironmentVariableTarget.User);
var api = new OpenAI_API.OpenAIAPI(openAiApiKey);

// introduction from moderator
var moderatorSetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var moderator = api.Chat.CreateConversation(moderatorSetupRequest);
var moderatorSetupPrompt = await File.ReadAllTextAsync("Prompts/ModeratorSetup.txt");
var moderatorResponse = await SetupActor(
    moderator,
    moderatorSetupPrompt,
    $"Producer: The contestants names are (A)='{contestantAName}' and (B)='{contestantBName}'. The rap battle subject is '{subject}'.",
    new KeyValuePair<string, string>("{date}", date));
Console.WriteLine();
Console.WriteLine();

// get first rap from A
var contestantASetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var contestantA = api.Chat.CreateConversation(contestantASetupRequest);
var contestantASetupPrompt = await File.ReadAllTextAsync("Prompts/ContestantASetup.txt");
var contestantARap1 = await SetupActor(
    contestantA,
    contestantASetupPrompt,
    $"Please provide your opening rap '{contestantAName}'!.",
    new KeyValuePair<string, string>("{date}", date),
    new KeyValuePair<string, string>("{name}", contestantAName),
    new KeyValuePair<string, string>("{opponent}", contestantBName),
    new KeyValuePair<string, string>("{subject}", subject));
Console.WriteLine();
Console.WriteLine();

// send A first rap to moderator
moderator.AppendUserInput(contestantARap1.ToString());
await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });
Console.WriteLine();
Console.WriteLine();

// get first rap from B
var contestantBSetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var contestantB = api.Chat.CreateConversation(contestantBSetupRequest);
var contestantBSetupPrompt = await File.ReadAllTextAsync("Prompts/ContestantBSetup.txt");
var contestantBRap1 = await SetupActor(
    contestantB,
    contestantBSetupPrompt,
    $"{contestantARap1}\r\n\r\nPlease respond '{contestantBName}'!",
    new KeyValuePair<string, string>("{date}", date),
    new KeyValuePair<string, string>("{name}", contestantBName),
    new KeyValuePair<string, string>("{opponent}", contestantAName),
    new KeyValuePair<string, string>("{subject}", subject));
Console.WriteLine();
Console.WriteLine();

// send B first rap to moderator
moderator.AppendUserInput(contestantBRap1.ToString());
await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });
Console.WriteLine();
Console.WriteLine();

// get last rap from A
var contestantARap2 = new StringBuilder();
contestantA.AppendUserInput($"{contestantBRap1}\r\n\r\nPlease respond '{contestantAName}'!");
await contestantA.StreamResponseFromChatbotAsync(s =>
{
    contestantARap2.Append(s);
    Console.Write(s);
});
Console.WriteLine();
Console.WriteLine();

// send A last rap to moderator
moderator.AppendUserInput(contestantARap2.ToString());
await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });
Console.WriteLine();
Console.WriteLine();

// get last rap from B
var contestantBRap2 = new StringBuilder();
contestantB.AppendUserInput($"{contestantARap2}\r\n\r\nPlease respond '{contestantBName}'!");
await contestantB.StreamResponseFromChatbotAsync(s =>
{
    contestantBRap2.Append(s);
    Console.Write(s);
});
Console.WriteLine();
Console.WriteLine();

// send B last rap to moderator
moderator.AppendUserInput($"{contestantARap2}\r\n\r\nThat's the last rap of the contest!");
await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });
Console.WriteLine();
Console.WriteLine();

// And we are done!
Console.WriteLine(string.Empty);
Console.WriteLine(new string('-', 25));
Console.WriteLine("Rap complete, press any key to exit.");
Console.ReadKey();

async Task<string> SetupActor(
    Conversation conversation,
    string promptTemplate,
    string userInput,
    params KeyValuePair<string, string>[] tokens)
{
    foreach (var token in tokens)
    {
        promptTemplate = promptTemplate.Replace(token.Key, token.Value);
    }
    conversation.AppendSystemMessage(promptTemplate);
    conversation.AppendUserInput($"{userInput}");

    var response = new StringBuilder();
    await conversation.StreamResponseFromChatbotAsync(s => 
    {
        response.Append(s);
        Console.Write(s);
    });

    Console.WriteLine(string.Empty);
    return response.ToString();
}