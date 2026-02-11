# next
add more props (you need not add all listed below)
finish tileset_rules.py (pseudocode)

# future
## art and concepting
add more props 
 - foliage (trees, flowers, lillypad, grass)
 - rocks
 - bridge
 - campfire

add creatures
 - fish shadows (likely using tris to allow for more animation and prettier scaling)
 - birds (fly away when close)

add sound

add game mechanics

add npcs

## tech
implement rendering/mapping
 - tilemap renderer (figure out how to render the tiles at runtime with correct y sorting)
 - tilemap generator (generate tilemap and collision info from heightmap and set of props)
 - mapping application (application for creating the heightmap and prop layout that can then be used to generate the final tilemap)
 - extra details:
  - props should be rotatable (to allow for more variation in things like flowers and rocks)
  - collision data is for the player but it will also need to store info for collision of things like the creatures and the fishing bobber. perhaps the collision data should just be the heightmap (modified by props) to allow it to be interpreted as I wish.
  - the 'overhangs' of elevated terrain that spill over onto the tile above will be rendered as translucent 'props'. all other tiles can be referenced from an opaque tileset.

implement gameplay
 - get player collison and walking working
 - get basic fishing working
 - add fish shadows
 - add fishing minigame
 - add other creatures
 - add npcs
