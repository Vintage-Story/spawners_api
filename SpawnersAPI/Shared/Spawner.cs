using System;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace SpawnersAPI;

public class Spawner : BlockEntity
{
    static public event Action<Entity> OnSpawnerSpawn;
    private int progressSpawn = 0;
    private long progressTickID = 0;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        progressTickID = RegisterGameTickListener(OnTickRate, 2000, 0);
    }

    private void OnTickRate(float obj)
    {
        // Progress calculation based on the light level of the block
        int lightLevel = Api.World.BlockAccessor.GetLightLevel(Pos, EnumLightLevelType.MaxLight);
        if (lightLevel <= 5) progressSpawn += 10;
        else if (lightLevel <= 10) progressSpawn += 5;
        else if (lightLevel <= 15) progressSpawn += 3;
        else if (lightLevel <= 25) progressSpawn += 1;
        else progressSpawn = 0;

        // Check final progress
        if (progressSpawn >= 100)
        {
            SpawnEntities();
            progressSpawn = 0;
        }
    }

    private void SpawnEntities()
    {
        float minX = Pos.X - 4;
        float minY = Pos.Y - 2;
        float minZ = Pos.Z - 4;
        float maxX = Pos.X + 4;
        float maxY = Pos.Y + 2;
        float maxZ = Pos.Z + 4;
        byte maxChances = 30;
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
                    EntityProperties type = Api.World.GetEntityType(new AssetLocation(Block.Code.ToString().Replace("spawnersapi:spawner-", "")));
                    // Check if entity exist
                    if (type == null)
                    {
                        Debug.Log($"ERROR: the entity from {Block.Code} is null: {Block.Code.ToString().Replace("spawnersapi:spawner-", "")}");
                        break;
                    }
                    // Instanciating
                    Entity entity = Api.World.ClassRegistry.CreateEntity(type);
                    entity.ServerPos.X = spawnX;
                    entity.ServerPos.Y = spawnY;
                    entity.ServerPos.Z = spawnZ;
                    entity.Pos.SetPos(entity.ServerPos);
                    entity.Attributes.SetBool("SpawnersAPI_Is_From_Spawner", true);
                    // Spawning
                    Api.World.SpawnEntity(entity);
                    OnSpawnerSpawn?.Invoke(entity);

                    // Limit of 4 entities spawned at once
                    if (entitiesSpawned >= 4) break;
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