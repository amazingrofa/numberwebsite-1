using Microsoft.AspNetCore.SignalR;

namespace WebApplication2.Hubs
{
    public class ViewCountHub : Hub
    {
        public async Task JoinNumberGroup(string numberId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, numberId);
        }
    }
}