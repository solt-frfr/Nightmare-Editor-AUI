import os
import sys
from PIL import Image

def downscale_image(image_path, output_path, size=(128, 64)):
    try:
        with Image.open(image_path) as img:
            img = img.resize(size, Image.LANCZOS)
            img.save(output_path)
            print(f"Saved {output_path}")
    except Exception as e:
        print(f"Failed to process {image_path}: {e}")

def process_folder(folder_path):
    if not os.path.isdir(folder_path):
        print(f"{folder_path} is not a valid directory")
        return

    output_folder = os.path.join(folder_path, "downscaled")
    os.makedirs(output_folder, exist_ok=True)

    for filename in os.listdir(folder_path):
        file_path = os.path.join(folder_path, filename)
        if os.path.isfile(file_path):
            output_path = os.path.join(output_folder, os.path.splitext(filename)[0] + ".png")
            downscale_image(file_path, output_path)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python downscale_images.py <folder_path>")
    else:
        for folder_path in sys.argv[1:]:
            process_folder(folder_path)
