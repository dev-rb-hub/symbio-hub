using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Symbio.API.Hubs
{
    [Authorize]
    public class MarketplaceHub : Hub
    {
        public const string ExpertsGroupName = "experts";

        public async Task JoinExpertAlerts()
        {
            if (Context.User?.IsInRole("Expert") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ExpertsGroupName);
            }
        }
    }
}