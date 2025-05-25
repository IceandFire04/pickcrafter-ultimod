# Ultimod
Ultamod is (I'm pretty sure) the first mod for PickCrafter (PC) that runs on MelonLoader. It adds a couple cheats to the game to show examples of what can be done using MelonLoader. I made this because I thought this would be a fun side-project and now I'm thinking of expanding on it in the future!

## How to Install

- Download [MelonLoader](https://github.com/LavaGang/MelonLoader)
- Set PickCrafter for the game you want to install it on
- Download the mod from releases
- Put the mod in the `Mods` folder of PickCrafter's files (usually `C:\Program Files (x86)\Steam\steamapps\common\PickCrafter\Mods`)

This is very simplified so feel free to google a tutorial on how to install it in more detail.

## Features
There are a variety of features introduced in Ultimod:

- All pickaxes are unlocked (does not modify save file)
- Chests instantly unlock
- Pickaxe powers have a 10 second cooldown
- Pressing the `F#` keys can do a number of things
  - `F1`: Prints all the `PickaxeControllers` to the console (used for debugging)
  - `F2`: Toggles debug info about your current Pickaxe
  - `F3`: Does the short chest cooldown thing from before
  - `F4`: Gives you 15 runic

## Developing
All the code is basically right there and ready to modify, but there a few things to know:
- **Dependencies**: Most of the dependencies can be found in `C:\Program Files (x86)\Steam\steamapps\common\PickCrafter\MelonLoader\Il2CppAssemblies` or `C:\Program Files (x86)\Steam\steamapps\common\PickCrafter\MelonLoader\net6`.
- **Source Code**: If you want to look at the game's "source code," download [dnSpy](https://github.com/dnSpy/dnSpy) and go to`C:\Program Files (x86)\Steam\steamapps\common\PickCrafter\MelonLoader\Il2CppAssemblies` and open `Assembly-CSharp.dll`. This is how you can find methods and stuff to override and change.
- **Where did my build go?**: Most likely `[Project Directory]\Ultimod\bin\Dev\net6.0`. `Dev` may be different, but the file should always be in that directory if it compiled correctly.

## The Future
I think I'll just make some refined library mod sometime soon now that I know what I'm doing. That's about all I really have to say though.
