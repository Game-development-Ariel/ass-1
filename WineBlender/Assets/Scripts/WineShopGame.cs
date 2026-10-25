using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WineBlender
{
    // Builds the whole wine bar UI from code and runs the day / customer loop.
    public class WineShopGame : MonoBehaviour
    {
        const int CustomersPerDay = 5;
        const float TasteCost = 5f; // seconds of patience each tasting costs
        const int Step = 10;

        enum State { Entering, Ordering, Result, DayEnd }

        static readonly Color Wall = new Color(0.17f, 0.10f, 0.08f);
        static readonly Color PanelDark = new Color(0.10f, 0.06f, 0.05f, 0.92f);
        static readonly Color RowColor = new Color(0.24f, 0.15f, 0.12f);
        static readonly Color Cream = new Color(0.98f, 0.93f, 0.84f);
        static readonly Color Gold = new Color(0.95f, 0.75f, 0.35f);
        static readonly Color WineRed = new Color(0.55f, 0.10f, 0.18f);
        static readonly Color Wood = new Color(0.40f, 0.24f, 0.14f);
        static readonly Color Green = new Color(0.25f, 0.55f, 0.32f);
        static readonly Color Grey = new Color(0.35f, 0.33f, 0.32f);

        static readonly Color[] SkinTones =
        {
            new Color(0.98f, 0.84f, 0.72f), new Color(0.90f, 0.72f, 0.56f),
            new Color(0.76f, 0.57f, 0.42f), new Color(0.55f, 0.38f, 0.26f), new Color(0.40f, 0.27f, 0.18f)
        };
        static readonly Color[] HairColors =
        {
            new Color(0.12f, 0.08f, 0.06f), new Color(0.35f, 0.20f, 0.10f),
            new Color(0.80f, 0.62f, 0.30f), new Color(0.60f, 0.25f, 0.12f), new Color(0.75f, 0.75f, 0.75f)
        };
        static readonly Color[] ShirtColors =
        {
            new Color(0.20f, 0.35f, 0.60f), new Color(0.55f, 0.20f, 0.25f), new Color(0.25f, 0.50f, 0.40f),
            new Color(0.60f, 0.50f, 0.20f), new Color(0.40f, 0.30f, 0.55f)
        };

        readonly System.Random rng = new System.Random();
        readonly int[] percents = new int[GrapeDatabase.All.Length];

        State state;
        int day = 1;
        int customerIndex;
        int money;
        float dayScoreSum;
        int dayEarnings;
        CustomerOrder order;
        float patienceLeft;

        // UI references
        Text dayText, moneyText, customerCountText;
        Text[] percentTexts;
        Text totalText, tasteText;
        Image glassFill;
        Button tasteButton, serveButton;
        RectTransform customerRoot;
        Image head, hair, shirt, mouth;
        Text customerNameText;
        Image patienceFill;
        GameObject speechBubble;
        Text speechText;
        GameObject overlay;
        Text overlayTitle, overlayBody;
        Button overlayButton;
        Text overlayButtonLabel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<WineShopGame>() == null)
                new GameObject("WineShopGame").AddComponent<WineShopGame>();
        }

        void Start()
        {
            if (Camera.main != null)
                Camera.main.backgroundColor = Wall;
            EnsureEventSystem();
            BuildUI();
            RefreshBlend();
            RefreshHud();
            StartCoroutine(CustomerEnters());
        }

        void Update()
        {
            if (state != State.Ordering)
                return;
            patienceLeft -= Time.deltaTime;
            UpdatePatienceBar();
            if (patienceLeft <= 0f)
                CustomerGivesUp();
        }

        // ---------------- Game flow ----------------

        IEnumerator CustomerEnters()
        {
            state = State.Entering;
            ClearBlend();
            speechBubble.SetActive(false);
            tasteText.text = "Add grapes with + and -, taste the blend, then serve it.";

            order = CustomerOrder.Generate(day, rng);
            patienceLeft = order.Patience;
            DressCustomer();
            customerNameText.text = order.CustomerName;
            UpdatePatienceBar();
            RefreshHud();

            yield return Slide(customerRoot, new Vector2(900f, 0f), Vector2.zero, 0.7f);

            speechText.text = order.Dialogue;
            speechBubble.SetActive(true);
            SetBlendingEnabled(true);
            state = State.Ordering;
        }

        void Serve()
        {
            if (state != State.Ordering)
                return;
            if (TotalPercent() != 100)
            {
                tasteText.text = "The blend has to add up to exactly 100% before you can serve it.";
                return;
            }

            CustomerOrder.Evaluation result = order.Evaluate(percents, patienceLeft / order.Patience);
            money += result.Pay;
            dayEarnings += result.Pay;
            dayScoreSum += result.Score;

            string reaction;
            if (result.Score >= 0.9f) reaction = "Perfect! Exactly what I wanted!";
            else if (result.Score >= 0.75f) reaction = "Lovely, I really like this one.";
            else if (result.Score >= 0.5f) reaction = "Hmm, it's okay...";
            else reaction = "This is not what I asked for.";
            SetMouth(result.Score);
            speechText.text = reaction;

            string body = $"Match: {Mathf.RoundToInt(result.Score * 100)}%   ·   Paid: ${result.Pay}\n\n" +
                          string.Join("\n", result.Feedback) +
                          $"\n\nThe ideal blend was:\n{order.RecipeText()}";
            ShowOverlay(reaction, body, "Next customer", NextCustomer);
            state = State.Result;
            SetBlendingEnabled(false);
            RefreshHud();
        }

        void CustomerGivesUp()
        {
            state = State.Result;
            SetBlendingEnabled(false);
            SetMouth(0f);
            speechText.text = "I've waited long enough. Goodbye!";
            ShowOverlay($"{order.CustomerName} left without a drink",
                $"They lost patience. Try tasting less and serving sooner.\n\nThe ideal blend was:\n{order.RecipeText()}",
                "Next customer", NextCustomer);
        }

        void NextCustomer()
        {
            overlay.SetActive(false);
            StartCoroutine(CustomerLeaves());
        }

        IEnumerator CustomerLeaves()
        {
            speechBubble.SetActive(false);
            yield return Slide(customerRoot, Vector2.zero, new Vector2(-900f, 0f), 0.5f);
            customerIndex++;
            if (customerIndex >= CustomersPerDay)
            {
                state = State.DayEnd;
                float rating = dayScoreSum / CustomersPerDay * 5f;
                ShowOverlay($"Day {day} is over!",
                    $"You earned ${dayEarnings} today.\nAverage rating: {rating:0.0} / 5 stars\n\n" +
                    "Tomorrow customers want more complex blends and have less patience.",
                    $"Start day {day + 1}", StartNextDay);
            }
            else
            {
                StartCoroutine(CustomerEnters());
            }
        }

        void StartNextDay()
        {
            overlay.SetActive(false);
            day++;
            customerIndex = 0;
            dayScoreSum = 0f;
            dayEarnings = 0;
            StartCoroutine(CustomerEnters());
        }

        // ---------------- Blending ----------------

        void ChangePercent(int index, int delta)
        {
            if (state != State.Ordering)
                return;
            int next = percents[index] + delta;
            if (next < 0 || TotalPercent() - percents[index] + next > 100)
                return;
            percents[index] = next;
            RefreshBlend();
        }

        void Taste()
        {
            if (state != State.Ordering)
                return;
            if (TotalPercent() == 0)
            {
                tasteText.text = "The glass is empty. Add some grapes first.";
                return;
            }
            tasteText.text = WineProfile.FromBlend(percents).DescribeTaste();
            patienceLeft -= TasteCost;
            UpdatePatienceBar();
        }

        void ClearBlend()
        {
            for (int i = 0; i < percents.Length; i++)
                percents[i] = 0;
            RefreshBlend();
        }

        int TotalPercent()
        {
            int total = 0;
            foreach (int p in percents)
                total += p;
            return total;
        }

        void RefreshBlend()
        {
            for (int i = 0; i < percents.Length; i++)
                percentTexts[i].text = percents[i] + "%";
            int total = TotalPercent();
            totalText.text = $"Total: {total}% / 100%";
            totalText.color = total == 100 ? Gold : Cream;
            glassFill.fillAmount = total / 100f * 0.62f;
            glassFill.color = WineProfile.FromBlend(percents).GlassColor();
            if (serveButton != null)
                serveButton.interactable = state == State.Ordering && total == 100;
        }

        void SetBlendingEnabled(bool enabled)
        {
            tasteButton.interactable = enabled;
            serveButton.interactable = enabled && TotalPercent() == 100;
        }

        // ---------------- Customer visuals ----------------

        void DressCustomer()
        {
            head.color = SkinTones[rng.Next(SkinTones.Length)];
            hair.color = HairColors[rng.Next(HairColors.Length)];
            shirt.color = ShirtColors[rng.Next(ShirtColors.Length)];
            SetMouth(0.6f);
        }

        void SetMouth(float mood)
        {
            // Wider, lighter mouth when happy; a small dark line when not.
            var rt = mouth.rectTransform;
            rt.sizeDelta = new Vector2(Mathf.Lerp(26f, 60f, mood), Mathf.Lerp(8f, 16f, mood));
            mouth.color = mood >= 0.5f ? new Color(0.65f, 0.15f, 0.18f) : new Color(0.25f, 0.12f, 0.10f);
        }

        void UpdatePatienceBar()
        {
            // Shrink the bar's right edge rather than using a Filled image, so the rounded ends keep their shape.
            float fraction = Mathf.Clamp01(patienceLeft / order.Patience);
            patienceFill.rectTransform.anchorMax = new Vector2(Mathf.Lerp(0.05f, 0.95f, fraction), 0.42f);
            patienceFill.enabled = fraction > 0.02f;
            patienceFill.color = Color.Lerp(new Color(0.80f, 0.20f, 0.15f), Green, fraction);
        }

        static IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            rt.anchoredPosition = to;
        }

        // ---------------- HUD / overlay ----------------

        void RefreshHud()
        {
            dayText.text = $"Day {day}";
            customerCountText.text = $"Customer {Mathf.Min(customerIndex + 1, CustomersPerDay)} / {CustomersPerDay}";
            moneyText.text = $"${money}";
        }

        void ShowOverlay(string title, string body, string buttonLabel, UnityEngine.Events.UnityAction onClick)
        {
            overlayTitle.text = title;
            overlayBody.text = body;
            overlayButtonLabel.text = buttonLabel;
            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(onClick);
            overlay.SetActive(true);
        }

        // ---------------- UI construction ----------------

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            Transform root = canvasGo.transform;

            UiKit.Panel("Background", root, Vector2.zero, Vector2.one, Wall, false);
            BuildTopBar(root);
            BuildCellar(UiKit.Panel("Cellar", root, new Vector2(0.012f, 0.025f), new Vector2(0.34f, 0.905f), PanelDark).transform);
            BuildShopFloor(UiKit.Rect("ShopFloor", root, new Vector2(0.352f, 0.025f), new Vector2(0.648f, 0.905f)));
            BuildStation(UiKit.Panel("Station", root, new Vector2(0.66f, 0.025f), new Vector2(0.988f, 0.905f), PanelDark).transform);
            BuildOverlay(root);
        }

        void BuildTopBar(Transform root)
        {
            Transform bar = UiKit.Panel("TopBar", root, new Vector2(0f, 0.92f), Vector2.one, new Color(0.08f, 0.04f, 0.04f), false).transform;
            UiKit.Label("Title", bar, new Vector2(0.015f, 0f), new Vector2(0.4f, 1f), "Vino Blend Bar", 44, Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            dayText = UiKit.Label("Day", bar, new Vector2(0.4f, 0f), new Vector2(0.5f, 1f), "", 32, Cream);
            customerCountText = UiKit.Label("Customers", bar, new Vector2(0.5f, 0f), new Vector2(0.68f, 1f), "", 32, Cream);
            moneyText = UiKit.Label("Money", bar, new Vector2(0.75f, 0f), new Vector2(0.985f, 1f), "", 40, Gold, TextAnchor.MiddleRight, FontStyle.Bold);
        }

        void BuildCellar(Transform panel)
        {
            UiKit.Label("Title", panel, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f), "Wine Cellar", 34, Gold, TextAnchor.MiddleLeft, FontStyle.Bold);

            int count = GrapeDatabase.All.Length;
            percentTexts = new Text[count];
            float top = 0.91f, bottom = 0.015f;
            float rowHeight = (top - bottom) / count;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                GrapeVariety grape = GrapeDatabase.All[i];
                float yMax = top - i * rowHeight;
                Transform row = UiKit.Panel(grape.Name, panel, new Vector2(0.03f, yMax - rowHeight + 0.006f), new Vector2(0.97f, yMax), RowColor).transform;

                RectTransform dot = UiKit.Rect("Swatch", row, new Vector2(0.025f, 0.3f), new Vector2(0.025f, 0.7f));
                dot.sizeDelta = new Vector2(30f, 0f);
                dot.anchoredPosition = new Vector2(15f, 0f);
                UiKit.Shape(dot, UiKit.Circle, grape.SwatchColor).preserveAspect = true;

                UiKit.Label("Name", row, new Vector2(0.1f, 0.48f), new Vector2(0.62f, 0.95f), grape.Name, 26, Cream, TextAnchor.LowerLeft, FontStyle.Bold);
                UiKit.Label("Hint", row, new Vector2(0.1f, 0.05f), new Vector2(0.62f, 0.5f), grape.Hint, 18, new Color(0.85f, 0.78f, 0.68f), TextAnchor.UpperLeft);

                UiKit.Button("Minus", row, new Vector2(0.64f, 0.2f), new Vector2(0.74f, 0.8f), "-", Grey, () => ChangePercent(index, -Step), 34);
                percentTexts[i] = UiKit.Label("Percent", row, new Vector2(0.74f, 0f), new Vector2(0.87f, 1f), "0%", 26, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Button("Plus", row, new Vector2(0.87f, 0.2f), new Vector2(0.97f, 0.8f), "+", WineRed, () => ChangePercent(index, Step), 34);
            }
        }

        void BuildShopFloor(RectTransform area)
        {
            // Back wall with shelves of bottles.
            UiKit.Panel("BackWall", area, Vector2.zero, Vector2.one, new Color(0.30f, 0.19f, 0.13f));
            for (int s = 0; s < 2; s++)
            {
                float y = 0.42f + s * 0.17f;
                UiKit.Panel("Shelf", area, new Vector2(0.05f, y), new Vector2(0.95f, y + 0.015f), Wood, false);
                for (int b = 0; b < 9; b++)
                {
                    float x = 0.08f + b * 0.097f;
                    Color bottle = (b + s) % 3 == 0 ? new Color(0.85f, 0.80f, 0.45f) : new Color(0.35f, 0.05f, 0.10f);
                    UiKit.Panel("Bottle", area, new Vector2(x, y + 0.015f), new Vector2(x + 0.045f, y + 0.12f), bottle);
                }
            }

            // The customer: hair behind the head, then head, eyes and mouth, then shirt.
            customerRoot = UiKit.FixedRect("Customer", area, new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(320f, 440f));
            customerRoot.pivot = new Vector2(0.5f, 0f);
            shirt = UiKit.Shape(UiKit.FixedRect("Shirt", customerRoot, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(300f, 240f)), UiKit.Rounded, Color.white);
            shirt.type = Image.Type.Sliced;
            hair = UiKit.Shape(UiKit.FixedRect("Hair", customerRoot, new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(200f, 200f)), UiKit.Circle, Color.white);
            head = UiKit.Shape(UiKit.FixedRect("Head", customerRoot, new Vector2(0.5f, 0f), new Vector2(0f, 310f), new Vector2(180f, 180f)), UiKit.Circle, Color.white);
            var eyeColor = new Color(0.12f, 0.08f, 0.08f);
            UiKit.Shape(UiKit.FixedRect("EyeL", head.transform, new Vector2(0.5f, 0.5f), new Vector2(-34f, 14f), new Vector2(20f, 20f)), UiKit.Circle, eyeColor);
            UiKit.Shape(UiKit.FixedRect("EyeR", head.transform, new Vector2(0.5f, 0.5f), new Vector2(34f, 14f), new Vector2(20f, 20f)), UiKit.Circle, eyeColor);
            mouth = UiKit.Shape(UiKit.FixedRect("Mouth", head.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -38f), new Vector2(50f, 14f)), UiKit.Rounded, Color.white);
            mouth.type = Image.Type.Sliced;
            mouth.pixelsPerUnitMultiplier = 4f;

            // Speech bubble above the customer.
            Image bubble = UiKit.Panel("SpeechBubble", area, new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.99f), Cream);
            speechBubble = bubble.gameObject;
            speechText = UiKit.Label("Text", bubble.transform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f), "", 25,
                new Color(0.18f, 0.10f, 0.08f), TextAnchor.MiddleLeft);
            speechText.verticalOverflow = VerticalWrapMode.Truncate;
            speechText.resizeTextForBestFit = true;
            speechText.resizeTextMinSize = 16;
            speechText.resizeTextMaxSize = 26;

            // Counter in front of the customer.
            Transform counter = UiKit.Panel("Counter", area, Vector2.zero, new Vector2(1f, 0.22f), Wood).transform;
            UiKit.Panel("CounterTop", counter, new Vector2(0f, 0.88f), Vector2.one, new Color(0.55f, 0.35f, 0.20f), false);
            customerNameText = UiKit.Label("Name", counter, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.85f), "", 28, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Label("PatienceLabel", counter, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.85f), "Patience", 22, new Color(0.9f, 0.82f, 0.7f), TextAnchor.MiddleRight);
            UiKit.Panel("PatienceBack", counter, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.42f), new Color(0f, 0f, 0f, 0.4f));
            patienceFill = UiKit.Panel("PatienceFill", counter, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.42f), Green);
            patienceFill.pixelsPerUnitMultiplier = 2f;
        }

        void BuildStation(Transform panel)
        {
            UiKit.Label("Title", panel, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f), "Blending Station", 34, Gold, TextAnchor.MiddleLeft, FontStyle.Bold);

            // A wine glass: the bowl fills from the bottom with the blend's color.
            RectTransform glass = UiKit.FixedRect("Glass", panel, new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(260f, 380f));
            UiKit.Shape(UiKit.FixedRect("Base", glass, new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(150f, 26f)), UiKit.Circle, new Color(1f, 1f, 1f, 0.35f));
            UiKit.Shape(UiKit.FixedRect("Stem", glass, new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(14f, 120f)), null, new Color(1f, 1f, 1f, 0.35f));
            RectTransform bowlRect = UiKit.FixedRect("Bowl", glass, new Vector2(0.5f, 0f), new Vector2(0f, 255f), new Vector2(250f, 250f));
            UiKit.Shape(bowlRect, UiKit.Circle, new Color(1f, 1f, 1f, 0.18f));
            glassFill = UiKit.Shape(UiKit.Rect("Wine", bowlRect, Vector2.zero, Vector2.one), UiKit.Circle, Color.clear);
            glassFill.type = Image.Type.Filled;
            glassFill.fillMethod = Image.FillMethod.Vertical;
            glassFill.fillOrigin = (int)Image.OriginVertical.Bottom;

            totalText = UiKit.Label("Total", panel, new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.42f), "", 30, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);

            Transform notes = UiKit.Panel("TastingNotes", panel, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.355f), RowColor).transform;
            tasteText = UiKit.Label("Text", notes, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f), "", 22, Cream, TextAnchor.MiddleLeft);
            tasteText.verticalOverflow = VerticalWrapMode.Truncate;
            tasteText.resizeTextForBestFit = true;
            tasteText.resizeTextMinSize = 14;
            tasteText.resizeTextMaxSize = 23;

            UiKit.Button("Reset", panel, new Vector2(0.05f, 0.03f), new Vector2(0.3f, 0.12f), "Reset", Grey, () => { if (state == State.Ordering) ClearBlend(); }, 28);
            tasteButton = UiKit.Button("Taste", panel, new Vector2(0.34f, 0.03f), new Vector2(0.64f, 0.12f), $"Taste (-{TasteCost:0}s)", new Color(0.55f, 0.40f, 0.15f), Taste, 26);
            serveButton = UiKit.Button("Serve", panel, new Vector2(0.68f, 0.03f), new Vector2(0.95f, 0.12f), "Serve", Green, Serve, 30);
            SetBlendingEnabled(false);
        }

        void BuildOverlay(Transform root)
        {
            Image dim = UiKit.Panel("Overlay", root, Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.6f), false);
            overlay = dim.gameObject;
            Transform card = UiKit.Panel("Card", dim.transform, new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.88f), Cream).transform;
            var dark = new Color(0.18f, 0.10f, 0.08f);
            overlayTitle = UiKit.Label("Title", card, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f), "", 38, WineRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            overlayBody = UiKit.Label("Body", card, new Vector2(0.07f, 0.17f), new Vector2(0.93f, 0.84f), "", 26, dark, TextAnchor.UpperCenter);
            overlayBody.verticalOverflow = VerticalWrapMode.Truncate;
            overlayBody.resizeTextForBestFit = true;
            overlayBody.resizeTextMinSize = 16;
            overlayBody.resizeTextMaxSize = 26;
            overlayButton = UiKit.Button("Button", card, new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.13f), "", WineRed, () => { }, 30);
            overlayButtonLabel = overlayButton.GetComponentInChildren<Text>();
            overlay.SetActive(false);
        }
    }
}
