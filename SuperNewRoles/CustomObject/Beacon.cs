using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class Beacon
    {
        public static List<Beacon> AllBeacons = new();
        public static Sprite[] beaconAnimationSprites = new Sprite[3];
        public static int maxid;
        public int id = 0;

        public static Sprite GetBeaconAnimationSprite(int index)
        {
            if (beaconAnimationSprites == null || beaconAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, beaconAnimationSprites.Length - 1);
            return beaconAnimationSprites[index];
        }


        public static void StartAnimation(int Id)
        {
            Beacon beacon = AllBeacons.FirstOrDefault((x) => x.id == Id);
            if (beacon == null) return;

            CustomAnimation Conjurer_Beacon_Animation = new()
            {
                Sprites = CustomAnimation.GetSprites("SuperNewRoles.Resources.ConjurerAnimation.Conjurer_Beacon", 60)
            };
            Transform Conjurer_Beacon1 = GameObject.Instantiate(GameObject.Find($"Beacon{beacon.id}").transform);
            Conjurer_Beacon_Animation.Start(30, Conjurer_Beacon1);
        }

        private readonly GameObject GameObject;
        public Vent vent;
        private readonly SpriteRenderer BeaconRenderer;
        static Transform Animation;

        public Beacon(Vector2 p)
        {
            GameObject = new GameObject($"Beacon{Conjurer.Count}") { layer = 11 };
            //GameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
            position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
                                                                            // Create the marker
            id = maxid;
            maxid++;
            GameObject.transform.position = position;
            BeaconRenderer = GameObject.AddComponent<SpriteRenderer>();
            CustomAnimation Conjurer_Beacon_Animation = new()
            {
                Sprites = CustomAnimation.GetSprites("SuperNewRoles.Resources.ConjurerAnimation.Conjurer_Beacon", 60)
            };
            Animation = GameObject.Instantiate(GameObject.Find($"Beacon{id}").transform, this.GameObject.transform);
            Conjurer_Beacon_Animation.Start(30, Animation);
            BeaconRenderer.sprite = GetBeaconAnimationSprite(0);
            // Only render the beacon for the conjurer
            var playerIsTrickster = PlayerControl.LocalPlayer;
            GameObject.SetActive(playerIsTrickster);

            AllBeacons.Add(this);
        }


        public static void ClearBeacons()
        {
            maxid = 0;
            int[] num = { 1, 2, 3 };
            foreach (var n in num)
            {
                Logger.Info($"Beacon{n}をClearします", "ClearBeacons");
                if (GameObject.Find($"Beacon{n}") != null)
                    GameObject.Destroy(GameObject.Find($"Beacon{n}"));
                if (GameObject.Find($"Beacon{n}(Clone)") != null)
                    GameObject.Destroy(GameObject.Find($"Beacon{n}(Clone)"));
            }
        }
    }
}