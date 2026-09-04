using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SpawnersAPI;

public class Spawner : BlockEntity
{
    static public event Action<Entity> OnSpawnerSpawn;
    private List<long> entitiesAlive = [];
    private List<string> entitiesToSpawn = ["game:drifter-normal"];
    private int progressSpawn = 0;
    private long progressTickID = 0;
    private string spawnerID = "";
    #region behaviours
    private bool torchWillDisableSpawn = false;
    private bool spawnOnlyInGround = false;
    private bool spawnOnlyWith2Heights = false;
    private bool droppable = false;
    private bool freezeOnAllEntitiesSpawned = false;
    private double healthAdditional = 1.0;
    private double damageAdditional = 1.0;
    private int lightLevel1 = 4;
    private int lightLevel2 = 7;
    private int lightLevel3 = 9;
    private int lightLevel4 = 13;
    private int maxSpawnedEntities = 20;
    private int maxEntitiesSpawnAtOnce = 4;
    private int xSpawnMaxDistance = 4;
    private int ySpawnMaxDistance = 2;
    private int zSpawnMaxDistance = 4;
    private int xPlayerDistanceToSpawn = 16;
    private int yPlayerDistanceToSpawn = 16;
    private int zPlayerDistanceToSpawn = 16;
    private int maxChancesToFindAValidBlockToSpawn = 30;
    private List<SpawnerDropGroupConfiguration> spawnerDrops = [];
    private bool extendedLogs = false;
    #endregion

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        // Clients does not need to register a game tick listener
        if (api.Side != EnumAppSide.Client)
            progressTickID = RegisterGameTickListener(OnTickRate, 2000, 0);

        #region config-load
        spawnerID = Block.Code.ToString().Replace("spawnersapi:spawner-", "");
        SpawnerConfiguration config = Configuration.LoadSpawnerConfiguration(Api, spawnerID);
        torchWillDisableSpawn = config.torchWillDisableSpawn;
        spawnOnlyInGround = config.spawnOnlyInGround;
        spawnOnlyWith2Heights = config.spawnOnlyWith2Heights;
        droppable = config.droppable;
        freezeOnAllEntitiesSpawned = config.freezeOnAllEntitiesSpawned;
        healthAdditional = config.healthAdditional;
        damageAdditional = config.damageAdditional;
        entitiesToSpawn = config.entitiesToSpawn;
        lightLevel1 = config.lightLevel1;
        lightLevel2 = config.lightLevel2;
        lightLevel3 = config.lightLevel3;
        lightLevel4 = config.lightLevel4;
        maxSpawnedEntities = config.maxSpawnedEntities;
        maxEntitiesSpawnAtOnce = config.maxEntitiesSpawnAtOnce;
        xSpawnMaxDistance = config.xSpawnMaxDistance;
        ySpawnMaxDistance = config.ySpawnMaxDistance;
        zSpawnMaxDistance = config.zSpawnMaxDistance;
        xPlayerDistanceToSpawn = config.xPlayerDistanceToSpawn;
        yPlayerDistanceToSpawn = config.yPlayerDistanceToSpawn;
        zPlayerDistanceToSpawn = config.zPlayerDistanceToSpawn;
        maxChancesToFindAValidBlockToSpawn = config.maxChancesToFindAValidBlockToSpawn;
        spawnerDrops = config.spawnerDrops;
        extendedLogs = config.extendedLogs;

        if (!droppable) Block.Drops = [];
        #endregion
    }

    private void OnTickRate(float obj)
    {
        // Checking entities alive
        for (int i = entitiesAlive.Count - 1; i >= 0; i--)
        {
            long id = entitiesAlive[i];
            Entity entity = Api.World.GetEntityById(id);
            if (entity is not null)
            {
                if (!entity.Alive) entitiesAlive.RemoveAt(i);
            }
            else entitiesAlive.RemoveAt(i);
        }
        // Getting the block
        Block spawnerBlock = Api.World.BlockAccessor.GetBlock(Pos);
        // Check the freeze on all entities spawned condition
        if (freezeOnAllEntitiesSpawned && entitiesAlive.Count >= maxSpawnedEntities)
        {
            if (extendedLogs)
                Debug.Log($"Progress is freezed, entities alive: {entitiesAlive.Count}");
            return;
        };
        // Check block existance
        if (spawnerBlock == null || !spawnerBlock.Code.ToString().Contains("spawnersapi:spawner"))
        {
            if (extendedLogs)
                Debug.Log($"{Block.Code} doesn't exist anymore removing tickrate");
            if (progressTickID != 0)
                UnregisterGameTickListener(progressTickID);
            return;
        };

        if (extendedLogs) Debug.Log($"{Block.Code} Validation Process");
        #region check-near-torchs
        if (torchWillDisableSpawn)
        {
            { // North torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} North Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // South torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} South Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // East torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} East Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // West torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} West Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // On the ground torch detection, used when the spawner is flying
                BlockPos newPosition = Pos.Copy();
                newPosition.Y -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} Ground Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // On the top torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Y += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} Top Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }
        }
        if (extendedLogs) Debug.Log($"{Block.Code} check-near-torchs: OK, config: {torchWillDisableSpawn}");
        #endregion
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

            if (extendedLogs) Debug.Log($"{Block.Code} check-near-players: OK, XYZ: {(int)xDistance}, {(int)yDistance}, {(int)zDistance}");
        }
        #endregion
        #region check-max-entities
        // Getting entities that is no longer alive
        List<long> entitiesToRemove = [];
        foreach (long entityId in entitiesAlive)
        {
            Entity entity = Api.World.GetEntityById(entityId);
            // Check the existance in the world
            if (entity == null)
            {
                entitiesToRemove.Add(entityId);
                continue;
            }
            // Check if is dead
            if (!entity.Alive)
            {
                entitiesToRemove.Add(entityId);
                continue;
            }
        }
        // Removing entities
        foreach (long entityId in entitiesToRemove) entitiesAlive.Remove(entityId);
        // Disabling spawner if the max entities reached
        if (entitiesAlive.Count >= maxSpawnedEntities) return;
        if (extendedLogs) Debug.Log($"{Block.Code} check-max-entities OK, quantity: {entitiesAlive.Count}");
        #endregion

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
                // Checking if the upper of the spawner is air
                if (spawnOnlyWith2Heights)
                {
                    BlockPos upperPosition = spawnBlockPos;
                    upperPosition.Y += 1;
                    Block upperBlock = Api.World.BlockAccessor.GetBlock(upperPosition);
                    if (upperBlock.Code.ToString() != "game:air") continue;
                }

                // Checking if the ground of the entity spawning is not air
                if (spawnOnlyInGround)
                {
                    BlockPos downPosition = spawnBlockPos;
                    downPosition.Y -= 1;
                    Block downBlock = Api.World.BlockAccessor.GetBlock(downPosition);
                    if (downBlock.Code.ToString() == "game:air") continue;
                }


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
                // Setting the variable to be increased the damage
                entity.Attributes.SetDouble("SpawnersAPIDamageIncrease", damageAdditional);
                entity.Attributes.SetDouble("SpawnersAPIHealthIncrease", healthAdditional);
                entity.Pos.X = spawnX + 0.5;
                entity.Pos.Y = spawnY;
                entity.Pos.Z = spawnZ + 0.5;
                entity.Attributes.SetBool("SpawnersAPI_Is_From_Spawner", true);
                // Spawning
                Api.World.SpawnEntity(entity);
                OnSpawnerSpawn?.Invoke(entity);
                entitiesAlive.Add(entity.EntityId);
                UpdateEntityStatus(entity);

                // Limit of maxSpawnedEntities once
                if (entitiesSpawned >= maxEntitiesSpawnAtOnce || entitiesAlive.Count >= maxSpawnedEntities) break;
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetInt("progressSpawn", progressSpawn);
        tree.SetString("entitiesAlive", string.Join(",", entitiesAlive));
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        // Load progress
        progressSpawn = tree.GetInt("progressSpawn");

        // Load spawned entities
        string entitiesSaved = tree.GetString("entitiesAlive");
        if (entitiesSaved != null && entitiesSaved != "")
            entitiesAlive = entitiesSaved.Split(",").Select(long.Parse).ToList();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        if (progressTickID != 0)
            UnregisterGameTickListener(progressTickID);
    }

    public override void OnBlockBroken(IPlayer byPlayer = null)
    {
        base.OnBlockBroken(byPlayer);
        // Particles when breaking
        SimpleParticleProperties props = new(15f, 22f, ColorUtil.ToRgba(150, 0, 0, 0), new Vec3d(Pos.X, Pos.Y, Pos.Z), new Vec3d(Pos.X + 1, Pos.Y + 1, Pos.Z + 1), new Vec3f(-0.2f, -0.1f, -0.2f), new Vec3f(0.2f, 0.2f, 0.2f), 1.5f, 0f, 0.5f, 1f, EnumParticleModel.Quad)
        {
            OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -200f),
            SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 2f)
        };
        Api.World.SpawnParticles(props);
        SimpleParticleProperties spiders = new(8f, 16f, ColorUtil.ToRgba(255, 50, 50, 50), new Vec3d(Pos.X, Pos.Y, Pos.Z), new Vec3d(Pos.X + 1, Pos.Y + 1, Pos.Z + 1), new Vec3f(-2f, -0.3f, -2f), new Vec3f(2f, 1f, 2f), 1f, 0.5f, 0.5f, 1.5f);
        Api.World.SpawnParticles(spiders);

        // Loot drops
        foreach (ItemStack itemStack in GetItemDrops())
            Api.World.SpawnItemEntity(itemStack, Pos.ToVec3d());

    }

    private List<ItemStack> GetItemDrops()
    {
        if (extendedLogs) Debug.Log("starting loot drop calculation");
        Random random = new();
        List<SpawnerDropItemConfiguration> drops = null;
        { // Getting the drops by chance
            // This is used to increase performance
            // when the chance is too low and cannot get any item
            // we will increase all chance drop rates to finally find one
            int chanceIncreaser = 0;
            while (true)
            {
                // Swipe all drop lists
                foreach (SpawnerDropGroupConfiguration drop in spawnerDrops)
                {
                    int chance = drop.chance + chanceIncreaser;
                    if (chance >= random.Next(0, 100))
                    {
                        // Adding the drops to the drops table
                        drops = drop.codes;
                        break;
                    }
                }
                // Checking if we finded the drop
                if (drops != null) break;
                chanceIncreaser += 10;
            }
        }

        List<ItemStack> items = [];
        // Swiping every drop from the drops array
        foreach (SpawnerDropItemConfiguration drop in drops)
        {
            if (random.Next(0, 100) <= drop.chance)
            {
                AssetLocation code = new(drop.code);
                ItemStack item;

                // Item
                try
                {
                    item = new(Api.World.GetItem(code))
                    {
                        StackSize = random.Next(drop.minQuantity, drop.maxQuantity + 1)
                    };
                    items.Add(item);
                    continue;
                }
                catch (Exception) { }

                // Block
                try
                {
                    item = new(Api.World.GetBlock(code))
                    {
                        StackSize = random.Next(drop.minQuantity, drop.maxQuantity + 1)
                    };
                    items.Add(item);
                    continue;
                }
                catch (Exception) { }

                // Invalid
                Debug.Log($"ERROR: Cannot retrieve spawner drop because {drop.code} does not exist");
            }
        }
        return items;
    }

    private void UpdateEntityStatus(Entity entity)
    {
        EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();
        if (entityLifeStats == null) return;
        if (entityLifeStats.BaseMaxHealth <= 0f || entityLifeStats.MaxHealth <= 0f || entityLifeStats.Health <= 0f) return;

        entityLifeStats.BaseMaxHealth *= (float)healthAdditional;
        entityLifeStats.MaxHealth *= (float)healthAdditional;
        entityLifeStats.Health *= (float)healthAdditional;

        // Looking for the damage? is on Overwrite.cs
    }
}