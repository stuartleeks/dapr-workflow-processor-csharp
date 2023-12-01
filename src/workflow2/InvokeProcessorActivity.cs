using System.Net;
using Dapr.Client;
using Dapr.Workflow;

namespace Workflow2
{
	record class InvokeProcessorRequest(string CorrelationId, string Content);
	public record class InvokeProcessorResult(bool Success, string Result);


	public class InvokeProcessorActivity : WorkflowActivity<ProcessingAction, bool>
	{
		private readonly DaprClient _daprClient;

		public InvokeProcessorActivity(DaprClient daprClient)
		{
			_daprClient = daprClient;
		}
		public async override Task<bool> RunAsync(WorkflowActivityContext context, ProcessingAction input)
		{
			try
			{
				Console.WriteLine($"{context.InstanceId}: {context.Name}: ⚡ triggered");

				// Publish to pubsub for processing_consumer to pick up and invoke processor
				// We're using the Action directly - may want to add validation ;-)
				var message = new
				{
					InstanceId = context.InstanceId,
					CorrelationId = input.Name,
					Content = input.Content
				};
				await _daprClient.PublishEventAsync("pubsub", input.Action, message);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"InvokeProcessorResult:Exception: {ex}");
				throw;
			}
		}
	}
}