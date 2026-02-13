using BlazorApp01.Features.CQRS.MediatorFacade;
using BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.BackgroundProcessing.Jobs;

internal sealed class AddRandomCustomTaskJob(
    ISenderFacade senderFacade,
    ILogger<AddRandomCustomTaskJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Starting AddRandomCustomTaskJob...");
        await senderFacade.SendAsync(new AddRandomCustomTaskCommand());
        logger.LogInformation("Finished AddRandomCustomTaskJob.");
    }
}