As it stands right now, this will generate a map based on a combination of perlin noise and cellular automata.
There are two options for generation: Constrained and Open
  These can be accessed by right and left clicking on the map to the left of the window.

To control the box on the right hand side, use the numpad
You can modify how the view radius works by using:    {   }   |
You can modify the size of the map by using:    C   V

The program functiona build of the program will generally be in 
  map_explore/Display_Map/bin/Debug
There are two files:
  Display map is a gui that you can interact with
  map_explore just generates a single map, doesn't give you any choice, 
    then dumps it in a text file and tries to open it with emacs
  the main function of map_explore is also outdated, but the componenets are required for proper function.
  
  The window will adjust the sizes of the textboxes as you resize it, so don't be afraid to do that if you're looking at pixels.
  
goals:
  To develop an autonomous system of blind map exploration
  To make it optimal
  To allow a player to compete against a computer when exploring a map
  To allow the computer to peek at what the player has explored 
    in the same way the player can peek at what the computer has explored in the development of his strategy
  to do this without causing a housefire
