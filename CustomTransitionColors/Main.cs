using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI.CCK.Components;
using ABI_RC.Core.Player;
using MelonLoader;
using DarkRift;
using Dissonance.Integrations.DarkRift2;
using Dissonance;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

[assembly: MelonInfo(typeof(CustomTransitionColors.Main), "CustomTransitionColors", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace CustomTransitionColors
{
    public class Main : MelonMod
    {
        public static MelonPreferences_Category transitionCategory;

        public static MelonPreferences_Entry<Color> gridColor;
        public static MelonPreferences_Entry<Color> baseColor;
        public static MelonPreferences_Entry<Color> fadeColor;

        public override void OnApplicationStart()
        {
            transitionCategory = MelonPreferences.CreateCategory("CustomTransitionColors");
            gridColor = transitionCategory.CreateEntry("gridColor", new Color(1, 0, 0, 1));
            baseColor = transitionCategory.CreateEntry("baseColor", new Color(0.5f, 0.5f, 0.5f, 1));
            fadeColor = transitionCategory.CreateEntry("fadeColor", new Color(0.5f, 0.5f, 0.5f, 1));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == 3)
            {
                UpdateTransitionProperties();
            }
        }

        public override void OnPreferencesSaved()
        {
            UpdateTransitionProperties();
        }

        private void UpdateTransitionProperties()
        {
            if (!PlayerSetup.Instance) return;

            var transitionMaterialDesktop = PlayerSetup.Instance.transitionEffectDesktop.material;
            transitionMaterialDesktop.SetColor("_GridColor", gridColor.Value);
            transitionMaterialDesktop.SetColor("_BaseColor", baseColor.Value);
            transitionMaterialDesktop.SetColor("_FadeColor", fadeColor.Value);

            var transitionMaterialVR = PlayerSetup.Instance.transitionEffectVr.material;
            transitionMaterialVR.SetColor("_GridColor", gridColor.Value);
            transitionMaterialVR.SetColor("_BaseColor", baseColor.Value);
            transitionMaterialVR.SetColor("_FadeColor", fadeColor.Value);
        }
    }
}
