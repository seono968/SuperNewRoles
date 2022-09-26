using SuperNewRoles.Patch;
using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor
{
    public static class EvilSeer
    {
        private const int OptionId = 334;// 設定のId
        /*============CustomOptionDateSTART============*/
        public static CustomRoleOption EvilSeerOption;
        public static CustomOption EvilSeerPlayerCount;
        public static CustomOption EvilSeerMode;
        public static CustomOption EvilSeerModeBoth;
        public static CustomOption EvilSeerModeFlash;
        public static CustomOption EvilSeerModeSouls;
        public static CustomOption EvilSeerLimitSoulDuration;
        public static CustomOption EvilSeerSoulDuration;

        public static void SetupCustomOptions()
        {
            EvilSeerOption = new(OptionId, false, CustomOptionType.Impostor, "EvilSeerName", color, 1);
            EvilSeerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilSeerOption);
            EvilSeerMode = CustomOption.Create(OptionId + 2, false, CustomOptionType.Impostor, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, EvilSeerOption);
            EvilSeerLimitSoulDuration = CustomOption.Create(OptionId + 3, false, CustomOptionType.Impostor, "SeerLimitSoulDuration", false, EvilSeerOption);
            EvilSeerSoulDuration = CustomOption.Create(OptionId + 4, false, CustomOptionType.Impostor, "SeerSoulDuration", 15f, 0f, 120f, 5f, EvilSeerLimitSoulDuration, format: "unitCouples");
        }
        /*============CustomOptionDateEND==============*/

        /*============RoleClassSTART===================*/
        public static List<PlayerControl> EvilSeerPlayer;
        public static Color32 color = ImpostorRed;
        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;
        public static void ClearAndReload()
        {
            EvilSeerPlayer = new();
            deadBodyPositions = new();
            soulDuration = EvilSeerSoulDuration.GetFloat();
        }
        /*============RoleClassEND=====================*/
    }
}