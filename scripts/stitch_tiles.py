from PIL import Image
import os
import sys
import glob

# Increase PIL image size limits to handle 32K images
# This disables the DecompressionBombWarning and removes the pixel limit
Image.MAX_IMAGE_PIXELS = None

def stitch_tiles(input_folder, output_filename, final_size=32768, tile_size=4096):
    """
    Stitches together tiles created by the TiledScreenshotTool Unity script.
    
    Args:
        input_folder: Folder containing the tile images (named tile_x{x}_z{z}.png)
        output_filename: Path where the stitched image will be saved
        final_size: Size of the final image (default: 32K)
        tile_size: Size of each tile (default: 4K)
    """
    print(f"Will create a {final_size}x{final_size} image from {tile_size}x{tile_size} tiles")
    
    # Calculate how many tiles per side
    tiles_per_side = final_size // tile_size
    if final_size % tile_size != 0:
        tiles_per_side += 1
        print(f"Warning: Final size {final_size} is not a multiple of tile size {tile_size}.")
        print(f"Using {tiles_per_side} tiles per side, which might result in extra padding.")
    
    print(f"Tiles per side: {tiles_per_side}")
    
    # Create a new image with the final size
    try:
        stitched_image = Image.new('RGB', (final_size, final_size))
        print(f"Created blank canvas of size {final_size}x{final_size}")
    except Exception as e:
        print(f"Error creating image: {e}")
        return
    
    # Find and sort all tile files
    tiles_found = 0
    
    print(f"Looking for tiles in {input_folder}...")
    
    # Process each tile
    for z in range(tiles_per_side):
        for x in range(tiles_per_side):
            tile_filename = os.path.join(input_folder, f"tile_x{x}_z{z}.png")
            
            if not os.path.exists(tile_filename):
                print(f"Warning: Missing tile at position x={x}, z={z}")
                continue
                
            try:
                # Open the tile
                tile = Image.open(tile_filename)
                
                # Calculate position for this tile in the final image
                x_pos = x * tile_size
                # FIXED: Invert the vertical position so z=0 is at the top of the image
                y_pos = (tiles_per_side - 1 - z) * tile_size
                
                # Paste tile into the stitched image
                stitched_image.paste(tile, (x_pos, y_pos))
                
                # Close the tile to free memory
                tile.close()
                
                tiles_found += 1
                if tiles_found % 10 == 0:
                    print(f"Processed {tiles_found} tiles...")
                
            except Exception as e:
                print(f"Error processing tile {tile_filename}: {e}")
    
    print(f"Successfully processed {tiles_found} tiles.")
    
    # Save the stitched image
    try:
        print(f"Saving stitched image to {output_filename} (may take a minute)...")
        stitched_image.save(output_filename, quality=95)
        print(f"Successfully saved stitched image to {output_filename}")
    except Exception as e:
        print(f"Error saving stitched image: {e}")
        
    # Clean up
    stitched_image.close()
    
if __name__ == "__main__":
    # Default parameters
    input_folder = "C:/temp/tiles"
    output_filename = "C:/temp/stitched_32k.png"
    final_size = 32768  # 32K
    tile_size = 4096    # 4K
    
    # Check for command-line arguments
    if len(sys.argv) > 1:
        input_folder = sys.argv[1]
    if len(sys.argv) > 2:
        output_filename = sys.argv[2]
    if len(sys.argv) > 3:
        try:
            final_size = int(sys.argv[3])
        except:
            print(f"Invalid final size: {sys.argv[3]}, using default {final_size}")
    if len(sys.argv) > 4:
        try:
            tile_size = int(sys.argv[4])
        except:
            print(f"Invalid tile size: {sys.argv[4]}, using default {tile_size}")
    
    print("Tile Stitcher - Combines tiles created by TiledScreenshotTool")
    print(f"Input folder: {input_folder}")
    print(f"Output file: {output_filename}")
    
    # Stitch the tiles
    stitch_tiles(input_folder, output_filename, final_size, tile_size)
