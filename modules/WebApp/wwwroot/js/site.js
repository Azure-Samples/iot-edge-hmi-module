/*
Overview: site.js 
1) establishes the signalR connection and controls the response to it.
2) Inserts the layout for the rectangles drawn over areas of the 
    shelf on load into the html
3) Dynamically updates the index.html file based on incoming data
  a) Updates rectangle colors around the products
  b) Updates the text within the rectangle to "Void" or blank

*/

const TABLE_ID = '#shelf_table';
const ID_TEXT = "Text"; //Text appended to SHELF_PRODUCTS name to show/remove Void text in rectangle

const SHELF_API_ENDPOINT = "/api/shelf/live";

const DOWN_JUSTIFY = 50;
const LEFT_JUSTIFY = 75;


const SVG = document.getElementById('shelfSvg');

/*A list of the rectangles which will be created and the attributes they will need*/
const SHELF_PRODUCTS = [
  { 'name': 'Cereal', 'x': '70', 'y': '95', 'width':  '255', 'height': '220' }, //Cereal
  { 'name': 'Oats', 'x': '340', 'y': '105', 'width': '285', 'height': '210' },   //Oats
  { 'name': 'Corn Flakes', 'x': '630', 'y': '105', 'width': '330', 'height': '205' },   //Corn Flakes
  { 'name': 'Sauce', 'x': '60', 'y': '340', 'width': '345', 'height': '180' },   //Sauce
  { 'name': 'Pickle', 'x': '415', 'y': '340', 'width': '225', 'height': '175' },    //Large Pickle
  { 'name': 'Salad Dressing', 'x': '655', 'y': '350', 'width': '300', 'height': '170' },    //Salad Dressing
  { 'name': 'Beans', 'x': '75', 'y': '550', 'width': '290', 'height': '155' },    //Beans
  { 'name': 'Ramen', 'x': '375', 'y': '545', 'width': '330', 'height': '160' },    //Ramen
  { 'name': 'Porridge', 'x': '715', 'y': '545', 'width': '240', 'height': '160' },    //Porridge
  { 'name': 'Chicken Soup', 'x': '70', 'y': '730', 'width': '425', 'height': '130' },    //Soup
  { 'name': 'Tuna', 'x': '500', 'y': '730', 'width': '200', 'height': '130' },    //Tuna
  { 'name': 'Corned Beef', 'x': '715', 'y': '730', 'width': '240', 'height': '130' }    //Corned Beef
];

/*
createShelfLayout takes the data necessary to create a rectangle, 
associated text (listed in SHELF_PRODUCTS) and append them to 
the specified svg on the page.
Ensure that you've updated the below product.* to match any changes you've made in the above key-value pairs.
*/

function createShelfLayout(product, svg) {
  const newRect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
  newRect.setAttribute('id', product.name);
  newRect.setAttribute('x', parseInt(product.x));
  newRect.setAttribute('y', parseInt(product.y));
  newRect.setAttribute('width', product.width) //product.x + product.width - (some distance for Void text)
  newRect.setAttribute('height', product.height)
  newRect.setAttribute('class','productRect');
  svg.appendChild(newRect);

  const newText = document.createElementNS('http://www.w3.org/2000/svg', 'text');
  newText.setAttribute('id', product.name+ID_TEXT)
  newText.setAttribute('x', parseInt(product.x) + parseInt(product.width) - LEFT_JUSTIFY);
  newText.setAttribute('y', parseInt(product.y) + DOWN_JUSTIFY);
  newText.setAttribute('class','productText');
  svg.appendChild(newText);
};

$(document).ready(function () {
  /*Establish signalR connection, see Startup.cs, ClientUpdateHub.cs,
    ClientNotifier.cs for more information on the signalR connection 
    and processes that are invoked.
  */
  const ws_connection = new signalR.HubConnectionBuilder().withUrl("/clientUpdateHub").build();
 
  for(product of SHELF_PRODUCTS){
    createShelfLayout(product, SVG);
  }

  /*on signalR "NewShelf" begin this proess*/
  ws_connection.on("NewShelf", function () {
        
    const route = SHELF_API_ENDPOINT;

    /*jQuery .getJSON based on API call in Controllers/ShelfController.cs*/
    $.getJSON(route, function (result) {
      const shelf = JSON.parse(result);
      const products = shelf.products;

      /*Clear table data except header row*/
      $(TABLE_ID).find("tr:gt(0)").remove();  

      /*Reset all rectangles to blue by id (which means that the products are stocked/ 
        in a good state, orange means void/bad state)*/
      $("[id^='U']").each(function (index, productRectangle) {
        productRectangle.setAttribute('style', 'stroke:blue;');
      });

      /*For each product on the shelf, determine if the rectangle should display 
      blue/orange for good/bad state.  Append the name data to the table 
      corresponding to that product location on the shelf.

      Blue/orange are used rather than red/green for accessibility .
      */
      $.each(products, function (index, product) {
        const void_str = product.voidStatus == 1 ? "True" : "False";
        let product_data = '';
        
        product_data += '<tr>';
        product_data += '<td>' + product.name + '</td>';
        product_data += '<td>' + void_str + '</td>';
        product_data += '</tr>';

        let productRectangle = document.getElementById(product.name);
        let productText = document.getElementById(product.name + "Text");
        if (product.voidStatus == 1) {
          productText.innerHTML = "Void";
          productRectangle.setAttribute('style', 'stroke:orange;');
        } else {
          productText.innerHTML = "";
          productRectangle.setAttribute('style', 'stroke:blue;');
        }

        $(TABLE_ID).append(product_data);
      });
    });
  });

  /*If signalR cannot connect display error message in console.*/
  ws_connection.start().catch(function (err) {
    return console.error(err.toString());
  });

});
