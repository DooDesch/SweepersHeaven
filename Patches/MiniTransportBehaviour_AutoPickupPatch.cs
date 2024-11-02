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
            if (!Plugin.Instance.MiniTransporterAutoPickup) return;

            if (__instance.velocity < 0.5f || !__instance.hasDriver || !__instance.hasAuthority) return;

            if (otherOBJ.CompareTag("Interactable") && otherOBJ.GetComponent<StolenProductSpawn>() != null)
            {
                __instance.StartCoroutine(PickupItemsCoroutine(__instance, otherOBJ));
            }
        }

        private static void PickupItem(MiniTransportBehaviour instance, GameObject item)
        {
            var itemComponent = item.GetComponent<StolenProductSpawn>();
            if (itemComponent != null)
            {
                itemComponent.CmdRecoverStolenProduct();
            }
        }

        private static IEnumerator PickupItemsCoroutine(MiniTransportBehaviour instance, GameObject item)
        {
            PlayerSyncCharacter_PickupPatch.ApplyForceToItem(item, instance.transform, instance.transform.forward);

            yield return new WaitForSeconds(1f);
            PickupItem(instance, item);
        }
    }
}
