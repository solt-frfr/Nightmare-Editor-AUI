# Image Formats
 There's a variety of image formats used in the game. I will explain each of them. The "**Luminance/Alpha Combinations**" section is the most important for all users to read.

## Uncompressed RGB/RGBA Formats
 R:Red, G:Green, B:Blue, A:Alpha

### RGBA8888
 32 bpp

 R:8, G:8, B:8, A:8

 High-quality textures with full alpha.

#### 

### RGB888
 24 bpp

 R:8, G:8, B:8

 High-quality textures without alpha.

#### 
 The alpha channel will be ignored when encoding.
#### 
### RGBA5551
 16 bpp

 Layout: R:5, G:5, B:5, A:1

 Space-saving with minimal alpha support (on/off).

#### 
 The upper 5 bits of each color channel will be encoded.

 The uppermost bit of the alpha channel will be encoded.

#### 
### RGB565
 16 bpp

 Layout: R:5, G:6, B:5

 No alpha, slightly more accurate green channel due to human vision sensitivity.

#### 
 The upper 5 bits of the red and blue channels will be encoded.

 The upper 6 bits of the green channel will be encoded.

 The alpha channel will be ignored when encoding.

#### 
### RGBA4444
 16 bpp

 Layout: R:4, G:4, B:4, A:4

 Moderate compression, usable alpha channel.

#### 
 The upper 4 bits of each channel will be encoded.

#### 
## Luminance/Alpha Combinations
 L:Luminance, A:Alpha

### LA8
 16 bpp

 Layout: L:8, A:8
 
 Grayscale images with full alpha.

#### 
 Colors will be averaged into grayscale when encoding.

#### 
### HILO8
 16 bpp

 Layout: Hi:8, Lo:8 (not color-based).

 Technical; often not a visible image format.

#### 
 The red channel will be interpreted as the Hi channel in both encoding and decoding.

 The blue channel will be interpreted as the Lo channel in both encoding and decoding.

 The green and alpha channels ignored when encoding.

#### 
### L8
 8 bpp

 Layout: L:8

 Grayscale images.

#### 
 Colors will be averaged into grayscale when encoding.

 The alpha channel will be ignored when encoding.

#### 
### A8
 8 bpp

 Layout: A:8

 Opacity mask.

#### 
 The color channels will be ignored when encoding.

#### 
### LA4
 8 bpp

 Layout: L:4, A:4

 Grayscale with lower precision alpha.

#### 
 Colors will be averaged into grayscale when encoding.

 The upper 4 bits of the average will be encoded.

 The upper 4 bits of the alpha channel will be encoded.

#### 
### L4
 4 bpp

 Layout: L:4

 Low-memory grayscale.

#### 
 Colors will be averaged into grayscale when encoding.

 The upper 4 bits of the average will be encoded.

 The alpha channel will be ignored when encoding.

#### 
### A4
 4 bpp

 Layout: A:4

 Low-memory opacity mask.

#### 
 The color channels will be ignored when encoding.

 The upper 4 bits of the alpha channel will be encoded.

#### 
## Compressed Formats
### ETC1
 4 bpp

 Ericsson Texture Compression for RGB only (no alpha).

 4x4 blocks, 64 bits per block.

#### 
 Colors will be compressed. Please use texture linking to avoid repeated compression.

#### 
### ETC1A4
 8 bpp

 Layout: ETC1 block (4bpp) + separate alpha block (4bpp).

 Adds basic alpha to ETC1. Despite not being an actual format, I've seen it used in many 3DS games.

#### 
 Colors will be compressed. Please use texture linking to avoid repeated compression.

 The upper 4 bits of the alpha channel will be encoded.