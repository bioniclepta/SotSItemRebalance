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
            else
            {
                Main.logSource.LogInfo("Looking glass not found, integrations not fired (If LookingGlass by DropPod was not installed or is disabled, this message can be ignored.)");
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
                    //Removes the Looking Glass Implementation
                    ItemDefinitions.allItemDefinitions.Remove((int)DLC2Content.Items.NegateAttack.itemIndex);
                    //Re-adds the definition with our new stat tracking
                    Main.logSource.LogInfo("Changing Looking Glass Implementation of: Antler Shield");
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
                    ItemDefinitions.allItemDefinitions.Add((int)DLC2Content.Items.NegateAttack.itemIndex, stats);
                }
                //Bison Steak
                if (BisonSteak.Enable)
                {
                    //Removes the Looking Glass Implementation
                    ItemDefinitions.allItemDefinitions.Remove((int)RoR2Content.Items.FlatHealth.itemIndex);
                    //Re-adds the definition with our new stat tracking
                    Main.logSource.LogInfo("Changing Looking Glass Implementation of: Bison Steak");
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Health: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.descriptions.Add("Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.calculateValuesNew = (luck, stackCount, procChance) =>
                    {
                        List<float> values = new();
                        values.Add(BisonSteak.StackHealth * stackCount);
                        values.Add(BisonSteak.StackAttackDamage * stackCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RoR2Content.Items.FlatHealth.itemIndex, stats);
                }
                //Warped Echo
                if (WarpedEcho.Enable)
                {
                    //Removes the Looking Glass Implementation
                    ItemDefinitions.allItemDefinitions.Remove((int)DLC2Content.Items.DelayedDamage.itemIndex);
                    //Re-adds the definition with our new stat tracking
                    Main.logSource.LogInfo("Changing Looking Glass Implementation of: Warped Echo");
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Proc Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValuesNew = (luck, stackCount, procChance) =>
                    {
                        List<float> values = new();
                        values.Add(WarpedEcho.procChance * stackCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)DLC2Content.Items.DelayedDamage.itemIndex, stats);
                }

                //Bolstering Lantern


                // GREEN ITEMS

                // RED ITEMS

                //Shattering Justice
                if (ShatteringJustice.Enable)
                {
                    //Removes the Looking Glass Implementation
                    ItemDefinitions.allItemDefinitions.Remove((int)RoR2Content.Items.ArmorReductionOnHit.itemIndex);
                    //Re-adds the definition with our new stat tracking
                    Main.logSource.LogInfo("Changing Looking Glass Implementation of: Shattering Justice");
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Max HP Reduction: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValuesNew = (luck, stackCount, procChance) =>
                    {
                        List<float> values = new();
                        values.Add(0.01f * stackCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RoR2Content.Items.ArmorReductionOnHit.itemIndex, stats);
                }

                //Sonorous Whispers
                if (SonorousWhispers.Enable)
                {
                    //Removes the Looking Glass Implementation
                    ItemDefinitions.allItemDefinitions.Remove((int)DLC2Content.Items.ResetChests.itemIndex);
                    //Re-adds the definition with our new stat tracking
                    Main.logSource.LogInfo("Changing Looking Glass Implementation of: Sonorous Whispers");
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Neutral Drop Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Neutral Items Dropped: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValuesNew = (luck, stackCount, procChance) =>
                    {
                        float dropChance = (SonorousWhispers.NeutralDropChance * 100) * stackCount;
                        float clampedChance = (dropChance <= 0f) ? 0f : (dropChance > 100f) ? 100f : dropChance;
                        List<float> values = new();
                        //Dividing by 100 again because it makes my brain happy when percents are out of 100 instead of 1
                        //LookingGlass multiplies by 100 when converting to % so we divide
                        values.Add(clampedChance / 100);
                        values.Add(SonorousWhispers.NeutralItemDropCount * stackCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)DLC2Content.Items.ResetChests.itemIndex, stats);
                }
                //...
            }
        }
    }
}