using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PixelTamagotchi
{
    [Serializable]
    public class PetSave
    {
        public bool Adopted;
        public string PetName;
        public string SpeciesId;
        public string ColorId;
        public long BirthdayUnix;
        public long LastUpdateUnix;
        public float Hunger = 90;
        public float Happiness = 90;
        public float Cleanliness = 100;
        public float Energy = 95;
        public float Health = 100;
        public int Coins = 80;
        public int TotalHatches = 0;
    }

    public class PixelTamagotchiApp : MonoBehaviour
    {
        private const string SaveKey = "pixel_tamagotchi_unity_save_v1";
        private PetSave save;
        private Canvas canvas;
        private RectTransform root;
        private RawImage petImage;
        private Text titleText;
        private Text statsText;
        private Text messageText;
        private Font font;
        private float frameTimer;
        private int frameIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            if (FindObjectOfType<PixelTamagotchiApp>() != null) return;
            var go = new GameObject("PixelTamagotchiApp");
            DontDestroyOnLoad(go);
            go.AddComponent<PixelTamagotchiApp>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Load();
            SettleOffline();
            BuildUi();
            Render("欢迎来到独立版像素拓麻歌子。当前是 Unity 迁移原型。黑描边 + 主色/辅色/点缀色来自原脚本像素数据。");
        }

        private void Update()
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 0.6f)
            {
                frameTimer = 0;
                frameIndex++;
                RenderPetTexture();
            }
        }

        private static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private static float ClampStat(float v) => Mathf.Clamp(v, 0, 100);

        private void Load()
        {
            var raw = PlayerPrefs.GetString(SaveKey, "");
            save = string.IsNullOrEmpty(raw) ? new PetSave() : JsonUtility.FromJson<PetSave>(raw);
            if (save == null) save = new PetSave();
        }

        private void Save()
        {
            save.LastUpdateUnix = NowUnix();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        private void SettleOffline()
        {
            if (!save.Adopted) return;
            long now = NowUnix();
            long last = save.LastUpdateUnix <= 0 ? now : save.LastUpdateUnix;
            float hours = Mathf.Clamp((now - last) / 3600f, 0, 24 * 30);
            if (hours <= 0.01f) return;
            save.Hunger = ClampStat(save.Hunger - 3.5f * hours);
            save.Happiness = ClampStat(save.Happiness - 1.7f * hours);
            save.Cleanliness = ClampStat(save.Cleanliness - 2.2f * hours);
            save.Energy = ClampStat(save.Energy - 2.8f * hours);
            if (save.Hunger <= 0 || save.Cleanliness <= 0) save.Health = ClampStat(save.Health - 2.5f * hours);
            Save();
        }

        private void BuildUi()
        {
            canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var ev = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                DontDestroyOnLoad(ev);
            }

            root = Panel(canvas.transform, "Root", new Color32(238, 221, 160, 255), new Vector2(0.5f, 0.5f), new Vector2(930, 1500));
            titleText = Label(root, "Title", "像素拓麻歌子", 54, TextAnchor.MiddleCenter, new Vector2(0, 650), new Vector2(860, 90));
            var screen = Panel(root, "Screen", new Color32(205, 226, 179, 255), new Vector2(0.5f, 0.5f), new Vector2(760, 760));
            screen.anchoredPosition = new Vector2(0, 180);
            petImage = new GameObject("PetPreview", typeof(RawImage)).GetComponent<RawImage>();
            petImage.transform.SetParent(screen, false);
            petImage.rectTransform.sizeDelta = new Vector2(520, 520);
            petImage.rectTransform.anchoredPosition = new Vector2(0, 40);
            statsText = Label(screen, "Stats", "", 30, TextAnchor.UpperLeft, new Vector2(0, -275), new Vector2(680, 145));
            messageText = Label(root, "Message", "", 32, TextAnchor.UpperCenter, new Vector2(0, -295), new Vector2(820, 170));

            var buttonY = -520;
            Button(root, "领养 / 重随机", new Vector2(-280, buttonY), () => AdoptRandom());
            Button(root, "喂食", new Vector2(0, buttonY), () => Action("喂食", 25, 4, -3, 4));
            Button(root, "玩耍", new Vector2(280, buttonY), () => Action("玩耍", -6, 24, -8, -4));
            Button(root, "清洁", new Vector2(-140, buttonY - 125), () => { save.Cleanliness = ClampStat(save.Cleanliness + 32); save.Happiness = ClampStat(save.Happiness + 3); Save(); Render("洗得干干净净，像素都亮起来了。"); });
            Button(root, "睡觉", new Vector2(140, buttonY - 125), () => { save.Energy = ClampStat(save.Energy + 35); save.Hunger = ClampStat(save.Hunger - 5); Save(); Render("它睡了一会儿，精神恢复了。"); });
        }

        private RectTransform Panel(Transform parent, string name, Color color, Vector2 pivot, Vector2 size)
        {
            var img = new GameObject(name, typeof(Image)).GetComponent<Image>();
            img.transform.SetParent(parent, false);
            img.color = color;
            var rt = img.rectTransform;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            return rt;
        }

        private Text Label(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 pos, Vector2 rect)
        {
            var t = new GameObject(name, typeof(Text)).GetComponent<Text>();
            t.transform.SetParent(parent, false);
            t.font = font;
            t.fontSize = size;
            t.color = new Color32(38, 53, 50, 255);
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = text;
            t.rectTransform.sizeDelta = rect;
            t.rectTransform.anchoredPosition = pos;
            return t;
        }

        private void Button(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(text, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color32(49, 71, 95, 255);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(245, 92);
            rt.anchoredPosition = pos;
            go.GetComponent<Button>().onClick.AddListener(onClick);
            var txt = Label(go.transform, "Text", text, 30, TextAnchor.MiddleCenter, Vector2.zero, rt.sizeDelta);
            txt.color = Color.white;
        }

        private void AdoptRandom()
        {
            var species = GeneratedPetDatabase.Species[Random.Range(0, GeneratedPetDatabase.Species.Length)];
            var colorArr = GeneratedPetDatabase.FindColors(species.Id);
            var color = colorArr[Random.Range(0, colorArr.Length)];
            long now = NowUnix();
            save = new PetSave
            {
                Adopted = true,
                PetName = color.Name + species.Name,
                SpeciesId = species.Id,
                ColorId = color.Id,
                BirthdayUnix = now,
                LastUpdateUnix = now,
                Hunger = 90,
                Happiness = 90,
                Cleanliness = 100,
                Energy = 95,
                Health = 100,
                Coins = 80,
                TotalHatches = save.TotalHatches + 1
            };
            Save();
            Render("你领养了「" + save.PetName + "」。后续正式版可以接入取名、蛋孵化、商店、日程和小游戏。");
        }

        private void Action(string name, float hunger, float happiness, float energy, float cleanliness)
        {
            if (!EnsurePet()) return;
            save.Hunger = ClampStat(save.Hunger + hunger);
            save.Happiness = ClampStat(save.Happiness + happiness);
            save.Energy = ClampStat(save.Energy + energy);
            save.Cleanliness = ClampStat(save.Cleanliness + cleanliness);
            Save();
            Render(save.PetName + "完成了「" + name + "」。");
        }

        private bool EnsurePet()
        {
            if (save.Adopted) return true;
            Render("还没有宠物，先点「领养 / 重随机」。");
            return false;
        }

        private void Render(string message)
        {
            titleText.text = save.Adopted ? save.PetName + " · 独立版原型" : "像素拓麻歌子 · 领养中心";
            statsText.text = save.Adopted
                ? $"饱腹 {save.Hunger:0}  心情 {save.Happiness:0}\n清洁 {save.Cleanliness:0}  精力 {save.Energy:0}\n健康 {save.Health:0}  金币 {save.Coins}"
                : "尚未领养宠物。";
            messageText.text = message;
            RenderPetTexture();
        }

        private void RenderPetTexture()
        {
            if (petImage == null) return;
            if (!save.Adopted)
            {
                petImage.texture = MakePlaceholderEgg();
                return;
            }
            var species = GeneratedPetDatabase.FindSpecies(save.SpeciesId);
            var color = GeneratedPetDatabase.FindColor(save.SpeciesId, save.ColorId);
            if (species == null || color == null) return;
            var frames = species.Frames;
            var frame = frames[Mathf.Abs(frameIndex) % frames.Length];
            petImage.texture = MakeTexture(frame, color);
        }

        private Texture2D MakePlaceholderEgg()
        {
            string[] egg = {
                "................", "................", "......KKKK......", ".....KAAAAK.....",
                "....KAAAAAAK....", "...KAAAAAAAK....", "...KAAAAAAAK....", "..KAAAABAAAAK...",
                "..KAAAAABAAAK...", "..KAAAAAAAAAK...", "...KAAAAAAAK....", "....KAAAAAK.....",
                ".....KKKKK......", "................", "................", "................"
            };
            var c = new ColorVariant { Primary = "#fff8df", Secondary = "#e5c66e", Accent = "#ef8f9d" };
            return MakeTexture(egg, c);
        }

        private Texture2D MakeTexture(string[] rows, ColorVariant variant)
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color transparent = new Color(0, 0, 0, 0);
            Color outline = Hex("#263532");
            Color primary = Hex(variant.Primary);
            Color secondary = Hex(variant.Secondary);
            Color accent = Hex(variant.Accent);
            Color white = Hex("#fff8df");
            for (int y = 0; y < 16; y++)
            {
                string row = y < rows.Length ? rows[y] : "";
                for (int x = 0; x < 16; x++)
                {
                    char ch = x < row.Length ? row[x] : '.';
                    Color col = transparent;
                    if (ch == 'K') col = outline;
                    else if (ch == 'A') col = primary;
                    else if (ch == 'B') col = secondary;
                    else if (ch == 'P') col = accent;
                    else if (ch == 'W') col = white;
                    tex.SetPixel(x, 15 - y, col);
                }
            }
            tex.Apply(false, false);
            return tex;
        }

        private Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }
    }
}
