using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace SotSItemRebalance
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {

        public const string MODUID = "dev.bioniclepta.sotsitemrebalance";
        public const string MODNAME = "SotSItemRebalance";
        public const string MODVERSION = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource logSource;
        public static PluginInfo pluginInfo;

        private void awake()
        {
            logSource = this.Logger;
            pluginInfo = Info;
            EnableChanges();
            SharedHooks.Setup();
            GameModeCatalog.availability.CallWhenAvailable(new Action(PostLoad_GameModeCatalog));
        }

        private void EnableChanges()
        {
            new Items.AntlerShield();
        }

    }
}