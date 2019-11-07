/*
This is the code that connects to the IoT Edge Hub and receives the shelf data.

The local code to this for testing is the InProcShelfDataGenerator which is used to generate data
without connecting to IoT Edge Hub (primarily for testing purposes).

Set the environment variable in deployment.template.json or launch.json as described in README.md 
to toggle between the two. For the demo, this module is not used.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Logging;

namespace WebApp {

/*Create the module as an ASP.NET Core Background Service
Details of this implementation pattern (Queued background tasks section) are available here: 
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.1#queued-background-tasks
*/
    public class HttpModuleClient : BackgroundService
    {           
        readonly ILogger<HttpModuleClient> _logger;

        private IBackgroundShelfQueue shelfQueue {get; set;}
        private ModuleClient moduleClient {get; set;}

        public HttpModuleClient(IBackgroundShelfQueue shelfQueue, ILogger<HttpModuleClient> logger)
        { 
            this.shelfQueue = shelfQueue;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /*The PipeMessage method is what will connect to the IoT Edge Hub and will queue the shelf data
        for processing. 
        
        It will be registered as an inputMessageHandler with the IoT Edge Hub in the ExecuteAsync method
        that follows.
        */
        public async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            /*This section receives the data from the IoT Edge Hub and converts it to a Shelf object 
              for processing through signalR to the WebApp module.
            */
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                try{
                    _logger.LogInformation($"Receiving Message: {messageString.TrimStart('"').TrimEnd('"').Replace('\\', ' ')}");
                    Shelf productData = JsonConvert.DeserializeObject<Shelf>
                        (messageString.TrimStart('"').TrimEnd('"').Replace("\\",String.Empty));

                    /*If we have more than 120 shelves to process, remove down to the limit*/

                    if(shelfQueue.Count() > 120){
                        while(shelfQueue.Count() > 120){
                            await shelfQueue.DequeueAsync(new CancellationToken()); //throw away result
                            _logger.LogInformation("Dequeing Extra live shelves.");
                        }
                    }
                    if(productData != null) {
                        // Use appropriate shelfQueue based on count.
                        if(productData.Products.Count() == 0){
                            _logger.LogInformation("Queuing 'Live' Feed for displaying in WebApp.");
                            shelfQueue.QueueShelf(productData);
                        } 
                    }
                // catch and swallow exceptions 
                } catch (AggregateException ex){
                    _logger.LogError($"Error processing message: {ex.Flatten()}");
                } catch (Exception ex){
                    _logger.LogError($"Error processing message: {ex}");
                }
            }
            return MessageResponse.Completed;
        }

        // ExecuteAsync is called when IHostedService starts.  
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            //Set AMQP as the transportation option
            var amqpSetting = new AmqpTransportSettings(Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            // SetInputMessageHandlerAsync provides a route ('WebAppInput') and the method to use
            // when a message is sent to the route ('PipeMessage')
            this.moduleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await moduleClient.OpenAsync();
            await moduleClient.SetInputMessageHandlerAsync("webAppInput", PipeMessage, moduleClient);
            _logger.LogInformation("IoT Hub module client initialized.");
        }
    }

    /* Create the interfaces for the singleton dependency injection that are used
    to coordinate receiving shelves that are received over IoT Edge Hub and sent to
    the HMI
    */
    public interface IBackgroundShelfQueue
    {
        void QueueShelf(Shelf workItem);

        Task<Shelf> DequeueAsync(CancellationToken token);

        int Count();

        IEnumerable<Shelf> List();
    }

    /* Create the implementations for the interfaces above */

    public class BackgroundShelfQueue : IBackgroundShelfQueue
    {
        private ConcurrentQueue<Shelf> _shelves = new ConcurrentQueue<Shelf>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueShelf(Shelf shelf)
        {
            if (shelf == null)
            {
                throw new ArgumentNullException(nameof(shelf));
            }

            _shelves.Enqueue(shelf);
            _signal.Release();
        }

        public async Task<Shelf> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _shelves.TryDequeue(out var shelf);

            return shelf;
        }

        public int Count()
        {
            return _shelves.Count();
        }

        public IEnumerable<Shelf> List()
        {
            return _shelves.AsEnumerable();
        }
    }
}