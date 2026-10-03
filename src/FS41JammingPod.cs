using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin("ghora.fs41.jammingpod", "FS-41 Native Jamming Pod", "1.0.0")]
public class FS41JammingPod : BaseUnityPlugin
{
    internal static ManualLogSource Log;

    private void Awake()
    {
        Log = Logger;

        var harmony = new Harmony("ghora.fs41.jammingpod");
        harmony.PatchAll();

        Log.LogInfo("FS-41 Native Jamming Pod loaded.");
    }
}

[HarmonyPatch(typeof(Blueprinter.Ops.OpAddWeaponToHardpointHandler), "Handle")]
static class FS41JammingPodPatch
{
    static void Postfix()
    {
        try
        {
            if (Encyclopedia.WeaponLookup == null)
            {
                FS41JammingPod.Log.LogWarning("WeaponLookup is null.");
                return;
            }

            if (!Encyclopedia.WeaponLookup.TryGetValue("JammingPod1", out var jammer))
            {
                FS41JammingPod.Log.LogWarning("JammingPod1 not found.");
                return;
            }

            if (Encyclopedia.Lookup == null)
            {
                FS41JammingPod.Log.LogWarning("Encyclopedia.Lookup is null.");
                return;
            }

            if (!Encyclopedia.Lookup.TryGetValue("Aryx_Interceptor1", out var definition))
            {
                FS41JammingPod.Log.LogWarning("Aryx_Interceptor1 not found yet.");
                return;
            }

            var aircraft = definition as AircraftDefinition;

            if (aircraft == null || aircraft.unitPrefab == null)
            {
                FS41JammingPod.Log.LogWarning("FS-41 AircraftDefinition/prefab not found.");
                return;
            }

            var weaponManager = aircraft.unitPrefab.GetComponentInChildren<WeaponManager>(true);

            if (weaponManager == null || weaponManager.hardpointSets == null)
            {
                FS41JammingPod.Log.LogWarning("FS-41 WeaponManager not found.");
                return;
            }

            int[] sets =
            {
                1,
                5,
                6
            };

            foreach (int index in sets)
            {
                if (index < 0 || index >= weaponManager.hardpointSets.Length)
                    continue;

                var set = weaponManager.hardpointSets[index];

                if (set == null)
                    continue;

                if (set.weaponOptions == null)
                    set.weaponOptions = new List<WeaponMount>();

                if (!set.weaponOptions.Contains(jammer))
                {
                    set.weaponOptions.Add(jammer);

                    FS41JammingPod.Log.LogInfo(
                        $"Added Radar Jamming Pod to FS-41 hardpoint set {index}: {set.name}");
                }
            }
        }
        catch (Exception ex)
        {
            FS41JammingPod.Log.LogError(ex);
        }
    }
}
