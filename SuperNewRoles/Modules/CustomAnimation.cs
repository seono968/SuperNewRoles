using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    public class CustomAnimation : MonoBehaviour
    {
        public bool Playing;
        public bool IsLoop;
        public float Framerate;
        public SpriteRenderer render;
        private float updatetime;
        private float updatedefaulttime;
        private int index;
        public Sprite[] Sprites;
        /// <summary>
        /// 指定したパスの連番のファイルを取得できます。
        /// </summary>
        /// <param name="path">そこまでのパス</param>
        /// <param name="num">連番数です。1～6だと5です。</param>
        /// <returns></returns>
        public static Sprite[] GetSprites(string path, int num, float pixelsPerUnit = 115f)
        {
            List<Sprite> Sprites = new();
            for (int i = 1; i < num + 1; i++)
            {
                string countdata = "000" + i.ToString();
                if (i >= 10)
                {
                    if (i >= 100)
                    {
                        countdata = "0" + i.ToString();
                    }
                    else
                    {
                        countdata = "00" + i.ToString();
                    }
                }
                Sprites.Add(ModHelpers.LoadSpriteFromResources(path + countdata + ".png", pixelsPerUnit));
            }
            return Sprites.ToArray();
        }
        public void Init(Sprite[] sprites, bool isLoop, float framerate, SpriteRenderer render)
        {
            Sprites = sprites;
            IsLoop = isLoop;
            Playing = false;
            Framerate = framerate;
            updatedefaulttime = 1 / framerate;
            updatetime = updatedefaulttime;
            index = 0;
            if (render == null)
            {
                render = gameObject.GetComponent<SpriteRenderer>();
            }
            this.render = render;
        }
        public void Stop()
        {
            Playing = false;
        }
        public void Play()
        {
            Playing = true;
        }
        public void BeginPlay()
        {
            Playing = true;
            index = 0;
            updatetime = updatedefaulttime;
        }
        public void FixedUpdate()
        {
            if (!Playing) return;
            updatetime -= Time.fixedDeltaTime;
            if (updatetime <= 0)
            {
                index++;
                if (Sprites.Length <= index)
                {
                    if (IsLoop)
                    {
                        index = 0;
                    } else
                    {
                        Playing = false;
                        return;
                    }
                }
                render.sprite = Sprites[index];
                updatetime = updatedefaulttime;
            }
        }
    }
}
