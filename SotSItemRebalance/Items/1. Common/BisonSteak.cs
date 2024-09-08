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
    public class BisonSteak
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        //Sets the max health % addition and attack damage % addition
        //Percent of max health to add
        internal static float StackHealth = 0.05f;
        //Increased attack damage percent
        internal static float StackAttackDamage = 0.015f;
        public BisonSteak()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Bison Steak");   //Start by displaying the message
            ClampConfig();      //Clamp the stats
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void ClampConfig()
        {
            //Just making sure numbers don't get messed up ig
            StackHealth = Math.Max(0f, StackHealth);
            StackAttackDamage = Math.Max(0f, StackAttackDamage);
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating Bison Steak Item Text");
            string pickup = "Slightly increase";
            string desc = "Increases";
            if (StackHealth > 0f)
            {
                pickup += " maximum health";
                desc += string.Format(" <style=cIsHealing>maximum health</style> by <style=cIsHealing>{0}%</style> <style=cStack>(+{0}% per stack)</style> of your maximum health", StackHealth * 100);
                if (StackAttackDamage > 0f)
                {
                    pickup += " and";
                    desc += " and";
                }
            }
            if (StackAttackDamage > 0f)
            {
                pickup += " attack damage";
                desc += string.Format(" attack damage by <style=cIsDamage>{0}%</style> <style=cStack>(+{0}% per stack)</style>", StackAttackDamage * 100f);
            }
            LanguageAPI.Add("ITEM_FLATHEALTH_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_FLATHEALTH_DESC", desc + ".");
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
                itemDef.tags = itemTags.ToArray();
            }
        }

        private void Hooks()
        {
            Main.logSource.LogInfo("Applying IL modifications to Bison Steak");
            IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
            SharedHooks.Handle_GetStatCoefficients_Actions += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args, Inventory inventory)
        {
            if (sender.inventory)
            {
                //Gets the number of steaks we have, then applies the buffs
                int steakCount = sender.inventory.GetItemCount(RoR2Content.Items.FlatHealth);
                //gets the players max health before adding more onto it
                float preMaxHealth = sender.maxHealth;
                if (steakCount > 0)
                {
                    //Adds to the max health % addition (this will scale on level up)
                    args.baseHealthAdd += steakCount * StackHealth * preMaxHealth;
                    //Increases attack damage by a small percent of your max HP
                    args.baseDamageAdd += steakCount * StackAttackDamage * preMaxHealth;

                }
            }
        }

        private void IL_RecalculateStats(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            //sets the IL cursor to where the game tries to add bison steak health increase to your max health and zeroes it out, using the new one above instead
            if (ilcursor.TryGotoNext(
                x => x.MatchLdloc(70),
                x => x.MatchLdloc(36),
                x => x.MatchConvR4()
                ))
            {
                ilcursor.Index += 3;
                //Since the game tries to do the following operation...
                //num61 += (float)num37 * 25f;
                //"Health += (float)numberOfBisonSteaks * 25f (< base health increase amount)"
                //We just skip over all of that, setting the 25f health addition to 0
                ilcursor.Next.Operand = 0f;
            }
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": Bison Steak IL Hook failed");
            }
        }

    }
}