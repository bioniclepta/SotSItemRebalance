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
    public class ShatteringJustice
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        public ShatteringJustice()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Shattering Justice");   //Start by displaying the message
            //ClampConfig();      //Nothing to clamp here
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating Shattering Justice Item Text");
            string pickup = "Reduces targets maximum health on hit";
            string desc = string.Format("<style=cIsDamage>100%</style> chance on hit to reduce targets maximum health by <style=cIsDamage>{0}%</style> <style=cStack>(+{1}% per stack)</style> <style=cIsDamage>permanently</style>", 1.01 , 0.01);
            LanguageAPI.Add("ITEM_ARMORREDUCTIONONHIT_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_ARMORREDUCTIONONHIT_DESC", desc + ".");
        }

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/FlatHealth/ArmorReductionOnHit.asset").WaitForCompletion();
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
            Main.logSource.LogInfo("Applying IL modifications to Shattering Justice");
            IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_OnTakeDamage);
            SharedHooks.Handle_GlobalHitEvent_Actions += GetGlobalHitEvent;
        }

        private void GetGlobalHitEvent(CharacterBody victim, CharacterBody attacker, DamageInfo damageInfo)
        {
            //Probably the simplest item yet, on hit, apply permanent curse to the victim
            int justiceCount = attacker.inventory.GetItemCount(RoR2Content.Items.ArmorReductionOnHit);
            if (justiceCount > 0)
            {
                if(justiceCount > 1)
                {
                    for (int i = 0; i < justiceCount; i++)
                    {
                        victim.AddBuff(RoR2Content.Buffs.PermanentCurse);
                    }
                }
                else
                {
                    victim.AddBuff(RoR2Content.Buffs.PermanentCurse);
                }
            }
        }

        private void IL_OnTakeDamage(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            //Goes to the takeDamage method and zeroes out the reflected damage component
            if (ilcursor.TryGotoNext(
                x => x.MatchLdloc(0),
                x => x.MatchCallvirt<CharacterMaster>("get_inventory"),
                x => x.MatchLdsfld(typeof(RoR2Content.Items), "ArmorReductionOnHit"),
                x => x.MatchCallvirt<Inventory>("GetItemCount")
                ))
            {
                //removes the call to...
                //characterMaster.inventory.GetItemCount(RoR2Content.Items.ArmorReductionOnHit);
                //from...
                //int itemCount7 = characterMaster.inventory.GetItemCount(RoR2Content.Items.ArmorReductionOnHit);
                ilcursor.RemoveRange(4);
                //Instead replacing itemCount7 = 0
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            }
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": Shattering Justice IL Hook failed");
            }
        }

    }
}