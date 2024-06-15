# Spawners API
Create custom spawners into creative inventory and for mods usages, this will not create any type of generations or structures with it, this is a simple API to be used among other mods.

How it works?

You can patch the json assets/spawnersapi/blocktypes/spawner.json to add your own code creature take a look in the default values:
```
"states": [
    "game:drifter-normal",
    "game:drifter-deeper",
    "game:drifter-tainted",
    "game:drifter-corrupt",
    "game:drifter-nightmare",
    "game:drifter-double-headed"
]
```
You can easily patch this json in your mod to add a new spawner with the entity you want, the json will automatically create a new spawner in creative tab with the new entity

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