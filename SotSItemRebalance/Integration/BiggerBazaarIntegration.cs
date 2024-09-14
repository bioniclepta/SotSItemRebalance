using System;
using System.Runtime.CompilerServices;
using RoR2;
using SotSItemRebalance.Items;
using BiggerBazaar;
using System.Collections.Generic;

namespace SotSItemRebalance.Integration
{
    public static class BiggerBazaarIntegration
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.MagnusMagnuson.BiggerBazaar");
                }
                return (bool)_enabled;
            }
        }

        //Returns true if the Looking Glass mod by DropPod is installed, false if not
        public static bool BiggerBazaarInstalled()
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
            if (BiggerBazaarInstalled())
            {
                Main.logSource.LogInfo("BiggerBazaar installed");
                try
                {
                    Main.logSource.LogInfo("Running code injection for BiggerBazaar");
                }
                catch (Exception e)
                {
                    Main.logSource.LogError(e);
                }
            }
            else
            {
                Main.logSource.LogInfo("BiggerBazaar not found, integrations not fired (If BiggerBazaar by MagnusMagnuson was not installed or is disabled, this message can be ignored.)");
            }
        }
    }
}