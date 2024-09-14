using System;
using System.Collections.Generic;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SotSItemRebalance.Integration;
using UnityEngine;

namespace SotSItemRebalance.Items
{
    public class WarBonds
    {
        //Sets an enable flag. If this class is loaded, then it will be set to true
        internal static bool Enable = true;
        internal static bool dropItemsPerPlayer = false;

        internal int numItemsDropped = 0;
        public WarBonds()
        {
            //If the method got called and we didn't initialize, we have an issue
            if (!Enable) { return; }
            //Otherwise we can run the changes
            Main.logSource.LogInfo("Changing War Bonds");   //Start by displaying the message
            //ClampConfig();      //Nothing to clamp here
            UpdateText();       //Update the item description in game
            UpdateItemDef();    //Sets the item model, tags, and stuff
            Hooks();            //Hooks onto base game methods to replace them
        }

        private void UpdateText()
        {
            Main.logSource.LogInfo("Updating War Bonds Item Text");
            string pickup;
            string desc;
            if (BiggerBazaarIntegration.BiggerBazaarInstalled())
            {
                pickup = "Increases the stock at the Bazaar Between Time";
                desc = string.Format("The Bigger Bazzar has <style=cIsUtility>2</style> <style=cStack>(+1 per stack)</style> more item purchase available");
            }
            else
            {
                pickup = "Gain items upon traveling through a blue portal";
                desc = string.Format("Gain 1 item (60%/<style=cIsHealing>30%</style>/<style=cIsHealth>10%</style>)upon entering the Bazaar Between Time <style=cStack>(Increases rarity chances of the items per stack)</style>");
            }
            LanguageAPI.Add("ITEM_GOLDONSTAGESTART_PICKUP", pickup + ".");
            LanguageAPI.Add("ITEM_GOLDONSTAGESTART_DESC", desc + ".");
        }

        private void UpdateItemDef()
        {
            //Loads the original asset from base game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/GoldOnStageStart/GoldOnStageStart.asset").WaitForCompletion();
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
            Main.logSource.LogInfo("Applying IL modifications to War Bonds");
            IL.RoR2.GoldOnStageStartBehaviour.GiveWarBondsGold += new ILContext.Manipulator(IL_GiveWarBondsGold);
            //also zero that out
            IL.RoR2.CharacterBody.Start += new ILContext.Manipulator(IL_OnStart);
            //probably something like on stage enter
            SharedHooks.Handle_OnServerStageBegin_Actions += GetOnStageEnter;
        }

        private void GetOnStageEnter(CharacterMaster master, Stage stage)
        {
            bool isInBazaar = SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("bazaar");
            int numItemsToDrop = ShareSuiteIntegration.ShareSuiteInstalled() ? 1 : PlayerCharacterMasterController.instances.Count;

            //logic sheet
            //if BB enabled, add to item buy pool
            //if SS enabled, only drop one item
            //if SS not enabled...
                //if dropping items, drop items
                //else put item in player inv


            if(isInBazaar)
            {
                //generatePickupDroplet(master.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart));
                if (numItemsDropped < numItemsToDrop)
                {
                    if(BiggerBazaarIntegration.BiggerBazaarInstalled())
                    {
                        UnityEngine.Vector3 inFrontofNewt = Vector3.zero;
                        inFrontofNewt.Set(-121.3758f, -22.00205f, -39.35149f);

                        UnityEngine.Vector3 rotation = Vector3.zero;
                        inFrontofNewt.Set(0f, 36f, 0f);

                        SpawnCard spawnCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1");
                        DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule();
                        directorPlacementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
                        GameObject spawnedInstance = spawnCard.DoSpawn(inFrontofNewt, Quaternion.Euler(new Vector3(0f, 0f, 0f)), new DirectorSpawnRequest(spawnCard, directorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                        spawnedInstance.transform.eulerAngles = rotation;
                        PurchaseInteraction component2 = spawnedInstance.GetComponent<PurchaseInteraction>();
                        component2.Networkcost = 
                        numItemsDropped++;
                    }
                }
            } 
            else
            {
                numItemsDropped = 0;
            }
        }

        private void IL_GiveWarBondsGold(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            //Goes to the takeDamage method and zeroes out the reflected damage component
            if (ilcursor.TryGotoNext(
                x => x.MatchLdnull()
                ))
            {
                ilcursor.Index += 10;
                ilcursor.Next.Operand = 10;
            }
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": War Bonds IL Hook failed (GiveWarBondsGold)");
            }
        }

        private void IL_OnStart(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            //Goes to the takeDamage method and zeroes out the reflected damage component
            if (ilcursor.TryGotoNext(
                x => x.MatchMul(),
                x => x.MatchLdloc(0),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchAdd()
                ))
            {
                ilcursor.Index += 5;
                ilcursor.Emit(OpCodes.Ldc_R4, 0f);
                ilcursor.Emit(OpCodes.Mul);
            }
            else
            {
                UnityEngine.Debug.LogError(Main.MODNAME + ": War Bonds IL Hook failed (OnStart)");
            }
        }

        private void generatePickupDroplet(int numWarBonds)
        {
            float tier1Weight = 0.6f;
            float tier2Weight = 0.3f;
            float tier3Weight = 0.1f;

            Main.logSource.LogInfo("Entered droplet");

            UnityEngine.Vector3 onTopofBarrels = Vector3.zero;
            onTopofBarrels.Set(-125.9141f, -22.22021f, -26.30829f);
            
            UnityEngine.Vector3 inFrontofBarrels = Vector3.zero;
            inFrontofBarrels.Set(-119.2826f, -24.98082f, -16.23789f);

            UnityEngine.Vector3 inFrontofNewt = Vector3.zero;
            inFrontofNewt.Set(-121.3758f, -22.00205f, -39.35149f);

            UnityEngine.Vector3 BB1 = Vector3.zero;
            BB1.Set(-101.109f, -20.61698f, -57.5067f);

            UnityEngine.Vector3 BB2 = Vector3.zero;
            BB2.Set(-96.86827f, -20.57293f, -60.53222f);

            UnityEngine.Vector3 BB3 = Vector3.zero;
            BB2.Set(-92.75403f, -20.53781f, -63.39692f);

            UnityEngine.Vector3 BB4 = Vector3.zero;
            BB4.Set(-88.34589f, -20.49241f, -66.53828f);

            UnityEngine.Vector3 BB5 = Vector3.zero;
            BB4.Set(-84.04456f, -20.42975f, -69.7736f);

            UnityEngine.Vector3 BB6 = Vector3.zero;
            BB4.Set(-80.09068f, -20.33688f, -73.0743f);

            UnityEngine.Vector3 overflow = Vector3.zero;
            BB4.Set(-121f, -20f, -12f);

            WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>();
            selector.Clear();

            float weight = tier1Weight / (float)Run.instance.availableTier1DropList.Count;

            foreach (PickupIndex sourceDrop in Run.instance.availableTier1DropList)
            {
                selector.AddChoice(sourceDrop, weight);
            }

            weight = tier2Weight / (float)Run.instance.availableTier2DropList.Count;

            foreach (PickupIndex sourceDrop in Run.instance.availableTier2DropList)
            {
                selector.AddChoice(sourceDrop, weight * (float)numWarBonds);
            }

            weight = tier3Weight / (float)Run.instance.availableTier3DropList.Count;

            foreach (PickupIndex sourceDrop in Run.instance.availableTier3DropList)
            {
                selector.AddChoice(sourceDrop, weight * (float)Math.Pow(numWarBonds, 2f));
            }

            Xoroshiro128Plus rng = Run.instance.runRNG;
            PickupIndex pickupIndex = PickupDropTable.GenerateDropFromWeightedSelection(rng, selector);
            
            PickupDropletController.CreatePickupDroplet(pickupIndex, onTopofBarrels + UnityEngine.Vector3.up * 1.5f, UnityEngine.Vector3.up * 20f);
            numItemsDropped += 1;

        }

    }
}