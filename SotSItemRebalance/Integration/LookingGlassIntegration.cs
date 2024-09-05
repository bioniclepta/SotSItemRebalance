using System;
using System.Runtime.CompilerServices;
using RoR2;
using SotSItemRebalance.Items;
using LookingGlass.ItemStatsNameSpace;
using System.Collections.Generic;

namespace SotSItemRebalance.Integration
{
    public static class LookingGlassIntegration
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(LookingGlass.PluginInfo.PLUGIN_GUID);
                }
                return (bool)_enabled;
            }
        }

        //Returns true if the Looking Glass mod by DropPod is installed, false if not
        public static bool LookingGlassInstalled()
        {
            return enabled;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SomeMethodThatRequireTheDependencyToBeHere()
        {
            // stuff that require the dependency to be loaded


        }

        internal static void init()
        {
            if(LookingGlassInstalled())
            {
                Main.logSource.LogInfo("Looking glass installed");
                try
                {
                    Main.logSource.LogInfo("Running code injection for LookingGlass");
                    LookingGlassStats.RegisterStats();
                }
                catch (Exception e)
                {
                    Main.logSource.LogError(e);
                }
            }
        }

        public static class LookingGlassStats
        {
            public static void RegisterStats()
            {
                //WHITE ITEMS

                //Antler Shield
                if (AntlerShield.Enable)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Shield Strength: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValuesNew = (luck, stackCount, procChance) =>
                    {
                        List<float> values = new();
                        values.Add(AntlerShield.StackShield * stackCount);
                        values.Add(AntlerShield.StackSpeed * stackCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RoR2Content.Items.PersonalShield.itemIndex, stats);
                }
                //Bison Steak

                //...
            }
        }
    }
}