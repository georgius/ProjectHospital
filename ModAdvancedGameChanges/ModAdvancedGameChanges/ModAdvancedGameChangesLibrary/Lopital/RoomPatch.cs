using GLib;
using HarmonyLib;
using Lopital;
using System.Collections.Generic;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Room))]
    public static class RoomPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Room), nameof(Room.IsCharactersTurn))]
        public static bool IsCharactersTurn(Entity character, Room __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if ((__instance.m_roomPersistentData.m_characterQueue.Count == 0) 
                && (__instance.m_roomPersistentData.m_characterQueueLab.Count == 0) 
                && (__instance.m_roomPersistentData.m_characterQueueUnits.Count == 0))
            {
                // all queues are empty
                // character can continue

                __result = true;
                return false;
            }

            // check if character is on top of queue
            if ((__instance.m_roomPersistentData.m_characterQueue.Count > 0) 
                && (__instance.m_roomPersistentData.m_characterQueue[0] == character))
            {
                __result = true;
                return false;
            }

            if ((__instance.m_roomPersistentData.m_characterQueueLab.Count > 0) 
                && (__instance.m_roomPersistentData.m_characterQueueLab[0] == character))
            {
                __result = true;
                return false;
            }

            if ((__instance.m_roomPersistentData.m_characterQueueUnits.Count > 0) 
                && (__instance.m_roomPersistentData.m_characterQueueUnits[0] == character))
            {
                __result = true;
                return false;
            }

            // default case
            // character cannot continue

            __result = false;
            return false;
        }
    }
}
