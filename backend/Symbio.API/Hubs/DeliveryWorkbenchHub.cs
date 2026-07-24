using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Symbio.API.Hubs;

[Authorize(Roles = "Expert")]
public class DeliveryWorkbenchHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var expertEmail = Context.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(expertEmail))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(expertEmail));
        }

        await base.OnConnectedAsync();
    }

    public static string GetGroupName(string expertEmail) => $"workbench:{expertEmail.Trim().ToLowerInvariant()}";
}