using System;
using System.Collections.Generic;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace SotSItemRebalance.Items
{
    public class PrayerBeads
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        //Sets the max health % addition and attack damage % addition
        //Percent of max health to add
        internal static float StackHealth = 0.05f;
        //Increased attack damage percent
        internal static float StackAttackDamage = 0.015f;
        public PrayerBeads()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Bison Steak");   //Start by displaying the message
            //ClampConfig();      //Clamp the stats
            //UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            //Hooks();            //Hooks onto base game methods to replace them
        }

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/FlatHealth/FlatHealth.asset").WaitForCompletion();
            if (itemDef)
            {
                List<ItemTag> itemTags = itemDef.tags.ToList();
                //Enemies can have this item
                itemTags.Remove(ItemTag.AIBlacklist);
                //Mythrix can have this item
                itemTags.Remove(ItemTag.BrotherBlacklist);
                itemTags.Add(ItemTag.PriorityScrap);
                itemDef.tags = itemTags.ToArray();
            }
        }


    }
}