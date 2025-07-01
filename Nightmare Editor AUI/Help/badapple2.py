excluded_numbers = [
    640,
    17024,
    33408,
    49792,
    66176,
    82560,
    98944,
    105088,
    111232,
    117376
]

frame_data = []

for i in range(1, 6574):
    if i not in excluded_numbers:
        frame_data.append({
            "Texture": i,
            "Length": 1,
            "Length2": 0
        })

output = {
    "Name": "EYE_BLINK",
    "Frames": frame_data
}

import json

import sys

if len(sys.argv) > 1:
    output_file = sys.argv[1]
else:
    output_file = "frame_data.json"

with open(output_file, "w") as f:
    json.dump(output, f, indent=2)

print("frame_data.json generated successfully.")