using System;
using System.IO;
using RoR2;
using R2API;
using SotSItemRebalance.Items;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.SocialPlatforms;
using SotSItemRebalance;

namespace SotSItemRebalance
{
    //Ripped from FlatItemBuffs who ripped from RiskyMod
    public static class Configs
    {
        public static ConfigFile GeneralConfig;
        public static ConfigFile ItemConfig;
        public static ConfigFile ArtifactConfig;
        public static string ConfigFolderPath { get => System.IO.Path.Combine(BepInEx.Paths.ConfigPath, Main.pluginInfo.Metadata.GUID); }

        //Defines the item section labels
        private const string Section_AntlerShield_Buff = "Antler Shield";
        private const string Section_BisonSteak_Buff = "Bison Steak";
        private const string Section_WarpedEcho_Buff = "Warped Echo";

        //Defines the labels
        private const string Label_EnableBuff = "Enable Changes";
        private const string Label_EnableRework = "Enable Rework";

        private const string Desc_EnableBuff = "Enables changes for this item.";
        private const string Desc_EnableRework = "Enables the rework for this item. Has priority over the the normal changes.";

        private const string Section_General_Bugs = "Bug Fixes";
        private const string Section_General_Mechanics = "Mechanics";

        public static void Setup()
        {
            //Sets up an item config the config editor
            ItemConfig = new ConfigFile(System.IO.Path.Combine(ConfigFolderPath, $"Items.cfg"), true);
            //Common
            Read_AntlerShield();
            Read_BisonSteak();
            Read_WarpedEcho();
            //Uncommon

            //Legendary

            //Boss

            //Void

            //Artifacts

            //General

        }
        //Config binding time
        //This is where you set the default config settings and descriptions of them
        //Might behoove to make an all on or all off one
        private static void Read_AntlerShield()
        {
            AntlerShield.Enable = ItemConfig.Bind(Section_AntlerShield_Buff, Label_EnableBuff, true, Desc_EnableBuff).Value;
            AntlerShield.StackShield = ItemConfig.Bind(Section_AntlerShield_Buff, "Stack Shield", 0.05f, "Shield each stack gives.").Value;
            AntlerShield.StackSpeed = ItemConfig.Bind(Section_AntlerShield_Buff, "Stack Movement Speed", 0.07f, "Movement speed each stack gives.").Value;
        }

        private static void Read_BisonSteak()
        {
            BisonSteak.Enable = ItemConfig.Bind(Section_BisonSteak_Buff, Label_EnableBuff, true, Desc_EnableBuff).Value;
            BisonSteak.StackHealth = ItemConfig.Bind(Section_BisonSteak_Buff, "Stack Health", 0.05f, "Health each stack gives.").Value;
            BisonSteak.StackAttackDamage = ItemConfig.Bind(Section_BisonSteak_Buff, "Stack Attack Damage", 0.015f, "Attack damage each stack gives.").Value;
        }

        private static void Read_WarpedEcho()
        {
            WarpedEcho.Enable = ItemConfig.Bind(Section_WarpedEcho_Buff, Label_EnableBuff, true, Desc_EnableBuff).Value;
            WarpedEcho.procChance = ItemConfig.Bind(Section_WarpedEcho_Buff, "Proc Chance", 0.05f, "Proc chance each stack gives.").Value;
        }
    }
}