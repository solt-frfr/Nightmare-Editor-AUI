# Quirks
## ETC.exe and Paths with Periods
![Qrk1](Help/Images/Qrk1.png)

 ETC.exe cuts the filepath at the first period in order to have a smooth filename. Therefore, if the path containing the program has a period at any point, it will break when trying to encode etc textures.