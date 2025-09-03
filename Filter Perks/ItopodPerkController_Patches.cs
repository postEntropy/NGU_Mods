using System;
using HarmonyLib;

[HarmonyPatch(typeof(ItopodPerkController))]
public static class ItopodPerkController_Patches
{
    [HarmonyPostfix]

    [HarmonyPatch("changePage", new Type[] { typeof(int) })] 
    [HarmonyPatch("onOrderChange")]                          
    public static void RefreshSearch_Postfix()
    {
        if (SearchITOPODPerks.Instance != null && SearchITOPODPerks.Instance._visible)
        {
            SearchITOPODPerks.Instance.RefreshPerks();
        }
    }
}
