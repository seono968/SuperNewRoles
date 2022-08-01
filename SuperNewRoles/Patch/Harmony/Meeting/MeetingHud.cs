using HarmonyLib;

namespace SuperNewRoles.Patch.Harmony.Meeting
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    class MeetingHud_Awake
    {
        static void Postfix(MeetingHud __instance)
        {
            if (BotManager.AllBots != null) BotManager.BotVote(__instance);
        }
    }
}