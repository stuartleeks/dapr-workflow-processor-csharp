using Dapr.Client;
using Dapr.Workflow;
using Workflow1;

const string DaprWorkflowComponent = "dapr";

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions());

builder.Services
	.AddDaprWorkflow(options =>
		{
			options.RegisterWorkflow<ProcessingWorkflow>();
			options.RegisterActivity<InvokeProcessorActivity>();
			options.RegisterActivity<SaveStateActivity>();
		})
	.AddHttpClient();

DaprClient daprClient;
var apiToken = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
if (!string.IsNullOrEmpty(apiToken))
{
	daprClient = new DaprClientBuilder().UseDaprApiToken(apiToken).Build();
}
else
{
	daprClient = new DaprClientBuilder().Build();
}

// // Wait for the sidecar to become available
// while (!await daprClient.CheckHealthAsync())
// {
// 	Console.WriteLine("Waiting for Dapr sidecar...");
// 	Thread.Sleep(TimeSpan.FromSeconds(5));
// }

var app = builder.Build();

app.MapPost("/workflows", async (ProcessingPayload request) =>
{
	var wfResponse = await daprClient.StartWorkflowAsync(
		workflowComponent: DaprWorkflowComponent,
		workflowName: nameof(ProcessingWorkflow),
		input: request,
		instanceId: Guid.NewGuid().ToString());

	return Results.Created(uri: $"/workflows/{wfResponse.InstanceId}", new { success = true, instanceId = wfResponse.InstanceId });
});

app.MapGet("/workflows/{workflowId}", async (string workflowId) =>
{
	var workflow = await daprClient.GetWorkflowAsync(instanceId: workflowId, workflowComponent: DaprWorkflowComponent);
	if (workflow.RuntimeStatus == WorkflowRuntimeStatus.Completed)
	{
		var state = await daprClient.GetStateAsync<ProcessResult>(storeName: "statestore", key: workflowId);
		return Results.Json(data: state, statusCode: 200);
	}

	return Results.Json(data: new { status = workflow.RuntimeStatus.ToString() }, statusCode: 200);
});

app.MapGet("/", () => "Hello 👋");


Console.WriteLine("Starting app");
await app.RunAsync($"http://0.0.0.0:{port}");
