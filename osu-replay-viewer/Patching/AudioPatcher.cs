using HarmonyLib;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Audio;
using osu.Game.Skinning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.Patching
{
    /// <summary>
    /// Apply patches to audio related stuffs in framework, which allow our project
    /// to know when the sample is played. We need that information so we can render
    /// audio without requiring loopback devices or screen recorder (think of digital
    /// audio workspace for example)
    /// </summary>
    public class AudioPatcher
    {
        /// <summary>
        /// Apply patches. Must be called before interacting with osu!
        /// </summary>
        public static void DoPatching()
        {
            var harmony = new Harmony("osureplayrenderer.Audio");
            harmony.PatchAll();
        }

        public static event Action<ISample> OnSamplePlay;

        public static void TriggerOnSamplePlay(ISample sample)
        {
            OnSamplePlay?.Invoke(sample);
        }
    }

    [HarmonyPatch(typeof(PoolableSkinnableSample))]
    [HarmonyPatch("Play")]
    class Patch01
    {
        static void Prefix(PoolableSkinnableSample __instance)
        {
            AudioPatcher.TriggerOnSamplePlay(__instance.Sample);
        }
    }
}
