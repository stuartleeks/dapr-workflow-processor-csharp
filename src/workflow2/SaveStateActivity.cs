using Dapr.Client;
using Dapr.Workflow;

namespace Workflow2
{

	public class SaveStateActivity : WorkflowActivity<ProcessResult, bool>
	{
		private readonly DaprClient _daprClient;

		public SaveStateActivity(DaprClient daprClient)
		{
			_daprClient = daprClient;
		}
		public async override Task<bool> RunAsync(WorkflowActivityContext context, ProcessResult input)
		{
			Console.WriteLine($"Starting {nameof(SaveStateActivity)}: {context.InstanceId}");

			await _daprClient.SaveStateAsync(storeName: "statestore", key: context.InstanceId, value: input);

			return true;
		}
	}
}