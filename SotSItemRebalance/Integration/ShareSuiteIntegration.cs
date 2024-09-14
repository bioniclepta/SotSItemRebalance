using System;
using System.Runtime.CompilerServices;
using RoR2;
using SotSItemRebalance.Items;
using System.Collections.Generic;

namespace SotSItemRebalance.Integration
{
    public static class ShareSuiteIntegration
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite");
                }
                return (bool)_enabled;
            }
        }

        //Returns true if the Looking Glass mod by DropPod is installed, false if not
        public static bool ShareSuiteInstalled()
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
            if (ShareSuiteInstalled())
            {
                Main.logSource.LogInfo("ShareSuite installed");
                try
                {
                    Main.logSource.LogInfo("Running code injection for ShareSuite");
                }
                catch (Exception e)
                {
                    Main.logSource.LogError(e);
                }
            }
            else
            {
                Main.logSource.LogInfo("ShareSuite not found, integrations not fired (If Sharesuite by FunkFrog-and-Sipondo was not installed or is disabled, this message can be ignored.)");
            }
        }
    }
}