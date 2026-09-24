# Texture Formats

Ever wonder what those strange letters mean? Here's a breakdown.

These are all the channels:

- R for Red

- G for Green

- B for Blue

- A for Alpha

- L for Luminance (Grayscale)

- LO for Low

- HI for High

- ETC1 for Ericsson Texture Compression, version 1

Each format has a combination of one or more of these channels. The numbers that follow represent the number of bits for that channel, aside from ETC1.

RGBA8888 means it has a Red, Green, Blue, and Alpha channel, with each pixel having 8 bits per channel.

ETC1 and ETC1A4 are special. For each "block" of 4x4 pixels, These have 8 bytes of compressed ETC1 data, and for ETC1A4, followed by a "block" of 4-bit alphas.



The 3DS has 14 different texture formats, and they go as follows.

- RGBA8888

- RGB888

- RGBA5551

- RGB565

- RGBA4444

- LA88

- HILO88

- L8

- A8

- LA44

- L4

- A4

- ETC1

- ETC1A4
