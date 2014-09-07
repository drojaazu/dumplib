dumplib
========
dumplib is a .NET library for working with data dumped from other media, such as optical disks, with an emphasis on video game consoles.

Concepts and Objects
====================

MediaImage
-----------
The MediaImage object represents all of the data on a medium. Game console cartridge dumps (ROMs) or disk images can be MediaImages. dumplib contains a number of MediaImage derivatives for various classic game consoles: NES, SNES, Gameboy, Gameboy Advance, Virtual Boy, Famicom Disk System, Nintendo 64, Genesis/Megadrive, Gamegear, Master System, NeoGeo Pocket

IDumpConverter
--------------
Some devices that dump data from a medium to a file may alter the data to be compatible with that device in particular; this is especially true for the 16-bit era game copiers. The IDumpConverter interface contains a method to convert the data back to the original format on the medium. dumplib contains a number of IDumpConverter objects: SNES dumpers (Super Magicom, Super Wild Card, Pro Fighter), Famicom Disk System dump formats (Pasofami, fwNES), NES dump formats (iNES), Nintendo64 dumpers (CD64, Doctor V64), Genesis dumpers (Super Magicdrive)

ImageMap
--------
The ImageMap object is a representation of image map (.imp) files. Image maps define offsets and lengths inside a media image as particular types of data.

IColorConverter, IPaletteConverter, ITileConverter
-
