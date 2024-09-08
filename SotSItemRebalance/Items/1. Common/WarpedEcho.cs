using System;
using System.Collections.Generic;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System.IO;
using RoR2.UI.LogBook;
using HG;
using static RoR2.DotController;

namespace SotSItemRebalance.Items
{
    public class WarpedEcho
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        //Sets the max health % addition and attack damage % addition
        //Chance for item to proc adding another debuff
        //previously 5%
        internal static float procChance = 0.05f;
        public WarpedEcho()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing Warped Echo");   //Start by displaying the message
            ClampConfig();      //Clamp the stats
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void ClampConfig()
        {
            //Just making sure numbers don't get messed up ig
            procChance = Math.Max(0f, procChance);
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating Warped Echo Item Text");
            string pickup = "Chance on hit to add another debuff to an enemy";
            string desc = string.Format("<style=cIsDamage>{0}%</style> <style=cStack>(+{0}% per stack)</style> on hit to add another stack of a debuff on an enemy", procChance * 100);
            LanguageAPI.Add("ITEM_DELAYEDDAMAGE_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_DELAYEDDAMAGE_DESC", desc + ".");
        }

        public static string AssetBundlePath => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.pluginInfo.Location), "assetbundles", "greenwarpedechobundle");
        public static AssetBundle greenWarpedEchoBundle;

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/DelayedDamage/DelayedDamage.asset").WaitForCompletion();
            itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>((object)"RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            //itemDef.tier = ItemTier.Tier2;
            greenWarpedEchoBundle = AssetBundle.LoadFromFile(AssetBundlePath);
            itemDef.pickupIconSprite = greenWarpedEchoBundle.LoadAsset<Sprite>("Assets/texDelayDamageIcon_nowwithGREEN.png");
            if ((UnityEngine.Object)(object)greenWarpedEchoBundle == (UnityEngine.Object)null)
            {
                Main.logSource.LogError("Asset Bundle not found for turning warped echo green.");
                Main.logSource.LogError("Did you check if you copied the assetbundle folder over?");
            }
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
            Main.logSource.LogInfo("Applying IL modifications to Warped Echo");
            //Zero out warped echo entierely (TODO)
            //IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
            SharedHooks.Handle_GlobalHitEvent_Actions += GetGlobalHitEvent;
        }

        private void GetGlobalHitEvent(CharacterBody victim, CharacterBody attacker, DamageInfo damageInfo)
        {
            int echoCount = attacker.inventory.GetItemCount(RoR2.DLC2Content.Items.DelayedDamage);
            //checks if the attacker has an echo on them and if the enemy has any stackable debuffs
            bool victimHasStackableDebuff = false;
            bool victimHasDOT = false;

            if (echoCount > 0)
            {
                DotController dotController = DotController.FindDotController(victim.gameObject);

                if ((bool)dotController)
                {
                    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                    {
                        if (dotController.HasDotActive(dotIndex))
                        {
                            victimHasDOT = true;
                            break;
                        }
                        //choose randomly to apply either dot or debuff
                        //check if there is dot AND debuff, otherwise choose the one available
                        //then add the chosen one
                    }
                }

                foreach (BuffIndex enemyBuff in victim.activeBuffsList)
                {
                    BuffDef enemyBuffDef = BuffCatalog.GetBuffDef(enemyBuff);
                    bool isBuffStackable = enemyBuffDef.canStack;
                    bool isDebuff = enemyBuffDef.isDebuff;

                    if (isBuffStackable && isDebuff)
                    {
                        victimHasStackableDebuff = true;
                        break;
                    }
                }

                if (victimHasStackableDebuff || victimHasDOT)
                {
                    bool echoProced = RoR2.Util.CheckRoll(procChance * echoCount, attacker.master.luck ,attacker.master);
                    //then checks if the echo proc'ed
                    if (echoProced)
                    {
                        //If the echo proc'ed, then add another (stackable) debuff to the victim
                        AddAnotherDebuffStack(victim, attacker, victimHasStackableDebuff, victimHasDOT);
                    }
                }
            }

        }

        private static void AddAnotherDebuffStack(CharacterBody victim, CharacterBody attacker, bool hasdebuff, bool hasDOT)
        {
            int numDebuffs = 0;
            int numDOTs = 0;
            List<BuffIndex> activeDebuffs = new List<BuffIndex>();

            if (hasdebuff)
            {
                BuffIndex[] debuffIndeces = RoR2.BuffCatalog.debuffBuffIndices;
                //Loops through all debuffs in the debuff table
                foreach (BuffIndex buffType in debuffIndeces)
                {
                    //then checks if the victim has that debuff and if the debuff can stack
                    if (victim.HasBuff(buffType) && BuffCatalog.GetBuffDef(buffType).canStack && BuffCatalog.GetBuffDef(buffType).isDebuff)
                    {
                        //adds the stackable debuff to the activeDebuffs list
                        activeDebuffs.Add(buffType);

                    }
                }
            }
            List<DotIndex> activeDOTs = new List<DotIndex>();
            if (hasDOT)
            {
                DotController dotController = DotController.FindDotController(victim.gameObject);

                if ((bool)dotController)
                {
                    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                    {
                        if (dotController.HasDotActive(dotIndex))
                        {
                            activeDOTs.Add(dotIndex);
                        }
                    }
                }
     
            }

            numDebuffs = activeDebuffs.ToArray().Length;
            numDOTs = activeDOTs.ToArray().Length;
            if(numDebuffs == 0)
            {
                int DOTToAdd = new System.Random().Next(0, numDOTs);
                InflictDot(victim.gameObject, attacker.gameObject, activeDOTs[DOTToAdd], GetDotDef(activeDOTs[DOTToAdd]).terminalTimedBuffDuration, GetDotDef(activeDOTs[DOTToAdd]).damageCoefficient);
            } 
            else if (numDOTs == 0)
            {
                int debuffToAdd = new System.Random().Next(0, numDebuffs);
                int buffCount = victim.GetBuffCount(activeDebuffs[debuffToAdd]);
                victim.SetBuffCount(activeDebuffs[debuffToAdd], buffCount + 1);
            } 
            else
            {
                int coinFlip = new System.Random().Next(0, 2);
                switch (coinFlip)
                {
                    case 0:
                        {
                            int DOTToAdd = new System.Random().Next(0, numDOTs);
                            InflictDot(victim.gameObject, attacker.gameObject, activeDOTs[DOTToAdd], UnityEngine.Random.Range(5, 60), UnityEngine.Random.Range(1, 3));
                            break;
                        }
                    case 1:
                        {
                            int debuffToAdd = new System.Random().Next(0, numDebuffs);
                            int buffCount = victim.GetBuffCount(activeDebuffs[debuffToAdd]);
                            victim.SetBuffCount(activeDebuffs[debuffToAdd], buffCount + 1);
                            break;
                        }
                }
            }
        }
    }
}