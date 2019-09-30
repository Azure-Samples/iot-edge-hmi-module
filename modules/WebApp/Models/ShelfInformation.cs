/*
This file creates the data structure for the image (shelf) that
is being used on the webpage.  Changes here will need to be 
reflected in the InProcShelfDataGenerator and in the website
js files.
*/
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApp
{
    public class Product
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("voidStatus")]
        public int VoidStatus { get; set; }
    }

    public class Shelf {

        [JsonProperty("products")]
        public Product[] Products {get; set;}

    }
}