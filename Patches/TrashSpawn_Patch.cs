using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;

namespace SweepersHeaven.Patches;

[HarmonyPatch(typeof(TrashSpawn))]
public class TrashSpawn_Patch
{
    // Store trash ids
    private static List<TrashSpawn> trashes = new List<TrashSpawn>();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TrashSpawn.CreateTrash))]
    public static void CreateTrash(TrashSpawn __instance)
    {
        if (!Plugin.Instance.PickUpAllTrashAtOnce) return;

        trashes.Add(__instance);

        Plugin.Logger.LogInfo($"Trash count: {trashes.Count}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TrashSpawn.CmdClearTrash))]
    public static void CmdClearTrash(TrashSpawn __instance)
    {
        if (!Plugin.Instance.PickUpAllTrashAtOnce) return;

        if (trashes.Contains(__instance)) trashes.Remove(__instance);

        if (trashes.Count == 0) return;

        TrashSpawn trashToRemove = trashes.First();
        Plugin.Logger.LogInfo($"Removing trash: {trashToRemove.trashID} -- Trash count: {trashes.Count}");
        trashes.Remove(trashToRemove);
        trashToRemove.CmdClearTrash();
    }
}