using System.IO;
using BoneLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BonelabHostileNpc.HostileNpcMod), "HostileNpcMod", "1.0.0", "OpenAI")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace BonelabHostileNpc
{
    public class HostileNpcMod : MelonMod
    {
        private const string BundleName = "ford(pinkspiderhoodiev2).bundle";
        private const string PrefabName = "Ford (PinkSpiderHoodie) UPDATED Variant";
        private AssetBundle _bundle;

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<NpcController>();
            ClassInjector.RegisterTypeInIl2Cpp<HealthComponent>();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(SpawnWhenReady());
        }

        private IEnumerator SpawnWhenReady()
        {
            while (PlayerReferences.Instance == null || PlayerReferences.Instance.Head == null)
            {
                yield return null;
            }

            SpawnNpc(PlayerReferences.Instance.Head.position + PlayerReferences.Instance.Head.forward * 4f);
        }

        private void SpawnNpc(Vector3 position)
        {
            if (_bundle == null)
            {
                var bundlePath = Path.Combine(MelonEnvironment.ModsDirectory, BundleName);
                _bundle = AssetBundle.LoadFromFile(bundlePath);
                if (_bundle == null)
                {
                    MelonLogger.Error($"Failed to load bundle at {bundlePath}");
                    return;
                }
            }

            var prefab = _bundle.LoadAsset<GameObject>(PrefabName);
            if (prefab == null)
            {
                MelonLogger.Error($"Failed to load prefab {PrefabName}");
                return;
            }

            var npcObject = Object.Instantiate(prefab, position, Quaternion.identity);
            if (npcObject.GetComponent<NpcController>() == null)
            {
                npcObject.AddComponent<NpcController>();
            }
        }
    }
}
