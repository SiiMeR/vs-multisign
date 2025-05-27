using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MultiSign;

public class GuiDialogReadonlyBookPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GuiDialogReadonlyBook), "Compose")]
    public static void Patch(
        GuiDialogReadonlyBook __instance)
    {
        var apiField = AccessTools.Field(typeof(GuiDialogReadonlyBook), "capi");
        var api = apiField.GetValue(__instance);

        if (api is not ICoreClientAPI capi)
        {
            return;
        }

        var font = CairoFont.TextInput().WithFontSize(18f);
        var titleBounds = ElementBounds.Fixed(0, 30, 400, 24);

        var lineHeight = font.GetFontExtents().Height * font.LineHeightMultiplier / RuntimeEnv.GUIScale;
        var textAreaBounds = ElementBounds.Fixed(0, 0, 400, 20 * lineHeight + 1)
            .FixedUnder(titleBounds, 5);

        var pageLabelBounds = ElementBounds.FixedSize(80, 30).FixedUnder(textAreaBounds, 18 + 2 * 5 + 5)
            .WithAlignment(EnumDialogArea.CenterFixed).WithFixedPadding(10, 2);

        var signButtonBounds = ElementBounds.FixedSize(0, 0).FixedUnder(pageLabelBounds, 5)
            .WithAlignment(EnumDialogArea.CenterFixed).WithFixedPadding(12, 2).WithFixedOffset(-11, 0);

        if (__instance.SingleComposer != null)
        {
            __instance.SingleComposer.Composed = false;
            __instance.SingleComposer.AddSmallButton(Lang.Get("editablebook-sign"),
                () => OnButtonSign(__instance, capi),
                signButtonBounds).ReCompose();
        }
    }

    private static bool OnButtonSign(GuiDialogReadonlyBook instance, ICoreClientAPI capi)
    {
        new GuiDialogConfirm(capi, Lang.Get("Save and sign book now? It can not be edited afterwards."),
                ok => OnConfirmSign(capi, instance, ok))
            .TryOpen();
        return true;
    }

    private static void OnConfirmSign(ICoreClientAPI capi, GuiDialogReadonlyBook instance, bool ok)
    {
        if (ok)
        {
            var bookmodSys = capi.ModLoader.GetModSystem<ModSystemEditableBook>();
            bookmodSys.EndEdit(capi.World.Player, instance.AllPagesText, instance.Title, true);
            instance.TryClose();
        }
    }
}