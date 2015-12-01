As it stands right now, this will generate a map based on a combination of perlin noise and cellular automata.

----------------------------------------------------
<dl>
There are two options for generation: Constrained and Open <br/>
  These can be accessed by right and left clicking on the map to the left of the window. <br/>
To control the box on the right hand side, use the numpad. <br/>
You can modify how the view radius works by using:&nbsp&nbsp    {&nbsp&nbsp   }&nbsp&nbsp   |&nbsp&nbsp <br/>
You can modify the size of the map by using:&nbsp&nbsp    C&nbsp&nbsp   V <br/>
You can switch view mode by pressing:&nbsp&nbsp B <br />
You can "solve" the map by pressing &nbsp&nbsp Spacebar <br /> <br />

There are currently two usage modes:<br/> </dl>
*    view: Shows map view, explore map
*    mask: Shows negative light map, explore map
    
the panes work as follows:
*   map view is just a simple overhead view of the map. Everything is visible, useful for checking on map features.
*   explore view shows the regions currently lit, and the regions that have been seen. This can tell you what areas have and have not been seen.
*   negative light map: this effectively shows each point that we have seen that is in direct LOS of where we suspect any open nodes to be.

There are a couple of known issues with how we solve the map:
  In our metric system, we check only the manhattan distance to each point - building a distance map for each movement seems to be a little too taxing - will need to devise a way of doing this well later.
  This means that occasionally on things like paired corridors, we'll flipflop between and slowly solve them - feels like watching butter.
  My current goal at the moment is to find an optimal way of doing this - Since this is built based on where the player has seen, we can't simply choose to take the large performance hit at the start to calculate  every mapping, and then compare using a bitwise & method.
  
----------------------------------------------------
<dl> 
The functional build of the program will generally be in: <br/>
  map_explore/Display_Map/bin/Debug  <br/>
 <br/>
There are two files:<br/>
  Display map is a gui that you can interact with,  <br/>
  map_explore just generates a single map, doesn't give you any choice, <br/>
    &nbsp&nbsp&nbsp&nbspthen dumps it in a text file and (probably) tries to open it with emacs  <br/>
    <br/>
  the main function of map_explore is also outdated, and will either be cleaned up later, or just turned into a dll, as the componenents are required for proper function.<br/>
  The window will adjust the sizes of the textboxes as you resize it, so don't be afraid to do that if you're looking at pixels.</dl>
  
-----------------------------------------------------  

goals:
  *		To develop an autonomous system of blind map exploration
  *		To make it optimal
  *		To allow a player to compete against a computer when exploring a map 
  *		To not cause a housefire
  *		<dl>To allow the computer to peek at what the player has explored <br/>
    in the same way the player can peek at what the computer has explored in the development of his strategy</dl>
