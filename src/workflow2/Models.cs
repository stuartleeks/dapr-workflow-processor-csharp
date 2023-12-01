namespace Workflow2
{
	public record class ProcessingAction(string Name, string Action, string Content);
	public record class ProcessingStep(string Name, ProcessingAction[] Actions);
	public record class ProcessingPayload(ProcessingStep[] Steps);

	public record class ProcessActionResult(string Action, string Content, bool Success, string? Result, int AttemptCount);
	public record class ProcessStepResult(string Name, List<ProcessActionResult> Actions);
	public record class ProcessResult(string Id, string Status, List<ProcessStepResult> Steps);
}