using System.Diagnostics;
using Dapr.Workflow;

namespace Workflow2
{

	public class ProcessingWorkflow : Workflow<ProcessingPayload, ProcessResult>
	{
		public async override Task<ProcessResult> RunAsync(WorkflowContext context, ProcessingPayload input)
		{
			try
			{

				if (!context.IsReplaying)
				{
					Console.WriteLine($"Starting workflow: {context.InstanceId}");
				}

				var stepResults = new List<ProcessStepResult>();
				var haveErrors = false;
				foreach (var step in input.Steps)
				{
					var actions = step.Actions;
					Console.WriteLine($"{context.InstanceId}: Step {step.Name}");
					if (haveErrors)
					{
						// skip further processing and add empty results
						stepResults.Add(new ProcessStepResult(
							step.Name,
							actions.Select(action => new ProcessActionResult(action.Action, action.Content, false, null, 0)).ToList()));
						continue;
					}
					// Submit actions to pubsub and wait for response via external event
					var actionTasks = step.Actions.Select(async action =>
					{
						await context.CallActivityAsync<bool>(nameof(InvokeProcessorActivity), action);
						return await context.WaitForExternalEventAsync<InvokeProcessorResult>(action.Name);
					}).ToList();
					await Task.WhenAll(actionTasks);

					stepResults.Add(
							new ProcessStepResult(
								step.Name,
								actionTasks
									.Select((task, index) => new ProcessActionResult(
										actions[index].Action,
										actions[index].Content,
										task.Result.Success,
										task.Result.Result,
										1)
									).ToList()
							)
						);
					if (actionTasks.Any(t => t.IsFaulted || !t.Result.Success))
					{
						Console.WriteLine($"{context.InstanceId}: Got error(s) - skipping further processing");
						haveErrors = true;
					}
				}

				var workflowResult = new ProcessResult(
					context.InstanceId,
					haveErrors ? "Failed" : "Completed",
					stepResults);

				await context.CallActivityAsync<bool>(
					name: nameof(SaveStateActivity),
					 input: workflowResult);

				return workflowResult;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				throw;
			}
		}
	}

}