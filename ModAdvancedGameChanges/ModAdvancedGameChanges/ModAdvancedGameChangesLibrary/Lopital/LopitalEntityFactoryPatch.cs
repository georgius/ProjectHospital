using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Helpers;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(LopitalEntityFactory))]
    public static class LopitalEntityFactoryPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LopitalEntityFactory), nameof(LopitalEntityFactory.CreateCharacterPedestrian))]
        public static bool CreateCharacterPedestrianPrefix(Floor floor, Vector2i position, ref Entity __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Entity entity = new Entity("Pedestrian ", 14U);
            entity.AddComponent(new CharacterPersonalInfoComponent(entity));
            LopitalEntityFactoryExtensions.AddDefaultCharacterComponents(entity, floor, position);
            entity.AddComponent(new BehaviorPedestrian(entity));
            entity.AddComponent(new PerkComponent(entity));

            entity.GetComponent<CharacterPersonalInfoComponent>().ChooseVoice(true);
            entity.Name = "Pedestrian " + entity.GetComponent<CharacterPersonalInfoComponent>().m_personalInfo.GetFullName();

            __result = entity;

            return false;
        }
    }

    public static class LopitalEntityFactoryExtensions
    {
        public static void AddDefaultCharacterComponents(Entity entity, Floor floor, Vector2i position)
        {
            MethodAccessHelper.CallStaticMethod(typeof(LopitalEntityFactory), "AddDefaultCharacterComponents", entity, floor, position);
        }
    }
}
