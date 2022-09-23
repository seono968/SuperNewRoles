using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Patch;

using UnityEngine;

using HarmonyLib;
using SuperNewRoles.Mode;

using COT = SuperNewRoles.Patch.CustomOptionType;
using CO = SuperNewRoles.Patch.CustomOption;

namespace SuperNewRoles.Roles.Impostor
{
    /*
    TODO:
    ・プレイヤーをキルした時代わりに爆弾が取り付けられ、一定時間たった後起動し時間経過で爆発する
    ・爆発時は自爆魔のようなキルをしその爆発には味方のインポスターや自身も巻き込まれる
    ・起動した爆弾がついたプレイヤーの名前には爆弾マークがつき他のインポスターからだけつけられていることが分かる
    ・会議中は全員爆弾マークを見ることが出来る
    !・会議が終わると爆弾は解除される
    ・爆弾が爆発するか解除されるまで次の爆弾は付けられない

    ・「起爆」ボタンを持ち、起動した爆弾がある時にのみ押すことが出来る
    ・押すことで自分がつけた爆弾を即起爆できるが、
    自身は一定時間移動できなくなりキルクールが伸びる

    ・また、爆破地点から一定範囲内のプレイヤーに爆破地点への矢印が
    !一定時間表示される
    */
    public class TimeBomber
    {
        public static CustomRoleOption Option;
        public static CO PlayerCount;
        public static CO KillCool;          // キルクール
        public static CO StartTime;         // 起動するまでの時間
        public static CO BombTime;          // 爆発するまでの時間
        public static CO BombScope;         // 爆発のキル半径
        public static CO CanNotMoveTime;    // 起爆時動けない時間
        public static CO ExtensionKillCool; // 起爆時に伸びるキルクールの量
        public static CO IsArrow;           // 起爆時に矢印を表示するか
        public static CO ArrowDuration;     // 矢印の表示されている時間
        public static CO ArrowScope;        // 起爆時に矢印を表示する半径

        public static void SetupCustomOptions()
        {
            var id = 992;
            Option = new(id, false, COT.Impostor, "TimeBomberName", color, 1);
            PlayerCount = CO.Create(id + 1, false, COT.Impostor, "SettingPlayerCountName", 1f, 1f, 5f, 1f, Option);
            KillCool = CO.Create(id + 2, false, COT.Impostor, "KillCoolDown", 20, 0, 60, 2.5f, Option);
            StartTime = CO.Create(id + 3, false, COT.Impostor, "TimeToStart", 5, 0, 15, 0.5f, Option);
            BombTime = CO.Create(id + 4, false, COT.Impostor, "TimeToBomb", 15, 0, 45, 0.5f, Option);
            BombScope = CO.Create(id + 5, false, COT.Impostor, "BombScope", 1, 0.5f, 3, 0.5f, Option);
            CanNotMoveTime = CO.Create(id + 6, false, COT.Impostor, "CanNotMoveTime", 5, 0, 10, 0.5f, Option);
            ExtensionKillCool = CO.Create(id + 7, false, COT.Impostor, "ExtensionKillCool", 10, 0, 30, 2.5f, Option);
            IsArrow = CO.Create(id + 8, false, COT.Impostor, "BombArrow", true, Option);
            ArrowDuration = CO.Create(id + 9, false, COT.Impostor, "ArrowDuration", 20, 0, 60, 2.5f, IsArrow);
            ArrowScope = CO.Create(id + 10, false, COT.Impostor, "ArrowScope", 4, 0.5f, 10, 0.5f, IsArrow);
        }

        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static List<PlayerControl> AllTarget; // 爆破した全てのターゲット
        public static List<PlayerControl> NowTarget; // 今のターゲット
        public static float NowBombTime;// 今のキルク
        public static Arrow ARROW = null;
        public static float ArrowTime;
        public static bool IsSpeedDown;
        public const string NameBombMark = " ★";

        public static void ClearAndReload()
        {
            Player = new();
            AllTarget = new();
            NowTarget = new();
            NowBombTime = BombTime.GetFloat();
            if (ARROW != null)
            {
                GameObject.Destroy(ARROW.arrow);
            }
            ARROW = new(color);
            ARROW.arrow.SetActive(false);

            ArrowTime = ArrowDuration.GetFloat();

            IsSpeedDown = false;
        }

        public static void AttachBomb(PlayerControl target)
        {
            if (AllTarget.Contains(target)) return; // targetがすでに爆破されているなら破棄
            if (RoleClass.IsMeeting)
            {
                AllTarget.Remove(target);
                NowTarget.Remove(target);
                return;
            }

            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.PlayerId != target.PlayerId)
                    if (SelfBomber.GetIsBomb(target, p, BombScope.GetFloat()))
                    {
                        target.RpcMurderPlayer(p);
                    }
            }
            target.RpcMurderPlayer(target);
            AllTarget.Add(target);
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                if (!ModeHandler.IsMode(ModeId.Default)) return;

                if (IsSpeedDown && PlayerControl.LocalPlayer.IsRole(RoleId.TimeBomber))
                {
                    Logger.Info("KIMASHITA");
                    __instance.body.velocity = new Vector2(0f, 0f);
                }
            }
        }
        private static CustomButton BombButton;  // 爆弾取り付けボタン
        private static CustomButton StartButton; // 起爆ボタン
        public static void SetupCustomButton(HudManager hm)
        {
            BombButton = new(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove && HudManagerStartPatch.SetTarget())
                    {
                        var target = HudManagerStartPatch.SetTarget();
                        NowTarget.Add(target);
                        new LateTask(() =>
                        {
                            AttachBomb(target);
                            NowTarget.Remove(target);
                        }, BombTime.GetFloat(), "Time bomber attach");
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && PlayerControl.LocalPlayer.IsRole(RoleId.TimeBomber); },
                () => { return PlayerControl.LocalPlayer.CanMove && HudManagerStartPatch.SetTarget(); },
                () => { ResetBombCoolDown(); },
                RoleClass.SelfBomber.GetButtonSprite(),
                new Vector3(0, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.Q,
                8,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("TimeBomberBombName"),
                showButtonText = true
            };

            StartButton = new(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove && NowTarget.Count != 0)
                    {
                        var target = HudManagerStartPatch.SetTarget();
                        ResetStartCoolDown();
                        AttachBomb(target);
                        NowBombTime += ExtensionKillCool.GetFloat();

                        IsSpeedDown = true;
                        new LateTask(() =>
                        {
                            IsSpeedDown = false;
                        }, CanNotMoveTime.GetFloat(), "Time bomber speed reset");
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && PlayerControl.LocalPlayer.IsRole(RoleId.TimeBomber); },
                () => { return PlayerControl.LocalPlayer.CanMove && NowTarget.Count != 0; },
                () => { ResetStartCoolDown(); },
                RoleClass.SelfBomber.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("TimeBomberStartName"),
                showButtonText = true
            };
        }

        public static void ResetStartCoolDown()
        {
            StartButton.Timer = StartTime.GetFloat();
            StartButton.MaxTimer = StartTime.GetFloat();
        }
        public static void ResetBombCoolDown()
        {
            BombButton.Timer = NowBombTime;
            BombButton.MaxTimer = NowBombTime;
        }

        public static void ArrowUpdate()
        {
            try
            {
                if (!IsArrow.GetBool()) return;
                if (RoleClass.IsMeeting) { ARROW.arrow.SetActive(false); return; }
                if (ARROW != null)
                {
                    if (AllTarget.Count != 0)
                    {
                        foreach (PlayerControl p in AllTarget)
                        {
                            if (p.IsDead())
                            {
                                if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, p.transform.position) <= ArrowScope.GetFloat())
                                {
                                    ARROW.arrow.SetActive(true);
                                    ARROW.Update(p.transform.position);
                                }
                                else
                                    ARROW.arrow.SetActive(false);
                            }
                        }
                    }
                    else
                        ARROW.arrow.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Logger.Info($"ArrowUpdateで例外が発生{e}", "TimeBomber");
            }
        }
    }
}