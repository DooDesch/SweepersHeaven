using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SweepersHeaven.Patches;

[HarmonyPatch(typeof(PlayerSyncCharacter), "LateUpdate")]
class PlayerSyncCharacter_PickupPatch
{
    private static bool canPickup = true;
    private static int broomItemIndex = 3;
    private static int broomHitAnimationIndex = 0;

    static void Postfix(PlayerSyncCharacter __instance)
    {
        if (!__instance.isLocalPlayer || !canPickup) return;

        int equippedItem = __instance.pNetwork.equippedItem;
        if (equippedItem != broomItemIndex) return;

        if (Input.GetKeyDown(Plugin.Instance.PickupKey))
        {
            List<GameObject> itemsInRange = ItemsInRange();
            if (itemsInRange.Count == 0) return;

            __instance.pNetwork.CmdPlayAnimation(broomHitAnimationIndex);
            __instance.StartCoroutine(PickupItemsCoroutine(__instance, itemsInRange));
        }

        if (Input.GetMouseButtonDown(1))
        {
            List<GameObject> itemsInRange = ItemsInRange();
            if (itemsInRange.Count == 0) return;

            __instance.pNetwork.CmdPlayAnimation(broomHitAnimationIndex);
            ThrowItems(itemsInRange, __instance);
        }
    }

    private static List<GameObject> ItemsInRange()
    {
        List<GameObject> itemsInRange = new List<GameObject>();

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            GameObject lookedAt = hit.collider.gameObject;

            if (lookedAt.CompareTag("Interactable") && lookedAt.GetComponent<StolenProductSpawn>() != null)
            {
                itemsInRange = FindItemsNearby(lookedAt.transform, Plugin.Instance.PickupRadius);
            }
        }

        return itemsInRange;
    }

    private static IEnumerator PickupItemsCoroutine(PlayerSyncCharacter instance, List<GameObject> itemsInRange)
    {
        int itemsPicked = 0;

        instance.StartCoroutine(PickupDelayCoroutine(instance));
        ThrowItems(itemsInRange, instance);

        foreach (var item in itemsInRange)
        {
            if (itemsPicked >= Plugin.Instance.MaxItemsToPick) break;

            if (item == null) continue;

            var itemComponent = item.GetComponent<StolenProductSpawn>();
            if (itemComponent == null) continue;

            yield return new WaitForSeconds(1f / Mathf.Min(Plugin.Instance.MaxItemsToPick, itemsInRange.Count));
            itemComponent.CmdRecoverStolenProduct();
            itemsPicked++;
        }

        Debug.Log($"Picked up {itemsPicked} items with the broom!");
    }

    private static void ThrowItems(List<GameObject> items, PlayerSyncCharacter instance)
    {
        foreach (var item in items)
        {
            if (!Plugin.Instance.ThrowItemsOnPickup) break;
            ApplyForceToItem(item, instance.transform);
        }
    }

    public static void ApplyForceToItem(GameObject item, Transform playerTransform, Vector3 direction = default)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        float randomForce = Random.Range(2f, 10f);

        Vector3 force = (Vector3.up + direction).normalized * randomForce;
        if (direction == default)
        {
            force = (Vector3.up + -playerTransform.right).normalized * randomForce;
        }

        rb.AddForce(force, ForceMode.Impulse);
    }

    private static IEnumerator PickupDelayCoroutine(PlayerSyncCharacter instance)
    {
        canPickup = false;
        yield return new WaitForSeconds(1f);
        canPickup = true;
    }

    private static List<GameObject> FindItemsNearby(Transform playerTransform, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, radius);
        List<GameObject> itemsInRange = new List<GameObject>();

        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Interactable") && collider.gameObject.GetComponent<StolenProductSpawn>() != null)
            {
                itemsInRange.Add(collider.gameObject);
            }
        }

        itemsInRange.Sort((a, b) =>
        {
            Vector3 toA = a.transform.position - playerTransform.position;
            Vector3 toB = b.transform.position - playerTransform.position;
            float angleA = Vector3.Angle(playerTransform.forward, toA);
            float angleB = Vector3.Angle(playerTransform.forward, toB);
            return angleA.CompareTo(angleB);
        });

        return itemsInRange;
    }
}
