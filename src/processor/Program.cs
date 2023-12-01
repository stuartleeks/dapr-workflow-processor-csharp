var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
var delay = float.Parse(Environment.GetEnvironmentVariable("DELAY") ?? "0");
var failureChance = int.Parse(Environment.GetEnvironmentVariable("FAILURE_CHANCE") ?? "0");
var shiftAmount = int.Parse(Environment.GetEnvironmentVariable("SHIFT_AMOUNT") ?? "1");

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

string CaesarShift(string plainText, int shift)
{
	var cipherText = string.Empty;
	foreach (char ch in plainText)
	{
		if (char.IsLetter(ch))
		{
			char d = char.IsUpper(ch) ? 'A' : 'a';
			cipherText += (char)(((ch + shift - d) % 26) + d);
		}
		else
		{
			cipherText += ch;
		}
	}
	return cipherText;
}

app.MapPost("/process", (ProcessRequest request) =>
{
	Console.WriteLine("Processing request : " + request);
	if (delay > 0)
	{
		Console.WriteLine("Delaying for " + delay + " seconds");
		Thread.Sleep(TimeSpan.FromSeconds(delay));
	}
	if (failureChance > 0)
	{
		var random = new Random();
		var chance = random.Next(1, 100);
		if (chance <= failureChance)
		{
			Console.WriteLine("Failing request with chance " + chance);
			return Results.Json(data: new { Success = false, Result = "Failed by random chance 😢" }, statusCode: 400);
		}
	}
	Console.WriteLine("Shifting content by " + shiftAmount);
	var shiftedContent = CaesarShift(request.Content, shiftAmount);
	return Results.Json(data: new { Success = true, Result = shiftedContent }, statusCode: 200);
});

app.MapGet("/", () => "Hello 👋");

await app.RunAsync($"http://0.0.0.0:{port}");

public record ProcessRequest(string CorrelationId, string Content);
