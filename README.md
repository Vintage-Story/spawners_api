# Spawners API
Create custom spawners into creative inventory and for mods usages, this will not create any type of generations or structures with it, this is a simple API to be used among other mods.

How it works?
- The spawners will work as long as the ambient light is less than 13. (Single torch in the spawner side on full dark place will stop the spawner)
- The darker the more fast spawners will work.
- Spawners doesn't have a entity limit, will continue spawning infinitly.
- Spawners will only work if theres is a player in 16 blocks XYZ.

# About Spawners API
Spawners API is open source project and can easily be accessed on the github, all contents from this mod is completly free.

If you want to contribute into the project you can access the project github and make your pull request.

You are free to fork the project and make your own version of Spawners API, as long the name is changed.

Inspirations:

Vanilla Minecraft Spawners

# Building
Learn more about vintage story modding in Linux or Windows

Download the mod template for vintage store with name SpawnersAPI and paste all contents from this project in there

Linux

> Make a symbolic link for fast tests

ln -s /path/to/project/Releases/spanersapi/* /path/to/game/Mods/SpawnersAPI/
Execute the comamnd ./build.sh, consider having setup everthing from vintage story ide before

> Windows

Just open the visual studio with LevelUP.sln

FTM License