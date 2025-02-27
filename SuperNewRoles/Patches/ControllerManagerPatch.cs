using Agartha;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using Agartha;
using AmongUs.GameOptions;
using System.Linq;
using UnityEngine;
using Hazel;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]
class ControllerManagerUpdatePatcha
{
    public static bool Prefix() => AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started;
}

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
class ControllerManagerUpdatePatch
{
    static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
    static int resolutionIndex = 0;
    public static void Postfix()
    {
        //解像度変更
        if (Input.GetKeyDown(KeyCode.F9))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }

        // その時点までのlogを切り出す
        if (ModHelpers.GetManyKeyDown(new[] { KeyCode.S, KeyCode.LeftShift, KeyCode.RightShift }))
        {
            string via = "KeyCommandVia";
            Logger.SaveLog(via, via);
        }


        //　ゲーム中
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost)
        {
            // 廃村
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift }))
            {
                RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                RPCProcedure.SetHaison();
                Logger.Info("===================== 廃村 ======================", "End Game");
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    EndGameCheck.CustomEndGame(ShipStatus.Instance, GameOverReason.ImpostorDisconnect, false);
                }
                else
                {
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    MapUtilities.CachedShipStatus.enabled = false;
                }
            }
            // 会議を強制終了
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift }) && RoleClass.IsMeeting)
            {
                if (MeetingHud.Instance != null)
                    MeetingHud.Instance.RpcClose();
            }
        }

        // デバッグモード　かつ　左コントロール
        if (ConfigRoles.DebugMode.Value && Input.GetKey(KeyCode.LeftControl))
        {
            // Spawn dummys
            if (Input.GetKeyDown(KeyCode.G))
            {
                PlayerControl bot = BotManager.Spawn(PlayerControl.LocalPlayer.NameText().text);

                bot.NetTransform.SnapTo(PlayerControl.LocalPlayer.transform.position);
                //new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                //new LateTask(() => { foreach (var pc in CachedPlayer.AllPlayers) pc.PlayerControl.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                //new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
            }

            //ここにデバッグ用のものを書いてね
            if (Input.GetKeyDown(KeyCode.I))
            {
                CustomObject.WaveCannonObject.Objects.All(x => {x.CreateRotationEffect(PlayerControl.LocalPlayer.GetTruePosition(), 1); return false; });
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                UnityEngine.Object.Instantiate(DestroyableSingleton<RoleManager>.Instance.protectAnim, PlayerControl.LocalPlayer.gameObject.transform).Play(PlayerControl.LocalPlayer, null, PlayerControl.LocalPlayer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.ImpostorGhost);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                PVCreator.Start();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                PVCreator.End();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                PVCreator.Start2();
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                ModHelpers.PlayerById(1).RpcMurderPlayer(PlayerControl.LocalPlayer);//ModHelpers.PlayerById(2));
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                BotManager.Spawn($"bot{(byte)GameData.Instance.GetAvailableId()}");
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                BotManager.AllBotDespawn();
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SuperNewRolesPlugin.Logger.LogInfo("new Vector2(" + (PlayerControl.LocalPlayer.transform.position.x - 12.63f) + "f, " + (PlayerControl.LocalPlayer.transform.position.y + 3.46f) + "f), ");
            }
        }
        // 以下フリープレイのみ
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
        // エアーシップのトイレのドアを開ける
        if (Input.GetKeyDown(KeyCode.T))
        {
            RPCHelper.RpcOpenToilet();
        }
    }
}