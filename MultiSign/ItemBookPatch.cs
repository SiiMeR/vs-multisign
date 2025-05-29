using System;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MultiSign;

public class ItemBookPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemBook), "GetHeldItemInfo")]
    public static void Patch(
        ItemBook __instance, ItemSlot inSlot,
        StringBuilder dsc,
        IWorldAccessor world,
        bool withDebugInfo)
    {
        var apiField = AccessTools.Field(typeof(ItemBook), "capi");
        var api = apiField.GetValue(__instance);

        if (api is not ICoreClientAPI capi)
        {
            return;
        }


        var signedByNames = inSlot.Itemstack.Attributes.GetString(MultiSignModSystem.SignedByNames, string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (signedByNames.Length == 0)
        {
            return;
        }

        dsc.Clear();

        if (withDebugInfo)
        {
            var stringBuilder = dsc;
            var index = __instance.Id;
            var str = $"<font color=\"#bbbbbb\">Id:{index.ToString()}</font>";
            stringBuilder.AppendLine(str);
            dsc.AppendLine($"<font color=\"#bbbbbb\">Code: {(string)__instance.Code}</font>");
            if (
                capi.Input.KeyboardKeyStateRaw[1])
            {
                dsc.AppendLine(
                    $"<font color=\"#bbbbbb\">Attributes: {inSlot.Itemstack.Attributes.ToJsonToken()}</font>\n");
            }
        }

        var transcriber = inSlot.Itemstack.Attributes.GetString("transcribedby");
        if (!string.IsNullOrEmpty(transcriber))
        {
            dsc.AppendLine(Lang.Get("Transcribed by {0}", transcriber));
        }

        if (!inSlot.Itemstack.Attributes.HasAttribute(MultiSignModSystem.SignedByUids))
        {
            return;
        }

        dsc.AppendLine(Lang.Get("Signed by:\n{0}", string.Join("\n", signedByNames)));
    }
}