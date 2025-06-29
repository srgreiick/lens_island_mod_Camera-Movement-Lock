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
    [BepInPlugin("com.tsukasaroot.inputcontroller", "Allow to lock into camera view", "1.0.0")]
    public class InputControllerPlugin : BaseUnityPlugin
    {
        public static InputControllerPlugin Instance { get; private set; }
        public static ManualLogSource _logger;
        private readonly Harmony harmony = new Harmony("com.tsukasaroot.inputcontroller");
        internal static ConfigEntry<KeyCode> configShortCut;
        public static bool isMouseHold = false;

        void Awake()
        {
            Instance = this;
            _logger = Logger;
            configShortCut = Config.Bind("Test", "test", KeyCode.LeftAlt, "test");

            _logger.LogInfo("Patching InputDebugger...");
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
            InputControllerPlugin.isMouseHold = !InputControllerPlugin.isMouseHold;

            if (InputControllerPlugin.isMouseHold)
            {
                SimulateMouseDown();
                _logger.LogInfo("Simulating left mouse hold");
            }
            else
            {
                SimulateMouseUp();
                _logger.LogInfo("Stopped left mouse hold");
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            if (InputControllerPlugin.isMouseHold == true)
            {
                InputControllerPlugin.isMouseHold = false;
                SimulateMouseUp();
            }
        }

        if (Input.GetKey(KeyCode.F))
        {
            InputControllerPlugin.isMouseHold = false;
            SimulateMouseUp();
        }
    }

    private static void SimulateMouseDown()
    {
        try
        {
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
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        catch (Exception e)
        {
            InputControllerPlugin._logger.LogError($"Failed to simulate mouse up: {e.Message}");
        }
    }
}