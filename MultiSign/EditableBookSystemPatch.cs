using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace MultiSign;

public static class EditableBookSystemPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ModSystemEditableBook), "EndEdit")]
    public static void Patch(
        ModSystemEditableBook __instance, IPlayer player, string text, string title, ref bool didSign)
    {
        var apiField = AccessTools.Field(typeof(ModSystemEditableBook), "api");
        var api = apiField.GetValue(__instance) as ICoreAPI;

        if (api is ICoreClientAPI)
        {
            return;
        }


        if (!didSign)
        {
            return;
        }


        var nowEditingField = AccessTools.Field(typeof(ModSystemEditableBook), "nowEditing");
        if (nowEditingField.GetValue(__instance) is not Dictionary<string, ItemSlot> nowEditing)
        {
            return;
        }

        if (nowEditing.TryGetValue(player.PlayerUID, out var itemSlot))
        {
            if (itemSlot.Itemstack.Attributes.GetString("signedbyuid") != null)
            {
                didSign = false;
            }

            var currentNames = itemSlot.Itemstack.Attributes.GetString(MultiSignModSystem.SignedByNames, string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            var currentUids = itemSlot.Itemstack.Attributes.GetString(MultiSignModSystem.SignedByUids, string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries);


            if (Enumerable.Contains(currentUids, player.PlayerUID))
            {
                return;
            }

            currentNames = currentNames.Append(player.PlayerName);
            currentUids = currentUids.AddToArray(player.PlayerUID);


            itemSlot.Itemstack.Attributes.SetString(MultiSignModSystem.SignedByNames, string.Join(',', currentNames));
            itemSlot.Itemstack.Attributes.SetString(MultiSignModSystem.SignedByUids, string.Join(',', currentUids));

            itemSlot.MarkDirty();
        }
    }

    [HarmonyPatch(typeof(ItemBook), nameof(ModSystemEditableBook.Transcribe))]
    public static void Postfix(
        IPlayer player,
        string pageText,
        string bookTitle,
        int pageNumber,
        ItemSlot bookSlot)
    {
        if (player is not IServerPlayer)
        {
            return;
        }

        ItemSlot newPageSlot = null;

        player.Entity.WalkInventory(slot =>
        {
            if (slot.Empty)
            {
                return true;
            }

            var atts = slot.Itemstack.Attributes;

            if (!atts.HasAttribute("text") || !atts.HasAttribute("pageNumber") || !atts.HasAttribute("title"))
            {
                return true;
            }


            if (atts.GetString("text") == pageText &&
                atts.GetInt("pageNumber") == pageNumber &&
                atts.GetString("title") == bookTitle && !atts.HasAttribute(MultiSignModSystem.SignedByNames) &&
                !atts.HasAttribute(MultiSignModSystem.SignedByUids))
            {
                newPageSlot = slot;
                return false;
            }

            return true;
        });

        if (newPageSlot == null)
        {
            return;
        }

        var src = bookSlot.Itemstack.Attributes;
        var dest = newPageSlot.Itemstack.Attributes;

        if (src.HasAttribute(MultiSignModSystem.SignedByNames))
        {
            dest.SetString(MultiSignModSystem.SignedByNames, src.GetString(MultiSignModSystem.SignedByNames));
        }

        if (src.HasAttribute(MultiSignModSystem.SignedByUids))
        {
            dest.SetString(MultiSignModSystem.SignedByUids, src.GetString(MultiSignModSystem.SignedByUids));
        }

        newPageSlot.MarkDirty();
    }
}