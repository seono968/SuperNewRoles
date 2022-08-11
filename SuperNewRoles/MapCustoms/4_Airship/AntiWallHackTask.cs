using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.MapCustoms.Airship
{
    public static class AntiWallHackTask
    {
        public static void HandleIntro()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AntiWallHackTask.GetBool())
            {
                UnityEngine.GameObject.FindObjectsOfType<Console>().All(console =>
                {
                    switch (console.name)
                    {
                        case "task_garbage1":
                        case "task_garbage2":
                        case "task_garbage3":
                        case "task_garbage4":
                        case "task_garbage5":
                        case "task_shower":
                        case "task_developphotos":
                        case "DivertRecieve" when console.Room == SystemTypes.Armory || console.Room == SystemTypes.MainHall:
                            console.checkWalls = true;
                            break;
                    }
                    return false;
                });
            }
        }
    }
}
