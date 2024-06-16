using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace SpawnersAPI;

public class Spawner : BlockEntity
{
    private List<string> entitiesToSpawn = ["game:drifter-normal"];
    static public event Action<Entity> OnSpawnerSpawn;
    private int progressSpawn = 0;
    private long progressTickID = 0;
    private string spawnerID = "";
    #region behaviours
    private bool torchWillDisableSpawn = false;
    private int lightLevel1 = 4;
    private int lightLevel2 = 7;
    private int lightLevel3 = 9;
    private int lightLevel4 = 13;
    private int maxEntitiesSpawnAtOnce = 4;
    private int xSpawnMaxDistance = 4;
    private int ySpawnMaxDistance = 2;
    private int zSpawnMaxDistance = 4;
    private int xPlayerDistanceToSpawn = 16;
    private int yPlayerDistanceToSpawn = 16;
    private int zPlayerDistanceToSpawn = 16;
    private int maxChancesToFindAValidBlockToSpawn = 30;
    private bool extendedLogs = false;
    #endregion

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        progressTickID = RegisterGameTickListener(OnTickRate, 2000, 0);
        #region config-load
        spawnerID = Block.Code.ToString().Replace("spawnersapi:spawner-", "");
        Dictionary<string, object> baseConfigs = Api.Assets.Get(new AssetLocation($"spawnersapi:config/{spawnerID}.json")).ToObject<Dictionary<string, object>>();
        { //torchWillDisableSpawn
            if (baseConfigs.TryGetValue("torchWillDisableSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: torchWillDisableSpawn is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: torchWillDisableSpawn is not boolean is {value.GetType()}");
                else torchWillDisableSpawn = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: torchWillDisableSpawn not set");
        }
        { //entitiesToSpawn
            if (baseConfigs.TryGetValue("entitiesToSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: entitiesToSpawn is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: entitiesToSpawn is not List is {value.GetType()}");
                else entitiesToSpawn = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: entitiesToSpawn not set");
        }
        { //lightLevel1
            if (baseConfigs.TryGetValue("lightLevel1", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel1 is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel1 is not int is {value.GetType()}");
                else lightLevel1 = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: lightLevel1 not set");
        }
        { //lightLevel2
            if (baseConfigs.TryGetValue("lightLevel2", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel2 is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel2 is not int is {value.GetType()}");
                else lightLevel2 = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: lightLevel2 not set");
        }
        { //lightLevel3
            if (baseConfigs.TryGetValue("lightLevel3", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel3 is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel3 is not int is {value.GetType()}");
                else lightLevel3 = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: lightLevel3 not set");
        }
        { //lightLevel4
            if (baseConfigs.TryGetValue("lightLevel4", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel4 is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel4 is not int is {value.GetType()}");
                else lightLevel4 = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: lightLevel4 not set");
        }
        { //maxEntitiesSpawnAtOnce
            if (baseConfigs.TryGetValue("maxEntitiesSpawnAtOnce", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maxEntitiesSpawnAtOnce is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxEntitiesSpawnAtOnce is not int is {value.GetType()}");
                else maxEntitiesSpawnAtOnce = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: maxEntitiesSpawnAtOnce not set");
        }
        { //xSpawnMaxDistance
            if (baseConfigs.TryGetValue("xSpawnMaxDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: xSpawnMaxDistance is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: xSpawnMaxDistance is not int is {value.GetType()}");
                else xSpawnMaxDistance = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: xSpawnMaxDistance not set");
        }
        { //ySpawnMaxDistance
            if (baseConfigs.TryGetValue("ySpawnMaxDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: ySpawnMaxDistance is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: ySpawnMaxDistance is not int is {value.GetType()}");
                else ySpawnMaxDistance = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: ySpawnMaxDistance not set");
        }
        { //zSpawnMaxDistance
            if (baseConfigs.TryGetValue("zSpawnMaxDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: zSpawnMaxDistance is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: zSpawnMaxDistance is not int is {value.GetType()}");
                else zSpawnMaxDistance = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: zSpawnMaxDistance not set");
        }
        { //xPlayerDistanceToSpawn
            if (baseConfigs.TryGetValue("xPlayerDistanceToSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: xPlayerDistanceToSpawn is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: xPlayerDistanceToSpawn is not int is {value.GetType()}");
                else xPlayerDistanceToSpawn = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: xPlayerDistanceToSpawn not set");
        }
        { //yPlayerDistanceToSpawn
            if (baseConfigs.TryGetValue("yPlayerDistanceToSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: yPlayerDistanceToSpawn is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: yPlayerDistanceToSpawn is not int is {value.GetType()}");
                else yPlayerDistanceToSpawn = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: yPlayerDistanceToSpawn not set");
        }
        { //zPlayerDistanceToSpawn
            if (baseConfigs.TryGetValue("zPlayerDistanceToSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: zPlayerDistanceToSpawn is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: zPlayerDistanceToSpawn is not int is {value.GetType()}");
                else zPlayerDistanceToSpawn = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: zPlayerDistanceToSpawn not set");
        }
        { //maxChancesToFindAValidBlockToSpawn
            if (baseConfigs.TryGetValue("maxChancesToFindAValidBlockToSpawn", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn is not int is {value.GetType()}");
                else maxChancesToFindAValidBlockToSpawn = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn not set");
        }
        { //extendedLogs
            if (baseConfigs.TryGetValue("extendedLogs", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: extendedLogs is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: extendedLogs is not boolean is {value.GetType()}");
                else extendedLogs = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: extendedLogs not set");
        }
        #endregion
    }

    private void OnTickRate(float obj)
    {
        // Check block existance
        if (!Api.World.BlockAccessor.GetBlock(Pos).Code.ToString().Contains("spawnersapi:spawner"))
        {
            if (extendedLogs)
                Debug.Log($"{Block.Code} doesn't exist anymore removing tickrate");
            UnregisterGameTickListener(progressTickID);
            return;
        };

        if (extendedLogs) Debug.Log($"{Block.Code} validating...");
        #region check-near-torchs
        if (torchWillDisableSpawn)
        {
            { // North torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (receivedBlock.Code.ToString() == "game:torch-basic-lit-up") return;
            }

            { // South torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (receivedBlock.Code.ToString() == "game:torch-basic-lit-up") return;
            }

            { // East torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (receivedBlock.Code.ToString() == "game:torch-basic-lit-up") return;
            }

            { // West torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (receivedBlock.Code.ToString() == "game:torch-basic-lit-up") return;
            }

            { // On the ground torch detection, used when the spawner is flying
                BlockPos newPosition = Pos.Copy();
                newPosition.Y -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (receivedBlock.Code.ToString() == "game:torch-basic-lit-up") return;
            }
        }
        #endregion
        if (extendedLogs) Debug.Log($"{Block.Code} torchWillDisableSpawn: {torchWillDisableSpawn}, validated");
        #region check-near-players
        IPlayer nearestPlayer = Api.World.NearestPlayer(Pos.X, Pos.Y, Pos.Z);
        if (nearestPlayer == null) return;
        else
        {
            // Getting positions
            double playerX = nearestPlayer.Entity.Pos.X;
            double playerY = nearestPlayer.Entity.Pos.Y;
            double playerZ = nearestPlayer.Entity.Pos.Z;
            double blockX = Pos.X;
            double blockY = Pos.Y;
            double blockZ = Pos.Z;

            // X Calculation
            double xDistance;
            if (playerX < blockX) xDistance = blockX - playerX;
            else xDistance = playerX - blockX;

            // Y Calcuation
            double yDistance;
            if (playerY < blockY) yDistance = blockY - playerY;
            else yDistance = playerY - blockY;

            // Z Calculation
            double zDistance;
            if (playerZ < blockZ) zDistance = blockZ - playerZ;
            else zDistance = playerY - blockY;

            // Distance check
            if (xDistance > xPlayerDistanceToSpawn || yDistance > yPlayerDistanceToSpawn || zDistance > zPlayerDistanceToSpawn)
                return;
        }
        #endregion
        if (extendedLogs) Debug.Log($"{Block.Code} theres a player near the spawn, validated");
        // Progress calculation based on the light level of the block
        int lightLevel = Api.World.BlockAccessor.GetLightLevel(Pos, EnumLightLevelType.MaxLight);
        if (lightLevel <= lightLevel1) progressSpawn += 10;
        else if (lightLevel <= lightLevel2) progressSpawn += 5;
        else if (lightLevel <= lightLevel3) progressSpawn += 3;
        else if (lightLevel < lightLevel4) progressSpawn += 2;
        else progressSpawn = 0;
        if (extendedLogs) Debug.Log($"{Block.Code} progress: {progressSpawn}, lightLevel: {lightLevel}");

        // Check final progress
        if (progressSpawn >= 100)
        {
            SpawnEntities();
            progressSpawn = 0;
        }
    }

    private void SpawnEntities()
    {
        if (extendedLogs) Debug.Log($"Entity Spawning...");
        float minX = Pos.X - xSpawnMaxDistance;
        float minY = Pos.Y - ySpawnMaxDistance;
        float minZ = Pos.Z - zSpawnMaxDistance;
        float maxX = Pos.X + xSpawnMaxDistance;
        float maxY = Pos.Y + ySpawnMaxDistance;
        float maxZ = Pos.Z + zSpawnMaxDistance;
        int maxChances = maxChancesToFindAValidBlockToSpawn;
        byte entitiesSpawned = 0;
        Random random = new();
        for (int i = 0; i < maxChances; i++)
        {
            i++;
            // Get a random block position
            int spawnX = random.Next((int)minX, (int)maxX);
            int spawnY = random.Next((int)minY, (int)maxY);
            int spawnZ = random.Next((int)minZ, (int)maxZ);
            // Get the block with the random position
            BlockPos spawnBlockPos = new(spawnX, spawnY, spawnZ, 0);
            Block spawnBlock = Api.World.BlockAccessor.GetBlock(spawnBlockPos);
            // Check if block is free
            if (spawnBlock.Code.ToString() == "game:air")
            {
                // Now we need to check the if he has free space upper
                Block upperBlock = Api.World.BlockAccessor.GetBlock(spawnBlockPos);
                if (upperBlock.Code.ToString() == "game:air")
                {
                    entitiesSpawned++;

                    // Getting Entity info
                    int selectedEntity = random.Next(0, entitiesToSpawn.Count); // Select a random entity
                    EntityProperties type = Api.World.GetEntityType(new AssetLocation(entitiesToSpawn[selectedEntity]));
                    // Check if entity exist
                    if (type == null)
                    {
                        Debug.Log($"ERROR: the entity from {Block.Code} is null: {entitiesToSpawn[selectedEntity]}");
                        break;
                    }

                    if (extendedLogs) Debug.Log($"valid position to spawn, entity spawning: {entitiesToSpawn[selectedEntity]}");

                    // Instanciating
                    Entity entity = Api.World.ClassRegistry.CreateEntity(type);
                    entity.ServerPos.X = spawnX + 0.5;
                    entity.ServerPos.Y = spawnY;
                    entity.ServerPos.Z = spawnZ + 0.5;
                    entity.Pos.SetPos(entity.ServerPos);
                    entity.Attributes.SetBool("SpawnersAPI_Is_From_Spawner", true);
                    // Spawning
                    Api.World.SpawnEntity(entity);
                    OnSpawnerSpawn?.Invoke(entity);

                    // Limit of 4 entities spawned at once
                    if (entitiesSpawned >= maxEntitiesSpawnAtOnce) break;
                }
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetInt("progressSpawn", progressSpawn);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        progressSpawn = tree.GetInt("progressSpawn");
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        UnregisterGameTickListener(progressTickID);
    }
}