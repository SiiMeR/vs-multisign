using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace MultiSign;

public class MultiSignModSystem : ModSystem
{
    public static string SignedByNames = "SignedByNames";
    public static string SignedByUids = "SignedByUids";


    public override void StartServerSide(ICoreServerAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);
        var original =
            AccessTools.Method(typeof(ModSystemEditableBook), "EndEdit");
        var patch = AccessTools.Method(typeof(EditableBookSystemPatch), nameof(EditableBookSystemPatch.Patch));

        harmony.Patch(original, new HarmonyMethod(patch));

        var originalTranscribe =
            AccessTools.Method(typeof(ModSystemEditableBook), "Transcribe");
        var transcribePatch = AccessTools.Method(typeof(EditableBookSystemPatch),
            nameof(EditableBookSystemPatch.Postfix));

        harmony.Patch(originalTranscribe, postfix: new HarmonyMethod(transcribePatch));
    }


    public override void StartClientSide(ICoreClientAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);

        var original =
            AccessTools.Method(typeof(ItemBook), "GetHeldItemInfo");
        var patch = AccessTools.Method(typeof(ItemBookPatch), nameof(ItemBookPatch.Patch));

        harmony.Patch(original, postfix: new HarmonyMethod(patch));

        var original2 =
            AccessTools.Method(typeof(GuiDialogReadonlyBook), "Compose");
        var patch2 = AccessTools.Method(typeof(GuiDialogReadonlyBookPatch), nameof(GuiDialogReadonlyBookPatch.Patch));

        harmony.Patch(original2, postfix: new HarmonyMethod(patch2));
    }
}