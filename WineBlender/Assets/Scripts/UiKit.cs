using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WineBlender
{
    // Small helpers for building uGUI from code, so the game needs no prefabs or imported art.
    public static class UiKit
    {
        static Font font;
        static Sprite rounded;
        static Sprite circle;

        public static Font Font => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        public static Sprite Rounded => rounded != null ? rounded : (rounded = MakeRounded(64, 18));
        public static Sprite Circle => circle != null ? circle : (circle = MakeCircle(128));

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        // A fixed-size rect (in reference pixels) placed at an anchor point.
        public static RectTransform FixedRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform rt = Rect(name, parent, anchor, anchor);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return rt;
        }

        public static Image Panel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, bool roundedCorners = true)
        {
            Image img = Rect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Image>();
            img.color = color;
            if (roundedCorners)
            {
                img.sprite = Rounded;
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        public static Image Shape(RectTransform rt, Sprite sprite, Color color)
        {
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        public static Text Label(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text,
            int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            Text t = Rect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = alignment;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string label,
            Color color, UnityAction onClick, int fontSize = 30)
        {
            Image bg = Panel(name, parent, anchorMin, anchorMax, Color.white);
            var button = bg.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
            colors.selectedColor = color;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.35f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            Label("Label", bg.transform, Vector2.zero, Vector2.one, label, fontSize, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            return button;
        }

        static Sprite MakeRounded(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d + 0.5f)));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d)));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
