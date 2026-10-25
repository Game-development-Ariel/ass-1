using System.Collections.Generic;
using UnityEngine;

namespace WineBlender
{
    // The measurable character of a wine, computed as a weighted average of its grapes.
    public struct WineProfile
    {
        public float Acidity;
        public float Alcohol;
        public float Tannin;
        public float Body;
        public float Sweetness;
        public float Redness; // 0 = all white grapes, 1 = all red grapes
        public List<string> Notes;

        // A grape needs at least this share of the blend for its aromas to come through.
        public const int NoteThreshold = 20;

        public static WineProfile FromBlend(int[] percents)
        {
            var profile = new WineProfile { Notes = new List<string>() };
            int total = 0;
            for (int i = 0; i < percents.Length; i++)
                total += percents[i];
            if (total == 0)
                return profile;

            for (int i = 0; i < percents.Length; i++)
            {
                if (percents[i] == 0)
                    continue;
                GrapeVariety g = GrapeDatabase.All[i];
                float w = percents[i] / (float)total;
                profile.Acidity += g.Acidity * w;
                profile.Alcohol += g.Alcohol * w;
                profile.Tannin += g.Tannin * w;
                profile.Body += g.Body * w;
                profile.Sweetness += g.Sweetness * w;
                if (g.Color == WineColor.Red)
                    profile.Redness += w;

                if (percents[i] * 100 / total >= NoteThreshold)
                {
                    foreach (string note in g.Notes)
                        if (!profile.Notes.Contains(note))
                            profile.Notes.Add(note);
                }
            }
            return profile;
        }

        // ---- Words a customer (or the player tasting) would use ----

        public static string DescribeColor(float redness)
        {
            if (redness > 0.75f) return "red";
            if (redness < 0.25f) return "white";
            return "rosé-style";
        }

        public static string DescribeSweetness(float s)
        {
            if (s < 3f) return "dry";
            if (s < 4.5f) return "off-dry";
            return "slightly sweet";
        }

        public static string DescribeAcidity(float a)
        {
            if (a < 4.5f) return "soft, gentle acidity";
            if (a < 6.5f) return "balanced acidity";
            if (a < 8f) return "fresh, crisp acidity";
            return "zingy, mouth-watering acidity";
        }

        public static string DescribeTannin(float t)
        {
            if (t < 2.5f) return "no tannins";
            if (t < 4.5f) return "silky tannins";
            if (t < 6.5f) return "medium tannins";
            return "firm, grippy tannins";
        }

        public static string DescribeBody(float b)
        {
            if (b < 4f) return "a light body";
            if (b < 6.5f) return "a medium body";
            return "a full body";
        }

        public static string DescribeAlcohol(float abv)
        {
            if (abv < 11.5f) return "light and low in alcohol";
            if (abv < 13f) return "moderate in alcohol";
            if (abv < 14f) return "fairly strong";
            return "warm and high in alcohol";
        }

        // "a dry red" / "an off-dry white"
        public static string WithArticle(string phrase, bool capitalized = false)
        {
            string article = "aeiou".IndexOf(char.ToLower(phrase[0])) >= 0 ? "an" : "a";
            if (capitalized)
                article = char.ToUpper(article[0]) + article.Substring(1);
            return article + " " + phrase;
        }

        public string DescribeTaste()
        {
            if (Notes == null)
                return "The glass is empty.";

            string text = $"{WithArticle($"{DescribeSweetness(Sweetness)} {DescribeColor(Redness)}", true)} with {DescribeAcidity(Acidity)}";
            if (Redness > 0.25f)
                text += $", {DescribeTannin(Tannin)}";
            text += $" and {DescribeBody(Body)}. It feels {DescribeAlcohol(Alcohol)}.";
            if (Notes.Count > 0)
                text += $"\nNotes of {string.Join(", ", Notes)}.";
            else
                text += "\nNo aroma stands out.";
            return text;
        }

        public Color GlassColor()
        {
            var white = new Color(0.95f, 0.86f, 0.42f, 0.95f);
            var red = new Color(0.42f, 0.03f, 0.12f, 0.95f);
            Color c = Color.Lerp(white, red, Redness);
            // Fuller wines look a little deeper in the glass.
            var deep = new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, c.a);
            return Color.Lerp(c, deep, Mathf.InverseLerp(3f, 9f, Body));
        }
    }
}
