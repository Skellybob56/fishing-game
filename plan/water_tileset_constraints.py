# if av, di, ah : return index

# tile types:
DEEP_WATER = 0
WATER = 1
SAND = 2
GRASS = 3
HILL = 4
TALL_HILL = 5

WET = [DEEP_WATER, WATER]
HILLY = [HILL, TALL_HILL]
STABLE = [GRASS, HILL, TALL_HILL]

? = [DEEP_WATER, WATER, SAND, GRASS, HILL, TALL_HILL]

if top:
	# special cases
	if DEEP_WATER, ?, DEEP_WATER: 
		return 1
	if HILLY, ?, HILLY:
		return 9

	# main cases
	if WET, WET, WET:
		return 0
	if WET, WET, SAND:
		return 2
	if WET, WET, STABLE:
		return 3
	if WET, SAND, WET:
		return 0
	if WET, SAND, SAND:
		return 2
	if WET, SAND, STABLE:
		return 2
	if WET, STABLE, WET:
		return 0
	if WET, STABLE, SAND:
		return 2
	if WET, STABLE, STABLE:
		return 2

	if SAND, WET, WET:
		return 6
	if SAND, WET, SAND:
		return 4
	if SAND, WET, STABLE:
		return 4
	if SAND, SAND, WET:
		return 7
	if SAND, SAND, SAND:
		return 4
	if SAND, SAND, STABLE:
		return 4
	if SAND, STABLE, WET:
		return 7
	if SAND, STABLE, SAND:
		return 4
	if SAND, STABLE, STABLE:
		return 5
	
	if STABLE, WET, WET:
		return 13
	if STABLE, WET, SAND:
		return 10
	if STABLE, WET, STABLE:
		return 12
	if STABLE, SAND, WET:
		return 14
	if STABLE, SAND, SAND:
		return 10
	if STABLE, SAND, STABLE:
		return 8
	if STABLE, STABLE, WET:
		return 14
	if STABLE, STABLE, SAND:
		return 11
	if STABLE, STABLE, STABLE:
		return 8

if bottom:
	# special cases
	if DEEP_WATER, ?, DEEP_WATER: 
		return 1

	# main cases
	if WET, WET, WET:
		return 0
	if WET, WET, SAND:
		return 2
	if WET, WET, STABLE:
		return 2
	if WET, SAND, WET:
		return 0
	if WET, SAND, SAND:
		return 2
	if WET, SAND, STABLE:
		return 2
	if WET, STABLE, WET:
		return 0 
	if WET, STABLE, SAND:
		return 2
	if WET, STABLE, STABLE:
		return 2
	
	if SAND, WET, WET:
		return 6
	if SAND, WET, SAND:
		return 4
	if SAND, WET, STABLE:
		return 4
	if SAND, SAND, WET:
		return 7
	if SAND, SAND, SAND:
		return 4
	if SAND, SAND, STABLE:
		return 4
	if SAND, STABLE, WET:
		return 6
	if SAND, STABLE, SAND:
		return 4
	if SAND, STABLE, STABLE:
		return 5
	
	if STABLE, WET, WET:
		return 0
	if STABLE, WET, SAND:
		return 2
	if STABLE, WET, STABLE:
		return 2
	if STABLE, SAND, WET:
		return 0
	if STABLE, SAND, SAND:
		return 2
	if STABLE, SAND, STABLE:
		return 8
	if STABLE, STABLE, WET:
		return 0
	if STABLE, STABLE, SAND:
		return 2
	if STABLE, STABLE, STABLE:
		return 8
