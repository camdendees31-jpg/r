using UnityEngine;
using System;

namespace SubscribeButtonMod
{
    public class ButtonSpawner : MonoBehaviour
    {
        public ButtonSpawner(IntPtr ptr) : base(ptr) { }

        // YouTube brand colors
        static readonly Color YT_RED   = new Color(1.00f, 0.00f, 0.00f);
        static readonly Color YT_DARK  = new Color(0.07f, 0.07f, 0.07f);
        static readonly Color YT_WHITE = Color.white;

        public static GameObject SpawnButton(Vector3 position)
        {
            // ── Root (physics body) ───────────────────────────────────────
            var root = new GameObject("YT_SubscribeButton");
            root.transform.position = position;

            var rb = root.AddComponent<Rigidbody>();
            rb.mass        = 0.4f;
            rb.drag        = 0.5f;
            rb.angularDrag = 1f;

            // Outer box collider for grabbing / physics
            var bodyCol = root.AddComponent<BoxCollider>();
            bodyCol.size = new Vector3(0.36f, 0.16f, 0.08f);

            // ── Dark housing (back plate) ─────────────────────────────────
            var housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            housing.name = "Housing";
            housing.transform.SetParent(root.transform, false);
            housing.transform.localScale = new Vector3(0.36f, 0.16f, 0.07f);
            SetColor(housing, YT_DARK);
            Destroy(housing.GetComponent<Collider>()); // physics handled by root

            // ── Red button face ───────────────────────────────────────────
            var face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "Face";
            face.transform.SetParent(root.transform, false);
            face.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            face.transform.localScale    = new Vector3(0.30f, 0.10f, 0.02f);
            SetColor(face, YT_RED);
            Destroy(face.GetComponent<Collider>());

            // ── "SUBSCRIBE" text label ────────────────────────────────────
            CreateLabel(root.transform, "SUBSCRIBE",    new Vector3(0f,  0.005f, -0.041f), 0.022f, YT_WHITE);
            CreateLabel(root.transform, "TagtusVR_AC", new Vector3(0f, -0.065f, -0.002f), 0.012f, new Color(0.8f, 0.8f, 0.8f));

            // Bell icon (simple sphere + stick using primitives)
            CreateBellIcon(root.transform, new Vector3(0.16f, 0.005f, -0.041f));

            // ── Invisible trigger zone (what the hand presses) ───────────
            var triggerObj = new GameObject("PressZone");
            triggerObj.transform.SetParent(root.transform, false);
            triggerObj.transform.localPosition = new Vector3(0f, 0f, -0.04f);

            var trigger = triggerObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size      = new Vector3(0.30f, 0.10f, 0.015f);

            var btn = triggerObj.AddComponent<SubscribeButton>();
            btn.Init(face.GetComponent<Renderer>());

            return root;
        }

        static void SetColor(GameObject obj, Color color)
        {
            var mat = obj.GetComponent<Renderer>().material;
            mat.color = color;
        }

        static void CreateBellIcon(Transform parent, Vector3 localPos)
        {
            // Bell body
            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Bell";
            bell.transform.SetParent(parent, false);
            bell.transform.localPosition = localPos;
            bell.transform.localScale    = new Vector3(0.018f, 0.018f, 0.008f);
            SetColor(bell, YT_WHITE);
            Destroy(bell.GetComponent<Collider>());

            // Bell handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "BellHandle";
            handle.transform.SetParent(parent, false);
            handle.transform.localPosition = localPos + new Vector3(0f, 0.012f, 0f);
            handle.transform.localScale    = new Vector3(0.004f, 0.006f, 0.004f);
            SetColor(handle, YT_WHITE);
            Destroy(handle.GetComponent<Collider>());
        }

        static void CreateLabel(Transform parent, string text, Vector3 localPos, float size, Color color)
        {
            var obj = new GameObject("Label_" + text);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale    = Vector3.one * size;

            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(10f, 2f);

            // TextMesh (legacy — always available without TMP)
            var textChild = new GameObject("Text");
            textChild.transform.SetParent(obj.transform, false);

            var tm = textChild.AddComponent<TextMesh>();
            tm.text      = text;
            tm.fontSize  = 60;
            tm.color     = color;
            tm.anchor    = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.08f;
            tm.fontStyle = FontStyle.Bold;
        }
    }
}
