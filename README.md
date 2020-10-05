# chip8sharp-unity
A CHIP-8 emulator built in Unity. Configured to work with PC, Mac, Linux Standalone and WebGL. Some ROMs come preloaded and on standalone versions you can upload any of you own ROMs.

Playable WebGL link: https://simmer.io/@work4games/chip-8-emulator

![Game](https://user-images.githubusercontent.com/26587807/95029009-806a5e80-067f-11eb-8c92-e64e1e3dbb6a.PNG)

## Input
Requires keyboard and mouse. Press "esc" or "backspace" to exit a game and be brought back to the title screen. 

Chip 8 keypad to keyboard controls:

![Controls](https://user-images.githubusercontent.com/26587807/95028970-2bc6e380-067f-11eb-8607-e6a88a8e88ff.PNG)

Touch controls can easily be added by implementing an IUserInput interface.

## Unit Tests
Using Unity's TestRunner, unit tests were created to test every opcode.

![Unity](https://user-images.githubusercontent.com/26587807/95028974-2f5a6a80-067f-11eb-8903-8de62ce2670c.PNG)

## 3rd Party Packages
I use SimpleFileBrowser to handle selecting ROM files for standlone version: https://github.com/yasirkula/UnitySimpleFileBrowser

## ROMs
ROMs were taken from here: https://github.com/dmatlack/chip8/tree/master/roms/games
