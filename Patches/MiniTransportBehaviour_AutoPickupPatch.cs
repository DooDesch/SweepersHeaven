using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace SweepersHeaven.Patches
{
    [HarmonyPatch(typeof(MiniTransportBehaviour), "CheckCollision")]
    class MiniTransport_AutoPickupPatch
    {
        // Postfix patch to add item pickup functionality on collision
        static void Postfix(MiniTransportBehaviour __instance, GameObject otherOBJ)
        {
            // Check if the auto pickup feature is enabled
            if (!Plugin.Instance.MiniTransporterAutoPickup) return;

            // Check if the vehicle is moving fast enough and is being driven
            if (__instance.velocity < 1f || !__instance.hasDriver || !__instance.hasAuthority) return;

            // Check if the collided object is a collectible item with the necessary tag and component
            if (otherOBJ.CompareTag("Interactable") && otherOBJ.GetComponent<StolenProductSpawn>() != null)
            {
                // Start the coroutine to pickup the item
                __instance.StartCoroutine(PickupItemsCoroutine(__instance, otherOBJ));
            }
        }

        private static void PickupItem(MiniTransportBehaviour instance, GameObject item)
        {
            // Access the StolenProductSpawn component and trigger the pickup command
            var itemComponent = item.GetComponent<StolenProductSpawn>();
            if (itemComponent != null)
            {
                itemComponent.CmdRecoverStolenProduct();
            }
        }

        private static IEnumerator PickupItemsCoroutine(MiniTransportBehaviour instance, GameObject item)
        {
            // Use ApplyForceToItem method from PlayerSyncCharacter_PickupPatch to apply force to the item. The direction should be forward based on the vehicle
            PlayerSyncCharacter_PickupPatch.ApplyForceToItem(item, instance.transform, instance.transform.forward);

            // Use the same coroutine method from PlayerSyncCharacter_PickupPatch to handle the pickup process
            yield return new WaitForSeconds(1f);
            PickupItem(instance, item);
        }
    }
}
