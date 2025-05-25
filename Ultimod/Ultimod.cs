using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(Ultimod.Ultimod), "Ultimod", "0.1", "IceandFire04")] // This is where you put your mod info. Theres also an optional download link.
[assembly: MelonGame("fiveamp", "PickCrafter")] // For some reason, FiveAmp is lowercase here.

namespace Ultimod;

public class Ultimod : MelonMod
{
    public bool AllPickaxesUnlocked = false; // This is a variable that can be used to toggle the unlocking of all pickaxes.
    public bool PickaxeTextVisible = false; // This is a variable that can be used to toggle the visibility of the pickaxe text.

    public override void OnInitializeMelon()
    {
        // These two lines let us modify the game instead of just adding new stuff.
        var Harmony = new HarmonyLib.Harmony("com.iceandfire04.pickcrafter.ultimod");
        Harmony.PatchAll(); // Apply patches.

        Melon<Ultimod>.Logger.Msg("Ultimod initialized and patched!");
    }

    public override void OnLateUpdate()
    {

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
            var cc = Object.FindObjectOfType<ChestController>(); // Get the chest controller.
            if (cc && cc != null)
                foreach (ChestData? cd in cc.chestDatas)
                {
                    if (cd != null)
                    {
                        cd.chestEntity.chestDropData.UNLOCK_DURATION = 0;
                        cd.OpenChest();
                    }
                }
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            RunicDust rd = Object.FindObjectOfType<RunicDust>();
            rd.Award(15, RunicDust.RunicRewardOrigins.RunicMachine);
        }

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

public class PickCrafterUtils
{
    public static void FindPickaxeControllers()
    {
        Melon<Ultimod>.Logger.Msg($"Finding PickaxeControllers..."); // Log the scene name.
        foreach (PickaxeController pc in Object.FindObjectsOfType<PickaxeController>())
        {
            Melon<Ultimod>.Logger.Msg($"Found Pickaxe Controller {pc.name} with ActivePickaxe {pc.ActivePickaxe.name} (Parent \"{pc.gameObject.GetParent()}\")");
        }
        Melon<Ultimod>.Logger.Msg($"Found {Object.FindObjectsOfType<PickaxeController>().Length} Pickaxe Controllers in current scene.");
    }

    public static PickaxeController? GetPickaxeController()
    {
        var pc = Object.FindObjectOfType<PickaxeController>();

        if (pc) return pc;
        else return null;
    }
}

#region patches
// This line denotes the start of a patch. It needs the class the function is apart of and the name of it.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.IsUnlocked))]
public class UnlockPickaxes // The name can be anything.
{
    // Prefix() runs before the original function.
    static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }

    // Postfix() runs after the original function.
    static void Postfix(ref bool __result)
    {
        // Nothing to do here.
    }
}

// This patch is pretty simple: every pickaxe is shown.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.GetIsHidden))]
public class ShowHiddenPickaxes
{
    static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}

// This patch is a little more complicated. I'll try my best to break it down.
[HarmonyPatch(typeof(PickaxeController), nameof(PickaxeController.GetDamage))]
public class CorrectPickaxeDamage
{
    static bool Prefix(ref double __result)
    {
        PickaxeController? pc = PickCrafterUtils.GetPickaxeController(); // Get the pickaxe controller.
        PickaxeData activePickaxe = pc!.ActivePickaxe; // Get the active pickaxe.
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
        __result = 10f;
        return false;
    }
}
#endregion