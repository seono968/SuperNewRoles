using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    public class StartGame
    {
        public static void Postfix()
        {
            MapOptions.RandomMap.Prefix();
            FixedUpdate.IsProDown = ConfigRoles.CustomProcessDown.Value;
        }
    }
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    public class AbilityUpdate
    {
        public static void Postfix(AbilityButton __instance)
        {
            if (CachedPlayer.LocalPlayer.Data.Role.IsSimpleRole && __instance.commsDown.active)
            {
                __instance.commsDown.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class ControllerManagerUpdate
    {
        static void Postfix()
        {
            // 以下ホストのみ
            if (!AmongUsClient.Instance.AmHost) return;

            //　ゲーム中
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                // 廃村
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift }))
                {
                    RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                    RPCProcedure.SetHaison();
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        EndGameCheck.CustomEndGame(ShipStatus.Instance, GameOverReason.HumansDisconnect, false);
                    }
                    else
                    {
                        ShipStatus.RpcEndGame(GameOverReason.HumansDisconnect, false);
                        MapUtilities.CachedShipStatus.enabled = false;
                    }
                }
            }

            // 会議を強制終了
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift }) && RoleClass.IsMeeting)
            {
                if (MeetingHud.Instance != null)
                    MeetingHud.Instance.RpcClose();
            }

            // 以下フリープレイのみ
            if (AmongUsClient.Instance.GameMode != GameModes.FreePlay) return;
            // エアーシップのトイレのドアを開ける
            if (Input.GetKeyDown(KeyCode.T))
            {
                RPCHelper.RpcOpenToilet();
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class FixedUpdate
    {
        static void SetBasePlayerOutlines()
        {
            foreach (PlayerControl target in CachedPlayer.AllPlayers)
            {
                var rend = target.MyRend();
                if (target == null || rend == null) continue;
                if (rend.material.GetFloat("_Outline") == 0f) continue;
                rend.material.SetFloat("_Outline", 0f);
            }
        }

        static void ReduceKillCooldown(PlayerControl __instance)
        {
            if (CustomOptionHolder.IsAlwaysReduceCooldown.GetBool())
            {
                // オプションがOFFの場合はベント内はクールダウン減少を止める
                bool exceptInVent = !CustomOptionHolder.IsAlwaysReduceCooldownExceptInVent.GetBool() && PlayerControl.LocalPlayer.inVent;
                // 配電盤タスク中はクールダウン減少を止める
                bool exceptOnTask = !CustomOptionHolder.IsAlwaysReduceCooldownExceptOnTask.GetBool() && ElectricPatch.onTask;

                if (!__instance.Data.IsDead && !__instance.CanMove && !exceptInVent && !exceptOnTask)
                {
                    __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
                    return;
                }
            }
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Tasker) && CustomOptionHolder.TaskerIsKillCoolTaskNow.GetBool())
            {
                if (!__instance.Data.IsDead && !__instance.CanMove && Minigame.Instance != null && Minigame.Instance.MyNormTask != null && Minigame.Instance.MyNormTask.Owner.AmOwner)
                    __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
            }
        }
        public static bool IsProDown;

        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerAnimation.GetPlayerAnimation(__instance.PlayerId) == null) new PlayerAnimation(__instance);
            if (__instance != PlayerControl.LocalPlayer) return;
            SluggerDeadbody.AllFixedUpdate();
            PlayerAnimation.FixedAllUpdate();
            PVCreator.FixedUpdate();

            VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
            OldModeButtons.OldModeUpdate();

            // -- 以下ゲーム中のみ --
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined)
                {
                    SNROnlySearch.FixedUpdate();
                }
                return;
            }

            SetBasePlayerOutlines();
            LadderDead.FixedUpdate();
            switch (ModeHandler.GetMode())
            {
                case ModeId.Default:
                    SabotageManager.Update();
                    SetNameUpdate.Postfix(__instance);
                    Jackal.JackalFixedPatch.Postfix(__instance, PlayerControl.LocalPlayer.GetRole());
                    JackalSeer.JackalSeerFixedPatch.Postfix(__instance, PlayerControl.LocalPlayer.GetRole());
                    Roles.Crewmate.Psychometrist.FixedUpdate();
                    Roles.Impostor.Matryoshka.FixedUpdate();
                    Roles.Neutral.PartTimer.FixedUpdate();
                    ReduceKillCooldown(__instance);
                    if (PlayerControl.LocalPlayer.IsAlive())
                    {
                        if (PlayerControl.LocalPlayer.IsImpostor()) { SetTarget.ImpostorSetTarget(); }
                        if (PlayerControl.LocalPlayer.IsMadRoles()) { VentDataModules.MadmateVent(); }
                        NormalButtonDestroy.Postfix();
                        switch (PlayerControl.LocalPlayer.GetRole())
                        {
                            case RoleId.Pursuer:
                                Pursuer.PursureUpdate.Postfix();
                                break;
                            case RoleId.Levelinger:
                                if (RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Pursuer))
                                {
                                    if (!RoleClass.Pursuer.arrow.arrow.active)
                                    {
                                        RoleClass.Pursuer.arrow.arrow.SetActive(true);
                                    }
                                    Pursuer.PursureUpdate.Postfix();
                                }
                                else
                                {
                                    if (RoleClass.Pursuer.arrow.arrow.active)
                                    {
                                        RoleClass.Pursuer.arrow.arrow.SetActive(false);
                                    }
                                }
                                break;
                            case RoleId.Hawk:
                                Hawk.FixedUpdate.Postfix();
                                break;
                            case RoleId.NiceHawk:
                                NiceHawk.FixedUpdate.Postfix();
                                break;
                            case RoleId.MadHawk:
                                MadHawk.FixedUpdate.Postfix();
                                break;
                            case RoleId.Vampire:
                                Vampire.FixedUpdate.Postfix();
                                break;
                            case RoleId.Vulture:
                                Vulture.FixedUpdate.Postfix();
                                break;
                            case RoleId.Mafia:
                                Mafia.FixedUpdate();
                                break;
                            case RoleId.SerialKiller:
                                SerialKiller.FixedUpdate();
                                break;
                            case RoleId.Kunoichi:
                                Kunoichi.Update();
                                break;
                            case RoleId.Revolutionist:
                                Roles.Neutral.Revolutionist.FixedUpdate();
                                break;
                            case RoleId.Spelunker:
                                Roles.Neutral.Spelunker.FixedUpdate();
                                break;
                            case RoleId.SuicidalIdeation:
                                SuicidalIdeation.Postfix();
                                break;
                            case RoleId.Doctor:
                                Doctor.FixedUpdate();
                                break;
                            case RoleId.Psychometrist:
                                Roles.Crewmate.Psychometrist.PsychometristFixedUpdate();
                                break;
                            case RoleId.SeeThroughPerson:
                                Roles.Crewmate.SeeThroughPerson.FixedUpdate();
                                break;
                            case RoleId.Hitman:
                                Roles.Neutral.Hitman.FixedUpdate();
                                break;
                            case RoleId.Photographer:
                                Roles.Neutral.Photographer.FixedUpdate();
                                break;
                            case RoleId.Doppelganger:
                                Roles.Impostor.Doppelganger.FixedUpdate();
                                break;
                            case RoleId.Pavlovsowner:
                                Roles.Neutral.Pavlovsdogs.OwnerFixedUpdate();
                                break;
                            case RoleId.WaveCannonJackal:
                                JackalSeer.JackalSeerFixedPatch.JackalSeerPlayerOutLineTarget();
                                break;
                            case RoleId.ConnectKiller:
                                Roles.Impostor.ConnectKiller.Update();
                                break;
                            case RoleId.ShiftActor:
                                Roles.Impostor.ShiftActor.FixedUpdate();
                                break;
                        }
                    }
                    else // -- 死亡時 --
                    {
                        if (MapOptions.MapOption.ClairvoyantZoom)
                        {
                            Clairvoyant.FixedUpdate.Postfix();
                        }
                        switch (PlayerControl.LocalPlayer.GetRole())
                        {
                            case RoleId.Bait:
                                if (!RoleClass.Bait.Reported)
                                {
                                    Bait.BaitUpdate.Postfix();
                                }
                                break;
                            case RoleId.SideKiller:
                                if (!RoleClass.SideKiller.IsUpMadKiller)
                                {
                                    var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                                    if (sideplayer != null)
                                    {
                                        sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                        RoleClass.SideKiller.IsUpMadKiller = true;
                                    }
                                }
                                break;
                            case RoleId.Vulture:
                                if (RoleClass.Vulture.Arrow?.arrow != null)
                                {
                                    Object.Destroy(RoleClass.Vulture.Arrow.arrow);
                                    return;
                                }
                                break;
                        }
                    }
                    break;
                case ModeId.SuperHostRoles:
                    Mode.SuperHostRoles.FixedUpdate.Update();
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.Mafia))
                    {
                        Mafia.FixedUpdate();
                    }
                    SerialKiller.SHRFixedUpdate(PlayerControl.LocalPlayer.GetRole());
                    Roles.Impostor.Camouflager.SHRFixedUpdate();
                    break;
                case ModeId.NotImpostorCheck:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BlockTool.FixedUpdate();
                    }
                    Mode.NotImpostorCheck.NameSet.Postfix();
                    break;
                default:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BlockTool.FixedUpdate();
                    }
                    ModeHandler.FixedUpdate(__instance);
                    break;
            }
        }
    }
}