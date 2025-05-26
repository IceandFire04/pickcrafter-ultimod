using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(Ultimod.Ultimod), "Ultimod", "0.1", "IceandFire04")] // This is where you put your mod info. Theres also an optional download link.
[assembly: MelonGame("fiveamp", "PickCrafter")] // For some reason, FiveAmp is lowercase here.

namespace Ultimod;

public class Ultimod : MelonMod
{
    public bool PickaxeTextVisible = false; // This is a variable that can be used to toggle the visibility of the pickaxe text.

    public override void OnInitializeMelon()
    {
        // These two lines allow use to use the patches at the bottom of the file.
        var Harmony = new HarmonyLib.Harmony("com.iceandfire04.pickcrafter.ultimod");
        Harmony.PatchAll(); // Apply patches.

        Melon<Ultimod>.Logger.Msg("Ultimod initialized and patched!");
    }

    public override void OnLateUpdate()
    {
        PickaxeController? pc = PickCrafterUtils.GetPickaxeController(); // Get the pickaxe controller.

        if (Input.GetKeyDown(KeyCode.F1)) // Press F1 to check for pickaxe controllers.
        {
            PickCrafterUtils.FindPickaxeControllers();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            PickaxeTextVisible = !PickaxeTextVisible; // Toggle the visibility of the pickaxe text.

            if (PickaxeTextVisible)
            {
                // Subscribing to OnGUI means that this function will be called every time OnGUI is.
                MelonEvents.OnGUI.Subscribe(DrawPickaxeText, 100);
            }
            else
            {
                // Unsubscribing reverts this.
                MelonEvents.OnGUI.Unsubscribe(DrawPickaxeText);
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            pc!.setActivePickaxe(pc!.GetBestPickaxe()); // Set the active pickaxe to the best one.
            PickaxeGO_Controller.pickaxeGameObjectSwap(pc!.ActivePickaxe, new(180f, 180f, 180f));
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            RunicDust rd = Object.FindObjectOfType<RunicDust>(); // Get the runic dust controller.
            rd.Award(15, RunicDust.RunicRewardOrigins.RunicMachine);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            pc!.UpgradePickaxe(pc!.ActivePickaxe, pc!.ActivePickaxe.maxLevel, true); // Upgrade the active pickaxe.

        }

        // This code always triggers in-game.
        if (SceneManager.GetActiveScene().name == "DefaultScene")
        {
            // This bit of code sets all chests to be ready and automatically opens them.
            var cc = Object.FindObjectOfType<ChestController>(); // Get the chest controller.

            if (cc && cc != null) // Make sure it actually exists.
            {
                foreach (ChestData? cd in cc.chestDatas) // Loop through all the chest data.
                {
                    if (cd != null) // If the chest data isn't null.
                    {
                        // When modifying chests, use ChestSlotSaveData to modify the chest enitity, time left, etc.
                        ChestSlotSaveData? csd = cd.chestSlotSaveData; // Get the chest slot save data.
                        if (csd.chestState != ChestData.ChestState.Empty) // Make sure it's not an empty slot so errors don't get spammed.
                        {
                            csd.chestState = ChestData.ChestState.Ready; // Ready the chest.
                            cd.OpenChest(); // Automatically open the chest.
                        }
                    }
                }
            }
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        Melon<Ultimod>.Logger.Msg($"Scene loaded: {sceneName}"); // Log the scene name.
    }

    public static void DrawPickaxeText()
    {
        PickaxeController? pc = PickCrafterUtils.GetPickaxeController(); // Get the pickaxe controller.
        PickaxeData activePickaxe = pc!.ActivePickaxe; // Get the active pickaxe.

        pc!.GetStrAndCritStrings(activePickaxe, out string strength, out string crit); // Get the strength and crit strings. These are only visual.
        string text = $"<color=#00CCFF><size=20>Pickaxe Info: Strength: {strength}, Crit: {crit}, BaseDamage: {pc!.GetPickaxe(activePickaxe.id).baseDamage}, TrueBaseDamage: {activePickaxe.baseDamage}</size></color>";

        GUI.Box(new Rect(20, 20, 1000, 40), text); // Draw the text on the screen.
    }
}

public static class PickCrafterUtils
{
    public static void FindPickaxeControllers()
    {
        Melon<Ultimod>.Logger.Msg($"Finding PickaxeControllers..."); // Log the scene name.
        foreach (PickaxeController pc in Object.FindObjectsOfType<PickaxeController>()) // Loop through all the pickaxe controllers.
        {
            // Log info about them.
            Melon<Ultimod>.Logger.Msg($"Found Pickaxe Controller {pc.name} with ActivePickaxe {pc.ActivePickaxe.name} (Parent \"{pc.gameObject.GetParent()}\")");
        }
        Melon<Ultimod>.Logger.Msg($"Found {Object.FindObjectsOfType<PickaxeController>().Length} Pickaxe Controllers in current scene."); // Say when done.
    }

    public static PickaxeController? GetPickaxeController()
    {
        var pc = Object.FindObjectOfType<PickaxeController>(); // Get the first pickaxe controller in the scene.

        if (pc) return pc; // If it exists, return it.
        else return null; // Elsewise, return null.
    }
}

#region patches

// This line denotes the start of a patch. It needs the class the function is apart of and the name of it.
// The MelonLoader wiki goes into more detail.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.IsUnlocked))]
public class UnlockPickaxes // The name can be anything.
{
    // Prefix() runs before the original function.
    static bool Prefix(ref bool __result)
    {
        __result = true; // Always return true, meaning any pickaxe is unlocked.
        return false; // Don't run the original function.
    }

    // Postfix() runs after the original function.
    static void Postfix(ref bool __result)
    {
        // Nothing to do here.
    }
}

// This patch is pretty simple: every pickaxe is shown, even if from a higher prestige.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.GetIsHidden))]
public class ShowHiddenPickaxes
{
    static bool Prefix(ref bool __result)
    {
        __result = false; // Same deal as last time.
        return false;
    }
}

// This patch is a little more complicated. I'll try my best to break it down.
// Since every pickaxe is pseudo-unlocked, it reads garbage values for the pickaxe's base damage.
// However, pickaxes' base damage is still stored in the game, just somewhere else.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.GetDamage))]
public class CorrectPickaxeDamage
{
    static bool Prefix(ref double __result)
    {
        PickaxeController? pc = PickCrafterUtils.GetPickaxeController(); // Get the pickaxe controller.
        PickaxeData activePickaxe = pc!.ActivePickaxe; // Get the active pickaxe.

        // If the pickaxe is a vanity pickaxe, we use the best pickaxe's base damage since vanity pickaxes all have 0 damage.
        // Else, just use the real base damage of the pickaxe.
        __result = activePickaxe.isVanity ? pc.GetBestPickaxe().baseDamage : pc!.GetPickaxe(activePickaxe.id).baseDamage;
        return false;
    }
}

// This patch makes pickaxe powers have little cooldown.
// If you wanted to you could get a PickaxeController and see what power is being used and then set the cooldown for only that power.
[HarmonyPatch(typeof(PickaxePowerController), nameof(PickaxePowerController.GetBaseCoolDown))]
public class ShortPowerCooldowns
{
    static bool Prefix(ref float __result)
    {
        __result = 10f; // Here we just set the cooldown to 10 seconds.
        return false;
    }
}

[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.IsUpgradableNow))]
public class AlwaysUpgradable
{
    static bool Prefix(ref bool __result)
    {
        __result = true; // Always return true, meaning the pickaxe is always upgradable.
        return false; // Don't run the original function.
    }
}
#endregion
