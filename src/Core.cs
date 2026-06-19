using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using System.IO;

[assembly: MelonInfo(typeof(SubscribeButtonMod.Core), "YT Subscribe Button", "1.0.0", "TagtusVR_AC")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace SubscribeButtonMod
{
    public class Core : MelonMod
    {
        internal static Core Instance { get; private set; }
        internal static AudioClip SubscribeClip { get; private set; }
        
        // Path inside BONELAB's UserData folder
        internal static string ModDataPath =>
            Path.Combine(MelonEnvironment.UserDataDirectory, "SubscribeButtonMod");

        public override void OnInitializeMelon()
        {
            Instance = this;

            // Register custom MonoBehaviours with Il2Cpp
            ClassInjector.RegisterTypeInIl2Cpp<SubscribeButton>();
            ClassInjector.RegisterTypeInIl2Cpp<ButtonSpawner>();

            // Make the UserData folder if it doesn't exist
            Directory.CreateDirectory(ModDataPath);

            // Copy bundled audio on first run
            string audioSrc  = Path.Combine(Path.GetDirectoryName(MelonAssembly.Location)!, "subscribe.wav");
            string audioDest = Path.Combine(ModDataPath, "subscribe.wav");
            if (File.Exists(audioSrc) && !File.Exists(audioDest))
                File.Copy(audioSrc, audioDest);

            LoggerInstance.Msg("YouTube Subscribe Button loaded — press B to spawn!");
        }

        public override void OnUpdate()
        {
            // B key spawns the button in front of the camera
            if (Input.GetKeyDown(KeyCode.B))
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    Vector3 spawnPos = cam.transform.position + cam.transform.forward * 1.5f;
                    ButtonSpawner.SpawnButton(spawnPos);
                    LoggerInstance.Msg("Spawned subscribe button!");
                }
            }
        }
    }
}
