
# run per quadrant of tile
# return value is tile index relative to this tile type in tileset.ase

# vars:
# tt = the tile type of this tile
# ah = the tile type of the horizontal adjacent tile
# av = the tile type of the vertical adjacent tile
# di = the tile type of the diagonal adjacent tile
# s_tt = tile type to the south
# s_di = tile type to the south diagonal. east/west is same as the current quadrant
# quad = which quadrant of the tile is this piece? left -> right, top -> bottom, 0-3

# tile types:
DEEP_WATER = 0
WATER = 1
SAND = 2
GRASS = 3
HILL = 4
TALL_HILL = 5

if tt == DEEP_WATER:
	if ah == av == di != DEEP_WATER:
		return 1
	return 0

elif tt == WATER:
	if av<=WATER or (quad>=2 and av>=GRASS):
		if ah<=WATER:
			if ah == av == DEEP_WATER:
				return 1
			return 0

		if quad<=1 and ah>SAND and di<=WATER:
			return 3
		return 2
	if av==SAND:
		if ah==SAND:
			return 4
		if ah>SAND:
			return 5
		if di<=WATER:
			return 6
		return 7
	if av>=GRASS and di>=GRASS and ah>=GRASS:
		return 8
	if quad>=2:
		return 0
	# unfinished

elif tt == SAND:
	if ah<=SAND and av<=SAND:
		return 0
	if ah>=GRASS and av>=GRASS:
		if quad>=2:
			return 1
		if ah > GRASS and av > GRASS:
			return 2
		return 1
	if ah > SAND:
		if di < SAND or (av<=SAND and di<=SAND):
			return 4
		return 3
	if av > SAND:
		if di < SAND or (ah<=SAND and di<=SAND):
			return 6
		return 5

elif tt == GRASS:
	if ah == av == WATER:
		return 1
	if quad >= 2: # bottom half
		return 0
	# bottom half - av is north
	if ah == av and (av == HILL or av == TALL_HILL):
		return 2
	return 0

elif tt == HILL:
	# todo: describe overlay/overhang prop spawing
	if quad <= 1: # top half
		return 0
	# bottom half - av and di are both south
	if av == HILL or av == TALL_HILL:
		return 0
	if di == HILL or di == TALL_HILL:
		return 1
	if ah == HILL or ah == TALL_HILL:
		return 2
	if ah<=WATER and av<=WATER:
		return 3
	return 4

elif tt == TALL_HILL:
	# todo: describe overlay/overhang prop spawing
	if s_tt == TALL_HILL:
		return 0
	if s_di == TALL_HILL:
		return 1
	if ah == TALL_HILL:
		if quad <= 1: # top half
			return 2
		if s_di == HILL:
			return 3
		return 2
	if quad <= 1: # top half
		return 4
	# bottom half - av is north
	if s_di == HILL:
		return 4
	if ah == HILL:
		return 5
	if ah<=WATER and av<=WATER:
		return 6
	return 7
