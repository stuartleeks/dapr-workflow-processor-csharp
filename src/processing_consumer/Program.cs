using Dapr;

var port = Environment.GetEnvironmentVariable("PORT") ?? "41234";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddDaprClient();

var app = builder.Build();

// DaprClient daprClient;
// var apiToken = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
// if (!string.IsNullOrEmpty(apiToken))
// {
//     daprClient = new DaprClientBuilder().UseDaprApiToken(apiToken).Build();
// }
// else
// {
//     daprClient = new DaprClientBuilder().Build();
// }

// Dapr configurations
app.UseCloudEvents();

app.MapSubscribeHandler();

app.MapControllers();


await app.RunAsync($"http://0.0.0.0:{port}");

