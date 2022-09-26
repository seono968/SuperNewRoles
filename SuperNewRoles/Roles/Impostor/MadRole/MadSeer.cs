using SuperNewRoles.Patch;
using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor
{
    class MadSeer
    {
        private const int OptionId = 323;// 設定のId
        /*============CustomOptionDateSTART============*/
        public static CustomRoleOption MadSeerOption;
        public static CustomOption MadSeerPlayerCount;
        public static CustomOption MadSeerMode;
        public static CustomOption MadSeerModeBoth;
        public static CustomOption MadSeerModeFlash;
        public static CustomOption MadSeerModeSouls;
        public static CustomOption MadSeerLimitSoulDuration;
        public static CustomOption MadSeerSoulDuration;
        public static CustomOption MadSeerIsCheckImpostor;
        public static CustomOption MadSeerCommonTask;
        public static CustomOption MadSeerShortTask;
        public static CustomOption MadSeerLongTask;
        public static CustomOption MadSeerCheckImpostorTask;
        public static CustomOption MadSeerIsUseVent;
        public static CustomOption MadSeerIsImpostorLight;

        public static void SetupCustomOptions()
        {
            MadSeerOption = new(OptionId, false, CustomOptionType.Crewmate, "MadSeerName", color, 1);
            MadSeerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadSeerOption);
            MadSeerMode = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, MadSeerOption);
            MadSeerLimitSoulDuration = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, MadSeerOption);
            MadSeerSoulDuration = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, MadSeerLimitSoulDuration, format: "unitCouples");
            MadSeerIsUseVent = CustomOption.Create(OptionId + 5, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadSeerOption);
            MadSeerIsImpostorLight = CustomOption.Create(OptionId + 6, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadSeerOption);
            MadSeerIsCheckImpostor = CustomOption.Create(OptionId + 7, false, CustomOptionType.Crewmate, "MadMateIsCheckImpostorSetting", false, MadSeerOption);
            var madseeroption = SelectTask.TaskSetting(OptionId + 8, OptionId + 9, OptionId + 10, MadSeerIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadSeerCommonTask = madseeroption.Item1;
            MadSeerShortTask = madseeroption.Item2;
            MadSeerLongTask = madseeroption.Item3;
            MadSeerCheckImpostorTask = CustomOption.Create(OptionId + 11, false, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, MadSeerIsCheckImpostor);
        }
        /*============CustomOptionDateEND==============*/

        /*============RoleClassSTART===================*/
        public static List<PlayerControl> MadSeerPlayer;
        public static Color color = ImpostorRed;
        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;

        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static void ClearAndReload()
        {
            MadSeerPlayer = new();
            deadBodyPositions = new();
            soulDuration = MadSeerSoulDuration.GetFloat();

            IsImpostorCheck = MadSeerIsCheckImpostor.GetBool();
            IsUseVent = MadSeerIsUseVent.GetBool();
            IsImpostorLight = MadSeerIsImpostorLight.GetBool();
            int Common = MadSeerCommonTask.GetInt();
            int Long = MadSeerLongTask.GetInt();
            int Short = MadSeerShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = PlayerControl.GameOptions.NumCommonTasks;
                Long = PlayerControl.GameOptions.NumLongTasks;
                Short = PlayerControl.GameOptions.NumShortTasks;
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(MadSeerCheckImpostorTask.GetString().Replace("%", "")) / 100f));
            MadSeer.CheckedImpostor = new();
        }
        /*============RoleClassEND=====================*/
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!IsImpostorCheck) return false;
            if (!p.IsRole(RoleId.MadSeer)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            SuperNewRolesPlugin.Logger.LogInfo("[MadSeer]Is Validity?:" + (ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[MadSeer]Returned valid.");
                return true;
            }
            return false;
        }
    }
}