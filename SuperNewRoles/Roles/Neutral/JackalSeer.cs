using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patch;
using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;
namespace SuperNewRoles.Roles.Neutral
{
    class JackalSeer
    {
        private const int OptionId = 375;// 設定のId
        /*============CustomOptionDateSTART============*/
        public static CustomRoleOption JackalSeerOption;
        public static CustomOption JackalSeerPlayerCount;
        public static CustomOption JackalSeerMode;
        public static CustomOption JackalSeerModeBoth;
        public static CustomOption JackalSeerModeFlash;
        public static CustomOption JackalSeerModeSouls;
        public static CustomOption JackalSeerLimitSoulDuration;
        public static CustomOption JackalSeerSoulDuration;
        public static CustomOption JackalSeerKillCoolDown;
        public static CustomOption JackalSeerUseVent;
        public static CustomOption JackalSeerUseSabo;
        public static CustomOption JackalSeerIsImpostorLight;
        public static CustomOption JackalSeerCreateSidekick;
        public static CustomOption JackalSeerNewJackalCreateSidekick;

        public static void SetupCustomOptions()
        {
            JackalSeerOption = new(OptionId, false, CustomOptionType.Neutral, "JackalSeerName", color, 1);
            JackalSeerPlayerCount = CustomOption.Create(OptionId+1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalSeerOption);
            JackalSeerMode = CustomOption.Create(OptionId+2, false, CustomOptionType.Neutral, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, JackalSeerOption);
            JackalSeerLimitSoulDuration = CustomOption.Create(OptionId+3, false, CustomOptionType.Neutral, "SeerLimitSoulDuration", false, JackalSeerOption);
            JackalSeerSoulDuration = CustomOption.Create(OptionId+4, false, CustomOptionType.Neutral, "SeerSoulDuration", 15f, 0f, 120f, 5f, JackalSeerLimitSoulDuration, format: "unitCouples");
            JackalSeerKillCoolDown = CustomOption.Create(OptionId+5, false, CustomOptionType.Neutral, "JackalCoolDownSetting", 30f, 2.5f, 60f, 2.5f, JackalSeerOption, format: "unitSeconds");
            JackalSeerUseVent = CustomOption.Create(OptionId+6, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalSeerOption);
            JackalSeerUseSabo = CustomOption.Create(OptionId+7, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalSeerOption);
            JackalSeerIsImpostorLight = CustomOption.Create(OptionId+8, false, CustomOptionType.Neutral, "MadMateImpostorLightSetting", false, JackalSeerOption);
            JackalSeerCreateSidekick = CustomOption.Create(OptionId+9, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalSeerOption);
            JackalSeerNewJackalCreateSidekick = CustomOption.Create(OptionId+10, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalSeerCreateSidekick);
        }
        /*============CustomOptionDateEND==============*/

        /*============RoleClassSTART===================*/
            public static List<PlayerControl> JackalSeerPlayer;
            public static List<PlayerControl> SidekickSeerPlayer;
            public static List<PlayerControl> FakeSidekickSeerPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);

            public static List<Vector3> deadBodyPositions;
            public static float soulDuration;

            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsImpostorLight;
            public static bool CreateSidekick;
            public static bool NewJackalCreateSidekick;
            public static bool CanCreateSidekick;
            public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSeerSidekickButton.png", 115f);

            public static void ClearAndReload()
            {
                JackalSeerPlayer = new();
                SidekickSeerPlayer = new();
                FakeSidekickSeerPlayer = new();

                deadBodyPositions = new();
                soulDuration = JackalSeerSoulDuration.GetFloat();

                KillCoolDown = JackalSeerKillCoolDown.GetFloat();
                IsUseVent = JackalSeerUseVent.GetBool();
                IsUseSabo = JackalSeerUseSabo.GetBool();
                IsImpostorLight = JackalSeerIsImpostorLight.GetBool();
                CreateSidekick = JackalSeerCreateSidekick.GetBool();
                CanCreateSidekick = JackalSeerCreateSidekick.GetBool();
                NewJackalCreateSidekick = JackalSeerNewJackalCreateSidekick.GetBool();
            }
        /*============RoleClassEND=====================*/

        public static void ResetCoolDown()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.MaxTimer = KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.Timer = KillCoolDown;
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        public class JackalSeerFixedPatch
        {
            static void JackalSeerPlayerOutLineTarget()
            {
                SetPlayerOutline(JackalSetTarget(), color);
            }
            public static void Postfix(PlayerControl __instance, RoleId role)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (SidekickSeerPlayer.Count > 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in JackalSeerPlayer)
                        {
                            if (p.IsAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickSeerPromotes, SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.SidekickSeerPromotes();
                        }
                    }
                }
                if (role == RoleId.JackalSeer)
                {
                    JackalSeerPlayerOutLineTarget();
                }
            }
        }
    }
}