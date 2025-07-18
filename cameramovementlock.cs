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

        // Configurable hold key (replacing right mouse)
        internal static ConfigEntry<KeyCode> configHoldKey;

        // Internal state tracking
        public static bool isCameraLockActive = false;

        void Awake()
        {
            _logger = Logger;

            configHoldKey = Config.Bind(
                "Hold Settings",
                "HoldToLockKey",
                KeyCode.P, // Default key
                "Hold this key to simulate holding the right mouse (camera lock)."
            );

            _logger.LogInfo("Patching CameraController.Update...");
            harmony.PatchAll(typeof(InputPatchs));
            _logger.LogInfo($"Plugin {Info.Metadata.Name} v{Info.Metadata.Version} loaded.");
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

            // Check if key is being held
            if (Input.GetKey(holdKey))
            {
                if (!InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = true;

                    // Replace this line with the actual camera lock logic (pseudocode here)
                    __instance.LockCamera = true;
                    _logger.LogInfo($"[{holdKey}] held — camera lock ON.");
                }
            }
            else
            {
                if (InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = false;

                    // Replace this line with the actual camera unlock logic
                    __instance.LockCamera = false;
                    _logger.LogInfo($"[{holdKey}] released — camera lock OFF.");
                }
            }

            // Optional override to force unlock on these key releases
            if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.F) ||
                Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.M) ||
                Input.GetKeyUp(KeyCode.Comma))
            {
                if (InputControllerPlugin.isCameraLockActive)
                {
                    InputControllerPlugin.isCameraLockActive = false;

                    // Force unlock camera (replace this if needed)
                    __instance.LockCamera = false;
                    _logger.LogInfo("Camera lock released due to interrupt key.");
                }
            }
        }
    }
}
