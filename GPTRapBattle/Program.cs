using OpenAI_API.Chat;
using System.Text;

var id = Guid.NewGuid();
var date = DateTime.Now.ToString("ddd, dd MMM yyy HH':'mm':'ss");

var contestantAName = "Frankie McSlickrhyme";
var contestantBName = "Bob Jovington";

Directory.CreateDirectory($"output/{id}");

var openAiApiKey = Environment.GetEnvironmentVariable("OpenAiApiKey", EnvironmentVariableTarget.User);
var api = new OpenAI_API.OpenAIAPI(openAiApiKey);

var moderatorSetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var moderator = api.Chat.CreateConversation(moderatorSetupRequest);
var moderatorSetupPrompt = await File.ReadAllTextAsync("Prompts/ModeratorSetup.txt");
moderatorSetupPrompt = moderatorSetupPrompt.Replace("{date}", date);
moderator.AppendSystemMessage(moderatorSetupPrompt);
moderator.AppendUserInput($"Producer: The contestants names are (A)='{contestantAName}' and (B)='{contestantBName}'. The rap battle subject is 'American Politics'.");

await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });

var contestantASetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var contestantA = api.Chat.CreateConversation(contestantASetupRequest);
var contestantASetupPrompt = await File.ReadAllTextAsync("Prompts/ContestantASetup.txt");
contestantASetupPrompt = contestantASetupPrompt.Replace("{date}", date);
contestantASetupPrompt = contestantASetupPrompt.Replace("{name}", contestantAName);
contestantASetupPrompt = contestantASetupPrompt.Replace("{contestantBName}", contestantBName);
contestantASetupPrompt = contestantASetupPrompt.Replace("{subject}", date);
contestantA.AppendSystemMessage(contestantASetupPrompt);
contestantA.AppendUserInput("Please provide your opening rap!");

var contestantARap1 = new StringBuilder();
await contestantA.StreamResponseFromChatbotAsync(s => { contestantARap1.Append(s); });

Console.WriteLine(contestantARap1);

moderator.AppendUserInput(contestantARap1.ToString());

await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });

var contestantBSetupRequest = new ChatRequest
{
    Model = OpenAI_API.Models.Model.GPT4
};
var contestantB = api.Chat.CreateConversation(contestantBSetupRequest);
var contestantBSetupPrompt = await File.ReadAllTextAsync("Prompts/ContestantBSetup.txt");
contestantBSetupPrompt = contestantBSetupPrompt.Replace("{date}", date);
contestantBSetupPrompt = contestantBSetupPrompt.Replace("{name}", contestantBName);
contestantBSetupPrompt = contestantBSetupPrompt.Replace("{contestantBName}", contestantAName);
contestantBSetupPrompt = contestantBSetupPrompt.Replace("{subject}", date);
contestantB.AppendSystemMessage(contestantBSetupPrompt);
contestantB.AppendUserInput($"{contestantARap1}\r\n\r\nPlease respond!");

var contestantBRap1 = new StringBuilder();
await contestantB.StreamResponseFromChatbotAsync(s => { contestantBRap1.Append(s); });

Console.WriteLine(contestantBRap1);

moderator.AppendUserInput(contestantBRap1.ToString());

await moderator.StreamResponseFromChatbotAsync(s => { Console.Write(s); });

Console.WriteLine(string.Empty);
Console.WriteLine(new string('-', 25));
Console.WriteLine("Rap complete, press any key to exit.");
Console.ReadKey();