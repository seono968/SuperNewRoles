using HarmonyLib;

namespace SuperNewRoles.Patch.Harmony.Meeting
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    class MeetingHud_Awake
    {
        static void Postfix(MeetingHud __instance)
        {
            if (BotManager.AllBots != null) new LateTask(()=> {BotManager.BotVote(__instance);}, 0.5f);
        }
    }
}