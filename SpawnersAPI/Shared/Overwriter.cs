using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Newtonsoft.Json.Linq;

namespace SpawnersAPI.Shared;
class Overwrite
{
    public Harmony overwriter;
    public void OverwriteNativeFunctions(ICoreAPI _)
    {
        if (!Harmony.HasAnyPatches("spawnersapi"))
        {
            overwriter = new Harmony("spawnersapi");
            overwriter.PatchCategory("spawnersapi");
            Debug.Log("Damage interaction has been overwrited");
        }
        else
        {
            Debug.Log("spawnersapi overwriter has already patched, probably by the singleplayer server");
        }
    }
}

#pragma warning disable IDE0060
[HarmonyPatchCategory("spawnersapi")]
class DamageInteraction
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AiTaskMeleeAttack), "LoadConfig")]
    public static void LoadConfig(AiTaskMeleeAttack __instance, ref JsonObject taskConfig, JsonObject aiConfig)
    {
        if (__instance.entity == null) return;
        if (__instance.entity.Attributes == null) return;

        float damage = taskConfig["damage"].AsFloat();
        if (damage == 0f || __instance.entity.Attributes.GetDouble("SpawnersAPIDamageIncrease") == 0) return;

        // Increase the damage
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("SpawnersAPIDamageIncrease"));

        string data = taskConfig.Token?.ToString();

        // Parsing the readonly object into editable object
        JObject jsonObject;
        try
        {
            jsonObject = JObject.Parse(data);
        }
        catch (Exception ex)
        {
            Debug.Log($"Invalid json for entity: {__instance.entity.Code}, exception: {ex.Message}");
            return;
        }

        // Checking if damage exist
        if (jsonObject.TryGetValue("damage", out JToken _))
        {
            // Redefining the damage
            jsonObject["damage"] = damage;
        }

        // Updating the json
        taskConfig = new(JToken.Parse(jsonObject.ToString()));
    }
}