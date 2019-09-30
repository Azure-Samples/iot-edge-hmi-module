using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace WebApp
{
public class ClientNotifier : BackgroundService
    {
        private IBackgroundShelfQueue shelfQueue { get; set; }
        
        private IHubContext<ClientUpdateHub> hubContext { get; set; }
        public ClientNotifier(IBackgroundShelfQueue shelfQueue,
            IHubContext<ClientUpdateHub> hubContext)
        {
            this.shelfQueue = shelfQueue;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            /*hubContext allows ClientNotifier to act as a part of the signalR hub and send messages.  See this
            for more information: https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.1 */

            var notifytask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (shelfQueue.Count() > 0)
                    {
                        await this.hubContext.Clients.All.SendAsync("NewShelf");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }, stoppingToken);

            await Task.WhenAll(notifytask);

        }
    }
}