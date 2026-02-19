
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
	if av>=GRASS:
        if quad<=1:
            if av >= HILL and ah >= HILL:
                return 9
            
            if ah>=GRASS:
                if di<=WATER:
                    return 12
            
            elif ah==SAND:
                if di>=GRASS:
                    return 11
                return 10
            elif ah<=WATER:
                if di<=WATER:
                    return 13
                return 14
        
        if quad>=2:
            if ah<=WATER:
                return 0
            if ah==SAND or (ah>=GRASS and di<=WATER):
                return 2

        if di>=SAND and ah>=GRASS:
            return 8
        
    elif av==SAND:
        if quad>=2 and di>=GRASS and ah<=WATER::
            return 6
        if ah<=WATER:
            if di>=SAND:
                return 7
            return 6
        else:
            if ah>=GRASS and di>=GRASS:
                return 5
            return 4
    
    elif av<=WATER:
        if av == ah == DEEP_WATER:
            return 1
        if ah>WATER:
            if quad<=1 and di<=WATER and ah>=GRASS:
                return 3
            return 2
        return 0

elif tt == SAND:
	if ah<=SAND and av<=SAND:
		return 0
	if ah>=GRASS and av>=GRASS:
		if quad>=2:
			return 1
		if ah>=HILL and av>=HILL:
			return 2
		return 1
	if ah>=GRASS:
		if di < SAND or (av<=SAND and di<=SAND):
			return 4
		return 3
	if di < SAND or (ah<=SAND and di<=SAND):
		return 6
	return 5

elif tt == GRASS:
	if ah == av == WATER:
		if quad<=1 and di==SAND:
			return 2
		return 1
	if quad >= 2: # bottom half
		return 0
	# top half - av is north
	if ah == av and av>=HILL:
		return 3
	return 0

elif tt == HILL:
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
	if s_tt == TALL_HILL:
		return 0
	if s_di == TALL_HILL:
		return 1
	if ah == TALL_HILL:
		if quad <= 1: # top half
			return 2
		if di == HILL:
			return 3
		return 2
	if quad <= 1: # top half
		return 4
	# bottom half - av is south
	if s_di == HILL:
		return 4
	if ah == HILL:
		return 5
	if ah<=WATER and av<=WATER:
		return 6
	return 7
