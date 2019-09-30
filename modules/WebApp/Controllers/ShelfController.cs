/*
    Web API Controller for getting the Shelf data out of the queue
    after the signalR notification that data is available is received
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
       
        private IBackgroundShelfQueue shelfQueue {get; set;}

        //Use dependency injection and singleton pattern for the ShelfController constructor
        public ShelfController(IBackgroundShelfQueue shelfQueue){
            this.shelfQueue = shelfQueue;
        }
        
        // GET api/values
        [HttpGet]
        [Produces("application/json")]
        [Route("live")]
        public async Task<string> GetLiveShelf()
        {
            if (shelfQueue.Count() > 0){
                var shelf = await shelfQueue.DequeueAsync(new CancellationToken());
                if(shelf != null){
                    return JsonConvert.SerializeObject(shelf);
                }
            }
            return string.Empty;
        }
    }
}
