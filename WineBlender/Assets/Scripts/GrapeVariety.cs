using UnityEngine;

namespace WineBlender
{
    public enum WineColor { White, Red }

    // A grape the player can add to the blend. Values are rough real-world averages:
    // acidity/tannin/body/sweetness on a 1-10 scale, alcohol in % ABV.
    public class GrapeVariety
    {
        public string Name;
        public WineColor Color;
        public float Acidity;
        public float Alcohol;
        public float Tannin;
        public float Body;
        public float Sweetness;
        public string[] Notes;
        public string Hint;

        public GrapeVariety(string name, WineColor color, float acidity, float alcohol, float tannin,
            float body, float sweetness, string[] notes, string hint)
        {
            Name = name;
            Color = color;
            Acidity = acidity;
            Alcohol = alcohol;
            Tannin = tannin;
            Body = body;
            Sweetness = sweetness;
            Notes = notes;
            Hint = hint;
        }

        public Color SwatchColor => Color == WineColor.Red
            ? new Color(0.45f, 0.06f, 0.14f)
            : new Color(0.93f, 0.85f, 0.45f);
    }

    public static class GrapeDatabase
    {
        public static readonly GrapeVariety[] All =
        {
            new GrapeVariety("Cabernet Sauvignon", WineColor.Red, 6f, 14.0f, 9f, 8f, 2f,
                new[] { "blackcurrant", "cedar", "mint" }, "Grippy, full · blackcurrant, cedar"),
            new GrapeVariety("Merlot", WineColor.Red, 5f, 13.5f, 5f, 6f, 3f,
                new[] { "plum", "chocolate" }, "Soft, round · plum, chocolate"),
            new GrapeVariety("Pinot Noir", WineColor.Red, 7f, 13.0f, 3f, 3f, 2f,
                new[] { "cherry", "raspberry", "mushroom" }, "Light, fresh · cherry, raspberry"),
            new GrapeVariety("Syrah", WineColor.Red, 5f, 14.5f, 7f, 8f, 2f,
                new[] { "blackberry", "black pepper", "smoke" }, "Bold, spicy · blackberry, pepper"),
            new GrapeVariety("Grenache", WineColor.Red, 4f, 15.0f, 4f, 6f, 3f,
                new[] { "strawberry", "cinnamon" }, "Warm, juicy · strawberry, spice"),
            new GrapeVariety("Sauvignon Blanc", WineColor.White, 9f, 12.5f, 1f, 3f, 2f,
                new[] { "grapefruit", "cut grass" }, "Zesty, light · grapefruit, grass"),
            new GrapeVariety("Chardonnay", WineColor.White, 5f, 13.5f, 1f, 7f, 2f,
                new[] { "butter", "apple", "vanilla" }, "Creamy, full · butter, apple"),
            new GrapeVariety("Riesling", WineColor.White, 9f, 10.0f, 1f, 3f, 6f,
                new[] { "lime", "honey", "peach" }, "Racy, a touch sweet · lime, honey"),
        };
    }
}
