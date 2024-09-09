using System;
using System.Collections.Generic;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;
using System.Numerics;
using UnityEngine;

namespace SotSItemRebalance.Items
{
    public class SonorousWhispers
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        internal static int NeutralItemDropCount = 5;
        internal static float NeutralDropChance = 0.2f;
        public SonorousWhispers()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Sonorous Whispers");   //Start by displaying the message
            //ClampConfig();      //Nothing to clamp here
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating Sonorous Whispers Item Text");
            string pickup = "When a large monster is killed it will always drop an item. When a neutral creature is destroyed it will drop many items";
            string desc = string.Format("<style=cIsUtility>100%</style> when a non-teleporter boss is killed to drop an item. <style=cIsUtility>{0}%</style> <style=cStack>(+{0}% per stack)</style> for neutral creatures to drop <style=cIsUtility>{1}</style> <style=cStack>(+{1} per stack)</style> items", NeutralDropChance * 100, NeutralItemDropCount);
            LanguageAPI.Add("ITEM_RESETCHESTS_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_RESETCHESTS_DESC", desc + ".");
        }

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/ResetChests/ResetChests.asset").WaitForCompletion();
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
            Main.logSource.LogInfo("Applying IL modifications to Sonorous Whispers");
            IL.RoR2.GlobalEventManager.OnCharacterDeath += new ILContext.Manipulator(IL_OnCharacterDeath);
            SharedHooks.Handle_GlobalKillEvent_Actions += GetCharacterDeath;
        }

        private void GetCharacterDeath(DamageReport damageReport)
        {
            //logic should still be applied for bosses
            //and elite conditions removed, only applying neutral chances
            CharacterMaster attackerMaster = damageReport.attackerMaster;
            int whisperCount = attackerMaster.inventory.GetItemCount(DLC2Content.Items.ResetChests);
            int dropChance = (int)((NeutralDropChance * 100) * whisperCount);
            int clampedChance = (dropChance <= 0) ? 0 : (dropChance > 100) ? 100 : dropChance;
            bool isVictimNeutral = TeamComponent.GetObjectTeam(damageReport.victim.gameObject) == TeamIndex.Neutral;
            bool procRollPassed = Util.CheckRoll(clampedChance, attackerMaster.luck, attackerMaster);
            if (whisperCount > 0 && isVictimNeutral && procRollPassed)
            {
                generatePickupDroplet(damageReport, NeutralItemDropCount * whisperCount);
            }
        }

        private void IL_OnCharacterDeath(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            //Goes to the takeDamage method and zeroes out the reflected damage component
            if (ilcursor.TryGotoNext(
                x => x.MatchLdcR4(2f),
                x => x.MatchLdcR4(1f)
                ))
            {
                ilcursor.Next.Operand = 0f;
                ilcursor.Index += 1;
                ilcursor.Next.Operand = 0f;
            }
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": Sonorous Whispers IL Hook failed");
            }
        }

        private void generatePickupDroplet(DamageReport damageReport, int numItemsToDrop)
        {
            GameObject gameObject = null;
            CharacterBody victimBody = damageReport.victimBody;
            InputBankTest inputBankTest = victimBody.inputBank;
            UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
            UnityEngine.Quaternion quaternion = UnityEngine.Quaternion.identity;

            if ((bool)damageReport.victim)
            {
                gameObject = damageReport.victim.gameObject;
            }

            Transform transform = gameObject.transform;

            if ((bool)transform)
            {
                vector = transform.position;
                quaternion = transform.rotation;
            }
            
            Ray ray = (inputBankTest ? inputBankTest.GetAimRay() : new Ray(vector, quaternion * UnityEngine.Vector3.forward));
            for (int i = 0; i < numItemsToDrop; i++)
            {
                PickupDropletController.CreatePickupDroplet(GlobalEventManager.CommonAssets.dtSonorousEchoPath.GenerateDrop(Run.instance.runRNG), vector + UnityEngine.Vector3.up * 1.5f, UnityEngine.Vector3.up * 20f + ray.direction * 2f);
            }
        }

    }
}