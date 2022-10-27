# Eutania-Server
Public version of Eutania's server to show off some of its codebase  

Technologies Used in Server
-> C#, .NET, SingalR, MYSQL

Technologies Used in Client
-> Javascript, Typescript, VueJS 3, Vuetify, CSS


  
*******   
In order to see a live version of Eutania, visit https://eutania.azurewebsites.net/.  From there, you can create an account to 
start from scratch, or follow the directions on the login screen to get an idea of things during the "playing" state.  
*******  
  
Eutania is a browser game created with my own ideas, blended with concepts from current Web3 games online today.  
Utilizing a mix of REST and socket API's, the game allows for the use of real-time browser events.  
  
-> An entire "universe" was created: planets and resources are all uniquely named and designed.  
-> Includes a functioning in-game currency system, including player wallets.  
-> Sectors are areas on planets in which players can mine to gather resources using their harvester.  
-> Harvesters can have items equipped to change their own properties.  
-> Each mine attempt brings with it a chance to mine an item, or the sector itself (if it is unowned).  
-> All game items, including sectors, can be traded between players in the in-game market.  
-> Sectors can have commission rates applied, in which any players mining on said sector automatically pay the owner
a commission rate - paid in resources mined.  
-> Players can apply buffs or debuffs to sectors - changing the rates at which mining events occur.  
-> A crafting system is in place, allowing players to use their gathered resources and items to create consumables or parts.  
-> Certain game events utilize socket technology to notify players of events.  For example, purchasing items on the market will
notify the seller if they are online.
