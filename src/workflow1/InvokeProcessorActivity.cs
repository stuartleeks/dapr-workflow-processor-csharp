using System.Net;
using Dapr.Client;
using Dapr.Workflow;

namespace Workflow1
{
	record class InvokeProcessorRequest(string CorrelationId, string Content);
	public record class InvokeProcessorResult(bool Success, string Result);


	public class InvokeProcessorActivity : WorkflowActivity<ProcessingAction, InvokeProcessorResult>
	{
		private readonly DaprClient _daprClient;
		private readonly IHttpClientFactory _httpClientFactory;

		public InvokeProcessorActivity(DaprClient daprClient, IHttpClientFactory httpClientFactory)
		{
			_daprClient = daprClient;
			_httpClientFactory = httpClientFactory;
		}
		public async override Task<InvokeProcessorResult> RunAsync(WorkflowActivityContext context, ProcessingAction input)
		{
			try
			{
				Console.WriteLine($"{context.InstanceId}: {context.Name}: ⚡ triggered");

				// Want to do this, but can't find a way to get the response body on a non-2XX response
				// Console.WriteLine($"Starting {nameof(InvokeProcessorActivity)}: {context.InstanceId}");
				// var result = await _daprClient.InvokeMethodAsync<InvokeProcessorRequest, InvokeProcessorResult>(
				// 	appId: input.Action,
				// 	methodName: "process",
				// 	data: new InvokeProcessorRequest(correlationId: context.InstanceId, content: input.Content));


				var daprPort = int.Parse(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");

				// Make http call
				using (var httpClient = _httpClientFactory.CreateClient())
				{
					HttpResponseMessage response = await httpClient.PostAsJsonAsync(
						requestUri: $"http://localhost:{daprPort}/v1.0/invoke/{input.Action}/method/process",
						value: new InvokeProcessorRequest(CorrelationId: context.InstanceId, Content: input.Content));

					if (response.StatusCode == HttpStatusCode.TooManyRequests)
					{
						Console.WriteLine($"{context.InstanceId}: {context.Name}: ⏳ 429 response from processor");
						return new InvokeProcessorResult(Success: false, Result: "429 response from processor");
					}
					var result = await response.Content.ReadFromJsonAsync<InvokeProcessorResult>();
					if (result == null)
					{
						var body = await response.Content.ReadAsStringAsync();
						return new InvokeProcessorResult(Success: false, Result: body ?? "no response");
					}
					var emoji = result.Success ? "✅" : "❌";
					Console.WriteLine($"{context.InstanceId}: {context.Name}: {emoji} Result: {result}");
					return new InvokeProcessorResult(Success: result.Success, Result: result.Result);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"InvokeProcessorResult:Exception: {ex}");
				throw;
			}
		}
	}
}