def LoadData(name):
    with open(name, "r") as file:
        data = file.read()
    return data

def SolveWaterTile(av, di, ah, quad):
    if av>=GRASS:
        if quad<=1:
            if av >= HILL and ah >= HILL:
                return 9
            
            if ah>=GRASS:
                if di<=WATER:
                    return 12
            
            elif ah==SAND:
                if di>=GRASS and ah==SAND:
                    return 11
                else:
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
        if quad>=2:
            if di>=GRASS and ah<=WATER:
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

def Test():
    data = LoadData("water_tile_dataset.txt")
    top, bottom = [[[int(v) for v in case.split(" ")[1:]] for case in part.split("|")] for part in data.split("\n")]

    for quad in range(4):
        current_set = top if quad<=1 else bottom
        for case in current_set:
            result = SolveWaterTile(case[0], case[1], case[2], quad)
            if result != case[3]:
                print(f"Failure: c{case}, q{quad}, recieved {result}")

    print("done")

DEEP_WATER = 0
WATER = 1
SAND = 2
GRASS = 3
HILL = 4
TALL_HILL = 5

Test()
