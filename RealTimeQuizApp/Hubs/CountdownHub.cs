using Microsoft.AspNetCore.SignalR;

namespace RealTimeQuizApp.Hubs
{
    public class CountdownHub : Hub
    {
        public async Task SendCountdownTime(int timeLeft)
        {
            await Clients.All.SendAsync("ReceiveCountdown", timeLeft);
        }
    }
}
