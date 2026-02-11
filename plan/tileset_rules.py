
# run per quadrant of tile
# return value is tile index relative to this tile type in tileset.ase

# vars:
# tt = the tile type of this tile
# ah = the tile type of the horizontal adjacent tile
# av = the tile type of the vertical adjacent tile
# di = the tile type of the diagonal adjacent tile
# n_tt = tile type to the north
# s_tt = tile type to the south
# s_di = tile type to the south diagonal. east/west is same as the current quadrant
# quad = which quadrant of the tile is this piece? left -> right, top -> bottom, 0-3

# tile types:
# 0 - deep water
# 1 - water
# 2 - beach
# 3 - grass
# 4 - hill
# 5 - tall hill

if tt == 0:
	if ah == av == di != 0:
		return 1
	return 0

elif tt == 1:
	if av<=1:
		if ah<=1:
			if ah == av == 0:
				return 1
			return 0
		if quad <= 1 and :
		return 2
	if av==2:
		if ah==2:
			return 4
		if ah>2:
			return 5
		if di<=1:
			return 6
		return 7
	if av>=3 and di>=3 and ah>=3:
		return 8
	if quad>=2:
		return 0
	# unfinished

elif tt == 2:
	if ah<=2 and av<=2:
		return 0
	if ah > 2 and av > 2:
		if quad>=2:
			return 1
		if ah > 3 and av > 3:
			return 2
		return 1
	if ah > 2:
		if di < 2 or (av<=2 and di<=2):
			return 4
		return 3
	if av > 2:
		if di < 2 or (ah<=2 and di<=2):
			return 6
		return 5

elif tt == 3:
	if ah == av == 1:
		return 1
	if quad >= 2: # bottom half
		return 0
	if ah == n_tt and (n_tt == 4 or n_tt == 5):
		return 2
	return 0

elif tt == 4:
	# todo: describe overlay/overhang prop spawing
	if quad <= 1: # top half
		return 0
	# bottom half
	if s_tt == 4 or s_tt == 5:
		return 0
	if s_di == 4 or s_di == 5:
		return 1
	if ah == 4 or ah == 5:
		return 2
	if (ah==0 or ah==1) and (s_tt==0 or s_tt==1):
		return 3
	return 4

elif tt == 5:
	# todo: describe overlay/overhang prop spawing
	if s_tt == 5:
		return 0
	if s_di == 5:
		return 1
	if ah == 5:
		if quad <= 1: # top half
			return 2
		if s_di == 4:
			return 3
		return 2
	if quad <= 1: # top half
		return 4
	if s_di == 4:
		return 4
	if ah == 4:
		return 5
	if (ah==0 or ah==1) and (s_tt==0 or s_tt==1):
		return 6
	return 7
