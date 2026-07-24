using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Symbio.API.Hubs;

[Authorize(Roles = "SME")]
public class AccountingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var email = Context.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(email))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(email));
        }

        await base.OnConnectedAsync();
    }

    public static string GetGroupName(string email) => $"sme-accounting:{email.Trim().ToLowerInvariant()}";
}
