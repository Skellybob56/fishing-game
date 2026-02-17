def LoadData(name):
    with open(name, "r") as file:
        data = file.read()
    return data

def WriteData(name, text):
    with open(name, "w") as file:
        data = file.write(text)

def SplitTopBottom(data):
    return data.split("if top:\n")[1].split("if bottom:\n")

# 3 digits in base 3
def IncrementTrinary(v):
    out = [i for i in v]
    out[2] += 1
    
    if out[2]>=3:
        out[2] = 0
        out[1] += 1
        
        if out[1]>=3:
            out[1] = 0
            out[0] += 1

    return out

def ParseMainCases(data):
    parsed_main_cases = data.split("# main cases")[1].split("return ")[1:]
    parsed_main_cases = [s.split("\n")[0] for s in parsed_main_cases]
    parsed_main_cases = [int(s) for s in parsed_main_cases]

    out = []
    i = 0
    index = [0, 0, 0]
    while index[0] < 3:
        out.append(index + [parsed_main_cases[i]])
        i += 1
        index = IncrementTrinary(index)
    
    return out

def UnpackMainCases(main_cases):
    lookup = [[0, 1], [2], [3, 4, 5]]
    out = []
    for case in main_cases:
        for av in lookup[case[0]]:
            for di in lookup[case[1]]:
                for ah in lookup[case[2]]:
                    out.append((av, di, ah, case[3]))
    return out

def OverrideWithSpecialCases(unpacked_mcases, top):
    out = [x for x in unpacked_mcases]
    for i in range(len(out)):
        if out[i][0] == 0 and out[i][2] == 0:
            out[i] = tuple(list(out[i])[:-1] + [1])
        if top and out[i][0]>=4 and out[i][2]>=4:
            out[i] = tuple(list(out[i])[:-1] + [9])
    return out

def FinalString(t_cases, b_cases):
    out = ""
    for case in t_cases:
        out += f"t {' '.join([str(v) for v in case])}|"
    out = out[:-1] + "\n"
    for case in b_cases:
        out += f"b {' '.join([str(v) for v in case])}|"
    return out[:-1]

def Main():
    data = LoadData("water_tileset_constraints.py")
    top, bottom = SplitTopBottom(data)
    t_parsed_mcases = ParseMainCases(top)
    b_parsed_mcases = ParseMainCases(bottom)

    t_cases = OverrideWithSpecialCases(UnpackMainCases(t_parsed_mcases), True)
    b_cases = OverrideWithSpecialCases(UnpackMainCases(b_parsed_mcases), False)

    final_string = FinalString(t_cases, b_cases)
    
    print(final_string)
    WriteData("water_tile_dataset.txt", final_string)

Main()
