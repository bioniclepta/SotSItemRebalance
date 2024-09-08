using System;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Security;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using MonoMod.ModInterop;
using SotSItemRebalance.Integration;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SotSItemRebalance
{
    //Mod information
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    //Hard Dependencies
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.bepis.r2api.recalculatestats", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.items", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]

    //Soft Dependencies
    [BepInDependency(LookingGlass.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    
    public class Main : BaseUnityPlugin
    {

        public const string MODUID = "dev.bioniclepta.sotsitemrebalance";
        public const string MODNAME = "SotSItemRebalance";
        public const string MODVERSION = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource logSource;
        public static PluginInfo pluginInfo;

        private void Awake()
        {
            logSource = this.Logger;
            pluginInfo = Info;
            Configs.Setup();
            EnableChanges();
            SharedHooks.Setup();
            GameModeCatalog.availability.CallWhenAvailable(new Action(PostLoad_GameModeCatalog));
        }

        private void EnableChanges()
        {
            //Common
            new Items.AntlerShield();
            new Items.BisonSteak();
            new Items.WarpedEcho();
        }

        private void PostLoad_GameModeCatalog()
        {
            //Items.DefenseNucleus_Shared.ExtraChanges();
           LookingGlassIntegration.init();
        }

    }
}