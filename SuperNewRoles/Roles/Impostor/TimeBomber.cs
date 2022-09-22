using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Patch;

using UnityEngine;




using COT = SuperNewRoles.Patch.CustomOptionType;

namespace SuperNewRoles.Roles.Impostor
{
    public class TimeBomber
    {
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption KillCool;          // キルクール
        public static CustomOption StartTime;         // 起動するまでの時間
        public static CustomOption BombTime;          // 爆発するまでの時間
        public static CustomOption BombScope;         // 爆発のキル半径
        public static CustomOption CanNotMoveTime;    // 起爆時動けない時間
        public static CustomOption ExtensionKillCool; // 起爆時に伸びるキルクールの量
        public static CustomOption IsArrow;           // 起爆時に矢印を表示するか
        public static CustomOption ArrowScope;        // 起爆時に矢印を表示する半径

        public static void SetupCustomOptions()
        {
            var id = 992;
            Option = new(id, false, COT.Impostor, "TimeBomberName", color, 1);
            PlayerCount = CustomOption.Create(id + 1, false, COT.Impostor, "SettingPlayerCountName", 1f, 1f, 5f, 1f, Option);
            KillCool = CustomOption.Create(id + 2, false, COT.Impostor, "KillCoolDown", 20, 0, 60, 2.5f, Option);
            StartTime = CustomOption.Create(id + 3, false, COT.Impostor, "TimeToStart", 5, 0, 15, 0.5f, Option);
            BombTime = CustomOption.Create(id + 4, false, COT.Impostor, "TimeToBomb", 15, 0, 45, 0.5f, Option);
            BombScope = CustomOption.Create(id + 5, false, COT.Impostor, "BombScope", 1, 0.5f, 3, 0.5f, Option);
            CanNotMoveTime = CustomOption.Create(id + 6, false, COT.Impostor, "CanNotMoveTime", 5, 0, 10, 0.5f, Option);
            ExtensionKillCool = CustomOption.Create(id + 7, false, COT.Impostor, "ExtensionKillCool", 10, 0, 30, 2.5f, Option);
            IsArrow = CustomOption.Create(id + 8, false, COT.Impostor, "BombArrow", true, Option);
            ArrowScope = CustomOption.Create(id + 9, false, COT.Impostor, "ArrowScope", 4, 0.5f, 10, 0.5f, IsArrow);
        }

        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;

        public static void ClearAndReload()
        {
            Player = new();
        }
    }
}