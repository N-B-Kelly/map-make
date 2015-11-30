As it stands right now, this will generate a map based on a combination of perlin noise and cellular automata.

----------------------------------------------------
<dl>
There are two options for generation: Constrained and Open <br/>
  These can be accessed by right and left clicking on the map to the left of the window. <br/>
To control the box on the right hand side, use the numpad. <br/>
You can modify how the view radius works by using:    {   }   | <br/>
You can modify the size of the map by using:    C   V <br/>
</dl>
----------------------------------------------------
<dl> 
The functional build of the program will generally be in: <br/>
  map_explore/Display_Map/bin/Debug  <br/>
 <br/>
There are two files:<br/>
  Display map is a gui that you can interact with,  <br/>
  map_explore just generates a single map, doesn't give you any choice, <br/>
    then dumps it in a text file and (probably) tries to open it with emacs  <br/>
    <br/>
  the main function of map_explore is also outdated, and will either be cleaned up later, or just turned into a dll, as the componenents are required for proper function.<br/>
  The window will adjust the sizes of the textboxes as you resize it, so don't be afraid to do that if you're looking at pixels.</dl>
  
-----------------------------------------------------  

goals:
  *		To develop an autonomous system of blind map exploration
  *		To make it optimal
  *		To allow a player to compete against a computer when exploring a map 
  *		<dl>To allow the computer to peek at what the player has explored <br/>
    in the same way the player can peek at what the computer has explored in the development of his strategy</dl>
  *		to do this without causing a housefire
