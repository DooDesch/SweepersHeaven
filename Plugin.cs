using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BepInEx.Configuration;

namespace SweepersHeaven;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance; // static instance
    internal static new ManualLogSource Logger;
    private Harmony harmony;

    private ConfigEntry<float> pickupRadius;
    private ConfigEntry<int> maxItemsToPick;
    private ConfigEntry<bool> throwItemsOnPickup;
    private ConfigEntry<KeyCode> pickupKey;
    private ConfigEntry<KeyCode> spawnKey;
    private ConfigEntry<bool> isDebugMode;
    private ConfigEntry<bool> miniTransporterAutoPickup;
    private ConfigEntry<bool> pickUpAllTrashAtOnce;

    internal static GameObject StolenProductPrefab;

    private void Awake()
    {
        Instance = this;

        // Initialize Config Entries
        pickupRadius = Config.Bind("Pickup Settings", "Pickup Radius", 2.5f, "Radius within which items are collected.");
        maxItemsToPick = Config.Bind("Pickup Settings", "Max Items to Pick", 5, "Maximum number of items picked at once.");
        throwItemsOnPickup = Config.Bind("Pickup Settings", "Throw Items on Pickup", true, "If true, items are thrown when picked up with the broom.");
        pickupKey = Config.Bind("Controls", "Pickup Key", KeyCode.Mouse0, "Key to activate broom sweeping. Default is Left Mouse Button.");
        spawnKey = Config.Bind("Debug", "Spawn Key", KeyCode.G, "Key to spawn items around player. Only works in Debug Mode.");
        isDebugMode = Config.Bind("Debug", "Enable Debug Mode", false, "If true, enables debug actions. Use Spawn Key to spawn items.");
        miniTransporterAutoPickup = Config.Bind("Mini Transporter", "Auto Pickup", true, "If true, enables auto pickup for Mini Transporter.");
        pickUpAllTrashAtOnce = Config.Bind("Trash", "Pick Up All Trash At Once", false, "If true, enables picking up all trash in the store at once.");

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    // Public properties to access config values
    public float PickupRadius => pickupRadius.Value;
    public int MaxItemsToPick => maxItemsToPick.Value;
    public bool ThrowItemsOnPickup => throwItemsOnPickup.Value;
    public KeyCode PickupKey => pickupKey.Value;
    public KeyCode SpawnKey => spawnKey.Value;
    public bool IsDebugMode => isDebugMode.Value;
    public bool MiniTransporterAutoPickup => miniTransporterAutoPickup.Value;
    public bool PickUpAllTrashAtOnce => pickUpAllTrashAtOnce.Value;

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(NPC_Info), nameof(NPC_Info.OnStartClient))]
    public static class NPCInfo_OnStartClient_Patch
    {
        public static void Postfix(NPC_Info __instance)
        {
            if (StolenProductPrefab == null && __instance.stolenProductPrefab != null)
            {
                StolenProductPrefab = __instance.stolenProductPrefab;
                Logger.LogInfo("Stolen product prefab set.");
            }
        }
    }

    private void DropRandomProductAtPlayer()
    {
        List<Transform> shelves = GetRandomShelfWithProducts();
        if (shelves.Count == 0) return;

        Transform shelf = shelves[Random.Range(0, shelves.Count)];
        var dataContainer = shelf.GetComponent<Data_Container>();
        int randomProductIndex = GetRandomProductFromShelf(dataContainer.productInfoArray);

        if (randomProductIndex != -1)
        {
            dataContainer.NPCGetsItemFromRow(randomProductIndex);
            SpawnProductNearPlayer(randomProductIndex);
        }
    }

    private List<Transform> GetRandomShelfWithProducts()
    {
        List<Transform> shelvesWithProduct = new List<Transform>();
        GameObject shelves = GameObject.Find("Level_SupermarketProps/Shelves");

        if (shelves != null)
        {
            for (int i = 0; i < shelves.transform.childCount; i++)
            {
                Transform child = shelves.transform.GetChild(i);
                var dataContainer = child.gameObject.GetComponent<Data_Container>();
                if (dataContainer != null && ProductExistsInShelf(dataContainer.productInfoArray))
                {
                    shelvesWithProduct.Add(child);
                }
            }
        }
        return shelvesWithProduct;
    }

    private int GetRandomProductFromShelf(int[] productInfoArray)
    {
        List<int> availableProducts = new List<int>();

        for (int i = 0; i < productInfoArray.Length / 2; i++)
        {
            int productID = productInfoArray[i * 2];
            int productCount = productInfoArray[i * 2 + 1];
            if (productID >= 0 && productCount > 0)
            {
                availableProducts.Add(productID);
            }
        }
        return availableProducts.Count > 0 ? availableProducts[Random.Range(0, availableProducts.Count)] : -1;
    }

    private bool ProductExistsInShelf(int[] productInfoArray)
    {
        for (int i = 0; i < productInfoArray.Length / 2; i++)
        {
            if (productInfoArray[i * 2 + 1] > 0) return true;
        }
        return false;
    }

    private void SpawnProductNearPlayer(int productID)
    {
        GameObject productPrefab = StolenProductPrefab;
        if (productPrefab == null) return;

        Vector3 spawnPosition = PlayerPosition() + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(-1.5f, 1.5f));
        GameObject spawnedProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
        spawnedProduct.AddComponent<StolenProductSpawn>();
        spawnedProduct.GetComponent<StolenProductSpawn>().NetworkproductID = productID;
        spawnedProduct.GetComponent<StolenProductSpawn>().NetworkproductCarryingPrice = 10f;
        NetworkServer.Spawn(spawnedProduct);
        Logger.LogInfo($"Spawned product ID {productID} at {spawnPosition}");
    }

    private static Vector3 PlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player") ?? GameObject.Find("LocalGamePlayer");
        return player?.transform.position ?? Vector3.zero;
    }

    private void Update()
    {
        if (isDebugMode.Value && Input.GetKeyDown(spawnKey.Value))
        {
            for (int i = 0; i < 100; i++) DropRandomProductAtPlayer();
        }
    }

}
