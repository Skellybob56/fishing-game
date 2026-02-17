import json

def RemoveJsoncComments(string):
    out_str = ""
    in_string = False
    in_line_comment = False
    in_long_comment = False
    backslash = False
    asterix = False
    frontslash = False
    for char in string:
        if in_string:
            if backslash:
                backslash = False
            elif char == "\\":
                backslash = True
            elif char == '"':
                in_string = False
            out_str += char
        
        elif in_line_comment:
            if char == "\n":
                in_line_comment = False
        
        elif in_long_comment:
            if asterix and char == "/":
                in_long_comment = False
            asterix = False
            if char == "*":
                asterix = True
        
        else:
            if frontslash:
                frontslash = False
                if char == "/":
                    
                    in_line_comment = True
                    out_str = out_str[:-1]
                    continue
                if char == "*":
                    in_long_comment = True
                    out_str = out_str[:-1]
                    continue
            if char == "/":
                frontslash = True
            
            if backslash:
                backslash = False
            elif char == "\\":
                backslash = True
            elif char == '"':
                in_string = True
            out_str += char
    
    return out_str

with open("testmap.fgmap", "r") as file:
    data = file.read()

print(RemoveJsoncComments(data))
y = json.loads(RemoveJsoncComments(data))
print(y)

data = data.split('"data" : [')[1].split("]")[0].split("\n")
data = [[int(i) for i in s.strip().split(",") if i!=""] for s in data]
print(data)

for line in data:
    print(' '.join([".'~=#@"[i] for i in line]))
