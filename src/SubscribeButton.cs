using MelonLoader;
using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace SubscribeButtonMod
{
    public class SubscribeButton : MonoBehaviour
    {
        public SubscribeButton(IntPtr ptr) : base(ptr) { }

        static readonly Color YT_RED         = new Color(1.00f, 0.00f, 0.00f);
        static readonly Color YT_RED_PRESSED = new Color(0.70f, 0.00f, 0.00f);

        Renderer    _faceRenderer;
        AudioSource _audio;
        Vector3     _originalScale;
        bool        _ready = true;
        const float COOLDOWN = 2.5f;

        // Called by ButtonSpawner after adding the component
        public void Init(Renderer faceRenderer)
        {
            _faceRenderer  = faceRenderer;
            _originalScale = transform.localScale;

            _audio                  = gameObject.AddComponent<AudioSource>();
            _audio.spatialBlend     = 1f;   // full 3D
            _audio.maxDistance      = 8f;
            _audio.rolloffMode      = AudioRolloffMode.Linear;

            MelonCoroutines.Start(LoadAudio());
        }

        IEnumerator LoadAudio()
        {
            string path = Path.Combine(Core.ModDataPath, "subscribe.wav");
            if (!File.Exists(path)) yield break;

            // UnityWebRequest approach for loading WAV at runtime
            using var req = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(
                "file:///" + path.Replace("\\", "/"),
                AudioType.WAV
            );
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                _audio.clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(req);
                Core.Instance.LoggerInstance.Msg("Subscribe sound loaded!");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_ready) return;

            // Detect player hands by name or tag (covers both Quest and PC VR)
            bool isHand = other.gameObject.name.ToLower().Contains("hand")
                       || other.gameObject.tag  == "Hand"
                       || other.gameObject.layer == LayerMask.NameToLayer("Player");

            if (isHand) Press();
        }

        void Press()
        {
            _ready = false;
            MelonCoroutines.Start(PressRoutine());
        }

        IEnumerator PressRoutine()
        {
            // ── Visual: squish the face ───────────────────────────────────
            if (_faceRenderer != null)
                _faceRenderer.material.color = YT_RED_PRESSED;

            transform.localScale = new Vector3(
                _originalScale.x * 0.92f,
                _originalScale.y * 0.60f,
                _originalScale.z
            );

            // ── Audio ─────────────────────────────────────────────────────
            if (_audio != null && _audio.clip != null)
                _audio.Play();

            // ── Particles ─────────────────────────────────────────────────
            SpawnConfetti();

            // ── Floating text "Thanks!" ───────────────────────────────────
            SpawnThanksText();

            yield return new WaitForSeconds(0.15f);

            // Release
            transform.localScale = _originalScale;
            if (_faceRenderer != null)
                _faceRenderer.material.color = YT_RED;

            yield return new WaitForSeconds(COOLDOWN);
            _ready = true;
        }

        void SpawnConfetti()
        {
            var go = new GameObject("Confetti");
            go.transform.position = transform.position + Vector3.back * 0.1f;

            var ps   = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 2f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(1.5f, 4f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.maxParticles  = 60;
            main.gravityModifier = 0.4f;
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0f, 0f),   // YouTube red
                new Color(1f, 1f, 1f)    // white
            );

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale     = new Vector3(0.28f, 0.1f, 0.02f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x       = new ParticleSystem.MinMaxCurve(-1f, 1f);
            vel.y       = new ParticleSystem.MinMaxCurve(0.5f, 2f);

            ps.Play();
            Destroy(go, 3f);
        }

        void SpawnThanksText()
        {
            var go = new GameObject("ThanksText");
            go.transform.position = transform.position + Vector3.up * 0.25f;

            var tm       = go.AddComponent<TextMesh>();
            tm.text      = "Thanks! 🔔";
            tm.fontSize  = 40;
            tm.color     = Color.white;
            tm.anchor    = TextAnchor.MiddleCenter;
            tm.characterSize = 0.05f;
            tm.fontStyle = FontStyle.Bold;

            MelonCoroutines.Start(FloatAndFade(go));
        }

        IEnumerator FloatAndFade(GameObject go)
        {
            var tm     = go.GetComponent<TextMesh>();
            float t    = 0f;
            float dur  = 1.8f;
            Vector3 startPos = go.transform.position;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                go.transform.position = startPos + Vector3.up * (p * 0.4f);
                if (tm != null)
                    tm.color = new Color(1f, 1f, 1f, 1f - p);
                yield return null;
            }

            Destroy(go);
        }
    }
}
