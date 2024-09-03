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
    //NOTE: This is directly copied from FlatItemBuffs antler shield use as a template for all items
    //https://github.com/kking117/RoR2-kking117Mods/blob/master/FlatItemBuff/Items/Common/AntlerShield.cs
    //We should probably rework this to our own implementation a bit
    public class AntlerShield
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        //Sets the armor and speed of the new antler shields
        //Armor number is flat
        internal static float StackArmor = 7.5f;
        //Speed number is a percentage.
        internal static float StackSpeed = 0.07f;
        public AntlerShield()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Antler Shield");   //Start by displaying the message
            ClampConfig();      //Clamp the stats
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void ClampConfig()
        {
            //Just making sure numbers don't get messed up ig
            StackArmor = Math.Max(0f, StackArmor);
            StackSpeed = Math.Max(0f, StackSpeed);
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating Antler Shield Item Text");
            string pickup = "Slightly increase";
            string desc = "Increases";
            if (StackArmor > 0f)
            {
                pickup += " armor";
                desc += string.Format(" <style=cIsHealing>armor</style> by <style=cIsHealing>{0}</style> <style=cStack>(+{0} per stack)</style>", StackArmor);
                if(StackSpeed > 0f)
                {
                    pickup += " and";
                    desc += " and";
                }
            }
            if (StackSpeed > 0f)
            {
                pickup += " movement speed";
                desc += string.Format(" <style=cIsUtility>movement speed</style> by <style=cIsUtility>{0}%</style> <style=cStack>(+{0}% per stack)</style>", StackSpeed * 100f);
            }
            LanguageAPI.Add("ITEM_NEGATEATTACK_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_NEGATEATTACK_DESC", desc + ".");
        }

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/NegateAttack/NegateAttack.asset").WaitForCompletion();
            if (itemDef)
            {
                List<ItemTag> itemTags = itemDef.tags.ToList();
                //Enemies can have this item
                itemTags.Remove(ItemTag.AIBlacklist);
                //Mythrix can have this item
                itemTags.Remove(ItemTag.BrotherBlacklist);
                itemDef.tags = itemTags.ToArray();
            }
        }

        private void Hooks()
        {
            Main.logSource.LogInfo("Applying IL modifications to Antler Shield");
            IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_OnTakeDamage);
            //SharedHooks.Handle_GetStatCoefficients_Actions += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args, Inventory inventory)
        {
            if (sender.inventory)
            {
                //Gets the number of antlers we have, then applies the buffs
                int antlerCount = sender.inventory.GetItemCount(DLC2Content.Items.NegateAttack);
                if (antlerCount > 0)
                {
                    args.moveSpeedMultAdd += antlerCount * StackSpeed;
                    args.armorAdd += antlerCount * StackArmor;
                }
            }
        }

        private void IL_OnTakeDamage(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (ilcursor.TryGotoNext(x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), "antlerShield"))) {
                ilcursor.Index += 1;
                ilcursor.Emit(OpCodes.Ldc_I4_0);
                ilcursor.Emit(OpCodes.Mul);
            } 
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": Antler Shield IL Hook failed");
            }
        }

    }
}