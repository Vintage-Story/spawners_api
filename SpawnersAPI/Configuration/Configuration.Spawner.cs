using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace SpawnersAPI;

public class SpawnerDropItemConfiguration
{
    public int chance = 100;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    public string code = "";
}

public class SpawnerDropGroupConfiguration
{
    public int chance = 100;
    public List<SpawnerDropItemConfiguration> codes = [];
}

public class SpawnerConfiguration
{
    public bool torchWillDisableSpawn = false;
    public bool spawnOnlyInGround = false;
    public bool spawnOnlyWith2Heights = false;
    public bool droppable = false;
    public bool freezeOnAllEntitiesSpawned = false;
    public double healthAdditional = 1.0;
    public double damageAdditional = 1.0;
    public List<string> entitiesToSpawn = ["game:drifter-normal"];
    public int lightLevel1 = 4;
    public int lightLevel2 = 7;
    public int lightLevel3 = 9;
    public int lightLevel4 = 13;
    public int maxSpawnedEntities = 20;
    public int maxEntitiesSpawnAtOnce = 4;
    public int xSpawnMaxDistance = 4;
    public int ySpawnMaxDistance = 2;
    public int zSpawnMaxDistance = 4;
    public int xPlayerDistanceToSpawn = 16;
    public int yPlayerDistanceToSpawn = 16;
    public int zPlayerDistanceToSpawn = 16;
    public int maxChancesToFindAValidBlockToSpawn = 30;
    public List<SpawnerDropGroupConfiguration> spawnerDrops = [];
    public bool extendedLogs = false;
}

public static partial class Configuration
{
    /// <summary>
    /// Loads the per-spawner-type configuration from ModConfig/SpawnersAPI/&lt;spawnerID&gt;.json,
    /// seeding a first-run file from this mod's bundled "spawnersapi:config/&lt;spawnerID&gt;.json" asset.
    /// </summary>
    public static SpawnerConfiguration LoadSpawnerConfiguration(ICoreAPI api, string spawnerID)
    {
        return ConfigManager.Load<SpawnerConfiguration>(
            api, "ModConfig/SpawnersAPI", spawnerID, Logger(api), $"spawnersapi:config/{spawnerID}.json");
    }
}
