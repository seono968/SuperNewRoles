using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Patch;

using UnityEngine;




using COT =SuperNewRoles.Patch.CustomOptionType;

namespace SuperNewRoles.Roles.Impostor
{
    public class TimeBomber
    {
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        public static void SetupCustomOptions()
        {
            var id = 992;
            Option = new(id, false, COT.Impostor, "TimeBomberName", color, 1);
            PlayerCount = CustomOption.Create(id+1, false, COT.Impostor, "SettingPlayerCountName", 1f, 1f, 5f, 1f, Option);
        }

        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;

        public static void ClearAndReload(){
            Player = new();
        }
    }
}