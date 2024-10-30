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

        if (Input.GetKeyDown(Plugin.Instance.PickupKey))
        {
            int equippedItem = __instance.pNetwork.equippedItem;
            if (equippedItem != broomItemIndex) return;

            __instance.pNetwork.CmdPlayAnimation(broomHitAnimationIndex);

            List<GameObject> itemsInRange = FindItemsNearby(__instance.transform, Plugin.Instance.PickupRadius);
            __instance.StartCoroutine(PickupItemsCoroutine(__instance, itemsInRange));
        }
    }

    private static IEnumerator PickupItemsCoroutine(PlayerSyncCharacter instance, List<GameObject> itemsInRange)
    {
        int itemsPicked = 0;
        instance.StartCoroutine(PickupDelayCoroutine(instance));

        foreach (var item in itemsInRange)
        {
            if (!Plugin.Instance.ThrowItemsOnPickup) break;
            ApplyForceToItem(item, instance.transform);
        }

        foreach (var item in itemsInRange)
        {
            if (itemsPicked >= Plugin.Instance.MaxItemsToPick) break;
            ApplyForceToItem(item, instance.transform);

            yield return new WaitForSeconds(1f / Mathf.Min(Plugin.Instance.MaxItemsToPick, itemsInRange.Count));
            item.GetComponent<StolenProductSpawn>().CmdRecoverStolenProduct();
            itemsPicked++;
        }

        Debug.Log($"Picked up {itemsPicked} items with the broom!");
    }

    private static void ApplyForceToItem(GameObject item, Transform playerTransform)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        float randomForce = Random.Range(2f, 10f);

        Vector3 leftForce = (Vector3.up + -playerTransform.right).normalized * randomForce;
        rb.AddForce(leftForce, ForceMode.Impulse);
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
