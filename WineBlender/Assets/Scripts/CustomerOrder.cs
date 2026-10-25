using System.Collections.Generic;
using UnityEngine;

namespace WineBlender
{
    public class CustomerOrder
    {
        public string CustomerName;
        public int[] TargetPercents;
        public WineProfile Target;
        public List<string> RequestedNotes = new List<string>();
        public string Dialogue;
        public float Patience;

        static readonly string[] Names =
        {
            "Noa", "Itai", "Maya", "Yoni", "Tamar", "Omer", "Shira", "Avi",
            "Lia", "Eitan", "Dana", "Gil", "Rina", "Ben", "Yael", "Ron"
        };

        static readonly string[] Greetings =
        {
            "Hi there!", "Good evening!", "Shalom!", "Hello!", "Hey, busy night?"
        };

        public static CustomerOrder Generate(int day, System.Random rng)
        {
            var order = new CustomerOrder();
            order.CustomerName = Names[rng.Next(Names.Length)];
            order.Patience = Mathf.Max(40f, 75f - (day - 1) * 8f);

            // Pick a color, then 1-3 grapes of that color (more grapes on later days).
            WineColor color = rng.NextDouble() < 0.6 ? WineColor.Red : WineColor.White;
            var candidates = new List<int>();
            for (int i = 0; i < GrapeDatabase.All.Length; i++)
                if (GrapeDatabase.All[i].Color == color)
                    candidates.Add(i);

            int maxGrapes = Mathf.Min(day == 1 ? 2 : 3, candidates.Count);
            int grapeCount = rng.Next(1, maxGrapes + 1);
            Shuffle(candidates, rng);

            order.TargetPercents = new int[GrapeDatabase.All.Length];
            int[] split = SplitIntoTens(grapeCount, rng);
            for (int i = 0; i < grapeCount; i++)
                order.TargetPercents[candidates[i]] = split[i];

            order.Target = WineProfile.FromBlend(order.TargetPercents);

            // Ask for one note from each of the two biggest grapes (or two from a single grape).
            var main = GrapeDatabase.All[candidates[0]];
            order.RequestedNotes.Add(main.Notes[rng.Next(main.Notes.Length)]);
            if (grapeCount > 1 && split[1] >= WineProfile.NoteThreshold)
            {
                var second = GrapeDatabase.All[candidates[1]];
                order.RequestedNotes.Add(second.Notes[rng.Next(second.Notes.Length)]);
            }
            else
            {
                foreach (string note in main.Notes)
                    if (!order.RequestedNotes.Contains(note))
                    {
                        order.RequestedNotes.Add(note);
                        break;
                    }
            }

            order.Dialogue = BuildDialogue(order, rng);
            return order;
        }

        static string BuildDialogue(CustomerOrder order, System.Random rng)
        {
            WineProfile t = order.Target;
            string text = Greetings[rng.Next(Greetings.Length)] + " ";
            text += $"I'd like {WineProfile.WithArticle($"{WineProfile.DescribeSweetness(t.Sweetness)} {WineProfile.DescribeColor(t.Redness)}")} " +
                    $"with {WineProfile.DescribeAcidity(t.Acidity)}";
            if (t.Redness > 0.25f)
                text += $", {WineProfile.DescribeTannin(t.Tannin)}";
            text += $" and {WineProfile.DescribeBody(t.Body)}. ";
            text += $"Something {WineProfile.DescribeAlcohol(t.Alcohol)}. ";
            text += $"I love notes of {order.RequestedNotes[0]}";
            if (order.RequestedNotes.Count > 1)
                text += $" and {order.RequestedNotes[1]}";
            return text + ".";
        }

        // Splits 100% into `parts` chunks, each a multiple of 10 and at least 20%,
        // biggest first so the first grape is always the dominant one.
        static int[] SplitIntoTens(int parts, System.Random rng)
        {
            var result = new int[parts];
            int remaining = 10 - parts * 2;
            for (int i = 0; i < parts; i++)
                result[i] = 2;
            while (remaining > 0)
            {
                result[rng.Next(parts)]++;
                remaining--;
            }
            System.Array.Sort(result);
            System.Array.Reverse(result);
            for (int i = 0; i < parts; i++)
                result[i] *= 10;
            return result;
        }

        static void Shuffle(List<int> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ---- Judging what the player served ----

        public struct Evaluation
        {
            public float Score; // 0..1
            public List<string> Feedback;
            public int Pay;
        }

        public Evaluation Evaluate(int[] servedPercents, float patienceLeftFraction)
        {
            WineProfile served = WineProfile.FromBlend(servedPercents);
            var feedback = new List<string>();
            float weighted = 0f, weights = 0f;

            void Compare(string label, float got, float want, float range, float weight)
            {
                float error = Mathf.Abs(got - want) / range;
                weighted += Mathf.Clamp01(1f - error / 0.35f) * weight;
                weights += weight;

                string verdict;
                if (error < 0.08f) verdict = "just right";
                else if (error < 0.2f) verdict = got > want ? "a little too high" : "a little too low";
                else verdict = got > want ? "way too high" : "way too low";
                feedback.Add($"{label}: {verdict}");
            }

            Compare("Color", served.Redness, Target.Redness, 1f, 2f);
            Compare("Acidity", served.Acidity, Target.Acidity, 9f, 1f);
            Compare("Alcohol", served.Alcohol, Target.Alcohol, 5f, 1f);
            if (Target.Redness > 0.25f)
                Compare("Tannin", served.Tannin, Target.Tannin, 9f, 1f);
            Compare("Body", served.Body, Target.Body, 9f, 1f);
            Compare("Sweetness", served.Sweetness, Target.Sweetness, 9f, 1f);

            int notesFound = 0;
            foreach (string note in RequestedNotes)
            {
                bool found = served.Notes.Contains(note);
                if (found) notesFound++;
                feedback.Add($"Notes of {note}: {(found ? "yes!" : "missing")}");
            }

            float attributeScore = weighted / weights;
            float noteScore = notesFound / (float)RequestedNotes.Count;
            float score = attributeScore * 0.75f + noteScore * 0.25f;

            int pay = Mathf.RoundToInt(15f + 35f * score);
            if (score > 0.85f)
                pay += Mathf.RoundToInt(10f * patienceLeftFraction); // tip for a quick, great pour

            return new Evaluation { Score = score, Feedback = feedback, Pay = pay };
        }

        public string RecipeText()
        {
            var parts = new List<string>();
            for (int i = 0; i < TargetPercents.Length; i++)
                if (TargetPercents[i] > 0)
                    parts.Add($"{TargetPercents[i]}% {GrapeDatabase.All[i].Name}");
            return string.Join(" + ", parts);
        }
    }
}
