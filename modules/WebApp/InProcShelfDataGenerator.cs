/*
This creates dummy image data for isolated HMI development.

See README.md for instructions on how to set the environment variables to use HttpModuleClient instead.
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
using Microsoft.Extensions.Logging;
using Serilog;

namespace WebApp {

    public class InProcShelfDataGenerator : BackgroundService
    {
        readonly ILogger<InProcShelfDataGenerator> _logger;

        private IBackgroundShelfQueue shelfQueue {get; set;}

        //The product names are defined on the shelf itself.  See the wwwroot/js/site.js code for the image layout.
        private string[] products = new[] 
        { 
            "Cereal"	   , 
            "Oats"	   , 
            "Corn Flakes", 
            "Sauce" , 
            "Pickle"	   , 
            "Salad Dressing"	   , 
            "Beans"	   , 
            "Ramen" , 
            "Porridge"	   , 
            "Chicken Soup",
            "Tuna",
            "Corned Beef"
        };

        public InProcShelfDataGenerator(IBackgroundShelfQueue shelfQueue, ILogger<InProcShelfDataGenerator> logger)
        {
            this.shelfQueue = shelfQueue;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task QueueShelf(Shelf productData)
        {
            try 
            {
                _logger.LogInformation($"Number of products on shelf: {productData.Products.Length}");

                if(shelfQueue.Count() > 120) 
                {
                    while(shelfQueue.Count() > 120)
                    {
                        await shelfQueue.DequeueAsync(new CancellationToken()); //throw away result
                    }
                }

                // exit early if we dont have a deserialized element and a serial number
                    if(productData != null) {
                        _logger.LogInformation("Shelf Data Generator queueing in progress...");
                        shelfQueue.QueueShelf(productData);
                    }
            } 
            catch (AggregateException ex)
            {
                _logger.LogError($"Error processing message: {ex.Flatten()}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex}");
            }
        }
        
        static Random rd = new Random();  
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing shelf generation.");
            
            while(!stoppingToken.IsCancellationRequested) 
            {
                var shelfProducts = products.Select(productName => new Product 
                                {
                                    Name = productName,
                                    VoidStatus = rd.Next(0,2)  //0 and 1 are used to indicate that the product is stocked/voided
                                })
                            .Where(c => c != null);

                await QueueShelf(new Shelf { Products = shelfProducts.ToArray()});
                
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            _logger.LogInformation("Cancellation requested.");
        }
    }
}