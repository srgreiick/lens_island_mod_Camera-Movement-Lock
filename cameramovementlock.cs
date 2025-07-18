using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace InputControllerPluginMod
{
    [BepInPlugin("com.tsukasaroot.inputcontroller", "Allow to lock into camera view", "1.1.0")]
    public class InputControllerPlugin : BaseUnityPlugin
    {
        public static ManualLogSource _logger;
        private readonly Harmony harmony = new Harmony("com.tsukasaroot.inputcontroller");

        internal static ConfigEntry<KeyCode> configHoldKey;
        public static bool isCameraLockActive = false;

        private Texture2D _lineTex;

        void Awake()
        {
            _logger = Logger;

            configHoldKey = Config.Bind(
                "Hold Settings",
                "HoldToLockKey",
                KeyCode.P,
                "Hold this key to simulate holding the right mouse (camera lock)."
            );

            harmony.PatchAll(typeof(InputPatchs));
            _logger.LogInfo($"Plugin {Info.Metadata.Name} v{Info.Metadata.Version} loaded.");
        }

        void OnGUI()
        {
            if (!isCameraLockActive)
                return;

            if (_lineTex == null)
            {
                _lineTex = new Texture2D(1, 1);
                _lineTex.SetPixel(0, 0, Color.white);
                _lineTex.Apply();
            }

            float size = 10f; // Half-length of each line
            float thickness = 2f;

            float xCenter = Screen.width / 2f;
            float yCenter = Screen.height / 2f;

            // Draw horizontal line
            GUI.DrawTexture(new Rect(xCenter - size, yCenter - (thickness / 2), size * 2, thickness), _lineTex);
            // Draw vertical line
            GUI.DrawTexture(new Rect(xCenter - (thickness / 2), yCenter - size, thickness, size * 2), _lineTex);
        }
    }

    [HarmonyPatch]
    internal static class InputPatchs
    {
        private static ManualLogSource _logger => InputControllerPlugin._logger;

        [HarmonyPatch(typeof(CameraController), "Update")]
        [HarmonyPostfix]
        static void Update_Postfix(CameraController __instance)
        {
            KeyCode holdKey = InputControllerPlugin.configHoldKey.Value;

            if (Input.GetKey(holdKey))
            {
                if (!InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = true;

                    __instance.LockCamera = true; // Replace with your actual implementation
                    _logger.LogInfo($"[{holdKey}] held — camera lock ON.");
                }
            }
            else
            {
                if (InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = false;

                    __instance.LockCamera = false;
                    _logger.LogInfo($"[{holdKey}] released — camera lock OFF.");
                }
            }

            if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.F) ||
                Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.M) ||
                Input.GetKeyUp(KeyCode.Comma))
            {
                if (InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = false;

                    __instance.LockCamera = false;
                    _logger.LogInfo("Camera lock released due to interrupt key.");
                }
            }
        }
    }
}
