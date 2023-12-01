using Microsoft.AspNetCore.Mvc;
using Dapr;
using Dapr.AspNetCore;
using System.Net;
using Dapr.Client;
using Microsoft.AspNetCore.Http.HttpResults;

namespace foo.Controllers;


public record class ProcessEvent(string InstanceId, string CorrelationId, string Content);

public record class InvokeProcessorRequest(string CorrelationId, string Content);
public record class InvokeProcessorResult(bool Success, string Result);


[ApiController]
[Route("[controller]")]
public class PubSubController : ControllerBase
{
    private readonly ILogger<PubSubController> _logger;

    public PubSubController(ILogger<PubSubController> logger)
    {
        _logger = logger;
    }

    [Topic("pubsub", "processor1")]
    [HttpPost("processor1")]
    public async Task<ActionResult> Process(ProcessEvent input, IHttpClientFactory httpClientFactory, DaprClient daprClient)
    {
        var action = "processor1";
        Console.WriteLine($"processing_consumer_processor1: Got data: {input}");
        var result = await InvokeProcessor(input, httpClientFactory, action);

        // Invoke service to raise event in workflow with result
        // TODO: Do this!
        var workflow = "workflow2"; // this could be specified in the payload for more flexibility;
        var body = new
        {
            input.InstanceId,
            input.CorrelationId,
            Response = result
        };
        await daprClient.InvokeMethodAsync(workflow, "raise-event", body);

        return Ok();
    }

    private static async Task<InvokeProcessorResult> InvokeProcessor(ProcessEvent input, IHttpClientFactory httpClientFactory, string action)
    {

        // Want to do this, but can't find a way to get the response body on a non-2XX response
        // Console.WriteLine($"Starting {nameof(InvokeProcessorActivity)}: {input.InstanceId}");
        // var result = await daprClient.InvokeMethodAsync<InvokeProcessorRequest, InvokeProcessorResult>(
        // 	appId: input.Action,
        // 	methodName: "process",
        // 	data: new InvokeProcessorRequest(correlationId: input.InstanceId, content: input.Content));

        var daprPort = int.Parse(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");

        // Make http call to submit action to processor
        using (var httpClient = httpClientFactory.CreateClient())
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                requestUri: $"http://localhost:{daprPort}/v1.0/invoke/{action}/method/process",
                value: new InvokeProcessorRequest(input.InstanceId, input.Content));

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"{input.InstanceId}: {input.CorrelationId}: ⏳ 429 response from processor");
                return new InvokeProcessorResult(Success: false, Result: "429 response from processor");
            }
            var result = await response.Content.ReadFromJsonAsync<InvokeProcessorResult>();
            if (result == null)
            {
                var body = await response.Content.ReadAsStringAsync();
                return new InvokeProcessorResult(Success: false, Result: body ?? "no response");
            }
            var emoji = result.Success ? "✅" : "❌";
            Console.WriteLine($"{input.InstanceId}: {input.CorrelationId}: {emoji} Result: {result}");
            return result;
        }
    }
}