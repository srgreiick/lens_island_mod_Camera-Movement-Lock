using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using InputControllerPluginMod;

namespace InputControllerPluginMod
{
    [BepInPlugin("com.tsukasaroot.inputcontroller", "Allow to lock into camera view", "1.0.1")]
    public class InputControllerPlugin : BaseUnityPlugin
    {
        public static ManualLogSource _logger;
        private readonly Harmony harmony = new Harmony("com.tsukasaroot.inputcontroller");
        internal static ConfigEntry<KeyCode> configShortCut;
        public static bool isMouseHold = false;

        void Awake()
        {
            _logger = Logger;
            configShortCut = Config.Bind("Toggle Settings", "toggleLock", KeyCode.F11, "Set the key as toggler to lock camera");
            _logger.LogInfo("Patching CameraMovementLock...");
            harmony.PatchAll(typeof(InputPatchs));
            _logger.LogInfo($"Plugin {Info.Metadata.Name} v{Info.Metadata.Version} loaded.");
        }
    }
}

[HarmonyPatch]
internal static class InputPatchs
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private static ManualLogSource _logger = InputControllerPlugin._logger;
    
    [HarmonyPatch(typeof(CameraController), "Update")]
    [HarmonyPostfix]
    static void Update_Postfix(CameraController __instance)
    {
        if (Input.GetKeyDown(InputControllerPlugin.configShortCut.Value))
        {
            _logger.LogInfo("F11 pressed, toggling mouse hold");
            
            if (InputControllerPlugin.isMouseHold)
            {
                SimulateMouseUp();
                _logger.LogInfo("Stopped left mouse hold");
            }
            else
            {
                SimulateMouseDown();
                _logger.LogInfo("Simulating left mouse hold");
            }
        }

        if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.F) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.Comma))
        {
            if (Input.GetMouseButton(1))
            {
                SimulateMouseUp();
            }
        }
    }

    private static void SimulateMouseDown()
    {
        try
        {
            InputControllerPlugin.isMouseHold = true;
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to simulate mouse down: {e.Message}");
        }
    }

    private static void SimulateMouseUp()
    {
        try
        {
            InputControllerPlugin.isMouseHold = false;
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        catch (Exception e)
        {
            InputControllerPlugin._logger.LogError($"Failed to simulate mouse up: {e.Message}");
        }
    }
}