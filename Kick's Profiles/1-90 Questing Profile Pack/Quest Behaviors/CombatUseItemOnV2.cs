// Behavior originally contributed by Chinajade
//
// LICENSE:
// This work is licensed under the
//     Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// also known as CC-BY-NC-SA.  To view a copy of this license, visit
//      http://creativecommons.org/licenses/by-nc-sa/3.0/
// or send a letter to
//      Creative Commons // 171 Second Street, Suite 300 // San Francisco, California, 94105, USA.

#region Summary and Documentation
//
// QUICK DOX:
// COMBATUSEITEMON uses an item on a target while the toon is in combat.
// The caller can determine at what point in combat the item will be used:
//  * When the target's health drops below a certain percentage
//  * When the target is casting a particular spell
//  * When the target gains a particular aura
//  * When the target loses a particular aura
//  * When the toon gains a particular aura
//  * When the toon loses a particular aura
//  * one or more of the above happens (the conditions are OR'd together)
//
// BEHAVIOR ATTRIBUTES:
// Basic Attributes:
//      ItemId [REQUIRED]
//          This is the Id of the Item that should be used on the mobs.
//          The item must exist in the backpack; otherwise, Honorbuddy will be stopped.
//      ItemAppliesAuraId [REQUIRED, if UseItemStrategy is UseItemOncePerTarget or UseItemOncePerTargetDontDefend,
//          or if a QuestId is not provided]
//          [allowed values: AssumeItemUseAlwaysSucceeds, or an aura id]
//          This is the aura that the item applies to the target once the item is used.  This is our only
//          means of telling if the Item use was successful.  (If no aura was applied, we must have been
//          interrupted in trying to use it, and need to keep trying).
//          If ItemAppliesAuraId="AssumeItemUseAlwaysSucceeds" should *only* be specified if there is no
//          distinguishable aura that is applied to the target when the item is used.  If this attribute,
//          is specified, there is no opportunity to figure out if the item use succeeded or not, and
//          could result in premature termination of the behavior.
//      MobId1, MobId2, ... MobIdN [at least one MobId is REQUIRED]
//          Identifies the mobs on which the item should be used.
//      NumOfTimesToUseItem [REQUIRED if a QuestId is not provided; otherwise IGNORED]
//          The behavior considers itself complete when the item has been used this number
//          of times.
//          If the behavior is also associated with a quest or quest objective, then the behavior
//          will also terminate when the quest or objective completes.  This may happen before
//          the NumOfTimesToUseItem has been consumed.
//      UseWhenMeHasAuraId [at least one of the UseWhen* attributes is REQUIRED; Default: none]
//          When the toon acquires the identified AuraId, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      UseWhenMeMissingAuraId [at least one of the UseWhen* attributes is REQUIRED; Default: none]
//          When the toon loses the identified AuraId, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      UseWhenMobCastingSpellId [at least one of the UseWhen* attributes is REQUIRED; Default: none]
//          When the mob is casting the identified SpellId, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      UseWhenMobHasAuraId [at least one of the UseWhen* attributes is REQUIRED; Default: none]
//          When the mob acquires the identified AuraId, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      UseWhenMobMissingAuraId [at least one of the UseWhen* attributes is REQUIRED; Default: none]
//          When the mob loses the identified AuraId, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      UseWhenMobHasHealthPercent [at least one of the UseWhen* attributes is REQUIRED; Default: 0]
//          When the mob's health drops to the identified percentage, the item is used on the mob.
//          If multiple UseWhen* attributes are provided, the item is used when _any_
//          of the UseWhen* conditions go true.
//      X/Y/Z [REQUIRED, if <HuntingGrounds> sub-element is omitted; Default: none]
//          This specifies the location of a 'safe spot' where the toon should loiter
//          while waiting for the AvoidMobId to clear the area.
//
// Quest binding:
//      QuestId [optional; Default:none]:
//      QuestCompleteRequirement [Default:NotComplete]:
//      QuestInLogRequirement [Default:InLog]:
//          A full discussion of how the Quest* attributes operate is described in
//          http://www.thebuddyforum.com/mediawiki/index.php?title=Honorbuddy_Programming_Cookbook:_QuestId_for_Custom_Behaviors
//
// Tunables:
//      CollectionDistance [optional; Default: 100.0]
//          Measured from the toon's current location, this value specifies
//          the maximum distance that should be searched when looking for
//          a viable MobId with which to interact.
//      IgnoreMobsInBlackspots [optional; Default: true]
//          When true, any mobs within (or too near) a blackspot will be ignored
//          in the list of viable targets that are considered for item use.
//      MaxRangeToUseItem [optional; Default: 25.0]
//          Defines the maximum range at which the item can be used on a mob.
//          If the toon is out of range (i.e., due to a ranged pull), the toon
//          will be moved within this distance of the mob to assure the item can be used
//          immediately when the appropriate conditions are met.
//      NonCompeteDistance [optional; Default: 15.0]
//          If any player is within NonCompeteDistance of the mob of interest,
//          the mob will be discarded as a viable target.
//      RecallPetAtMobPercentHealth [optional: Default: provided by UseWhenMobHasHealthPercent]
//          Pet playing classes may need to recall their pet prematurely to prevent
//          killing the mob before the item is used.  This attribute allows for early
//          pet recall.
//      UseItemStrategy [optional; Default: UseItemOncePerTarget]
//          [Allowed values: UseItemOncePerTarget, UseItemOncePerTargetDontDefend,
//              UseItemContinuouslyOnTarget, UseItemContinuouslyOnTargetDontDefend]
//          Defines how the item is to be used on the mob:
//              UseItemOncePerTarget
//                  Uses the item on the mob.  If the mob acquires the ItemAppliesAuraId
//                  then the use is considered successful; otherwise, we keep trying
//                  to use the item on the mob.
//                  If the mob continues to attack us after a successful use of the item,
//                  we will defend ourselves to the point of killing the mob, if necessary.
//              UseItemOncePerTargetDontDefend
//                  Uses the item on the mob.  If the mob acquires the ItemAppliesAuraId
//                  then the use is considered successful; otherwise, we keep trying
//                  to use the item on the mob.
//                  If the mob continues to attack us after a successful use of the item,
//                  we will not defend ourself and move on to the next mob.
//              UseItemContinuouslyOnTarget
//                  Uses the item on the same mob everytime the item is not on cooldown.
//                  If the mob continues to attack us after a successful use of the item,
//                  we will defend ourselves to the point of killing the mob, if necessary.
//              UseItemContinuouslyOnTargetDontDefend
//                  Uses the item on the same mob everytime the item is not on cooldown.
//                  If the mob continues to attack us after a successful use of the item,
//                  we will not defend ourself to the point where the mob gives up, or
//                  we die.
//      WaitTimeAfterItemUse [optional; Default 0ms]
//          Defines the number of milliseconds to wait after the item is successfully
//          used before carrying on with the behavior on other mobs.
//
// BEHAVIOR EXTENSION ELEMENTS (goes between <CustomBehavior ...> and </CustomBehavior> tags)
// See the "Examples" section for typical usage.
//      HuntingGrounds [REQUIRED, if X/Y/Z omitted in main attributes]
//          The HuntingGrounds contains a set of Waypoints we will visit to seek mobs
//          that fulfill the quest goal.  The <HuntingGrounds> element accepts the following
//          attributes:
//              WaypointVisitStrategy= [optional; Default: Random]
//              [Allowed values: InOrder, Random]
//              Determines the strategy that should be employed to visit each waypoint.
//              Any mobs encountered while traveling between waypoints will be considered
//              viable.  The Random strategy is highly recommended unless there is a compelling
//              reason to otherwise.  The Random strategy 'spread the toons out', if
//              multiple bos are running the same quest.
//          Each Waypoint is provided by a <Hotspot ... /> element with the following
//          attributes:
//              Name [optional; Default: ""]
//                  The name of the waypoint is presented to the user as it is visited.
//                  This can be useful for debugging purposes, and for making minor adjustments
//                  (you know which waypoint to be fiddling with).
//              X/Y/Z [REQUIRED; Default: none]
//                  The world coordinates of the waypoint.
//              Radius [optional; Default: 7.0]
//                  Once the toon gets within Radius of the waypoint, the next waypoint
//                  will be sought.
//
#endregion


#region FAQs
#endregion


#region Examples
// "Camel Tow" (http://wowhead.com/quest=28352)
// Beat camels into submission (25% health), then use the Sullah's Camel Hareness (http://wowhead.com/item=67241)
// on them.  The quest is complete after doing this three times.
//      <CustomBehavior File="CombatUseItemOn" QuestId="28352" ItemId="67241" ItemAppliesAuraId="94956" MobId="51193"
//          UseWhenMobHasHealthPercent="25" MaxRangeToUseItem="15" UseItemStrategy="UseItemOncePerTargetDontDefend"
//          RecallPetAtMobPercentHealth="50" >
//          <HuntingGrounds WaypointVisitStrategy="Random" >
//              <Hotspot Name="Sullah's Sideshow" X="-8986.617" Y="677.9194" Z="177.0783" />
//              <Hotspot Name="NW of Temple of Uldum" X="-9223.614" Y="666.8814" Z="188.2858" />
//              <Hotspot Name="W of Temple of Uldum" X="-9401.372" Y="686.7933" Z="185.5984" />
//              <Hotspot Name="SW of Temple of Uldum" X="-9523.359" Y="618.4017" Z="137.2736" />
//              <Hotspot Name="S of Temple of Uldum (upper ridge)" X="-9589.536" Y="399.2485" Z="132.335" />
//              <Hotspot Name="SW of Temple of Uldum (lower ridge)" X="-9743.044" Y="579.4816" Z="75.00929" />
//          </HuntingGrounds>
//      </CustomBehavior>
//
// "Do the Imp-Possible" (http://wowhead.com/quest=28000)
// Fight Impsy.  When he is 15% health, use the Enchanted Imp Sack (http://wowhead.com/item=62899) to capture him.
//      <CustomBehavior File="CombatUseItemOn" QuestId="28000" ItemId="62899" MobId="47339"
//          ItemAppliesAuraId="88330" UseWhenMobHasHealthPercent="15" MaxRangeToUseItem="15"
//          UseItemStrategy="UseItemOncePerTargetDontDefend"
//          X="4287.259" Y="-1112.751" Z="323.6652" />
//
// "Unlimited Potential" (http://wowhead.com/quest=28351)
// Beat pygmies into submission (25% health), then use Sullah's Pygmy Pen (http://wowhead.com/item=67232)
// on them to capture.  The quest is complete after doing this five times.
//      <CustomBehavior File="CombatUseItemOn" QuestId="28351" ItemId="67232" ItemAppliesAuraId="94365" MobId="51217"
//          UseWhenMobHasHealthPercent="25" MaxRangeToUseItem="10" UseItemStrategy="UseItemOncePerTargetDontDefend" 
//          RecallPetAtMobPercentHealth="40" >
//          <HuntingGrounds WaypointVisitStrategy="Random" >
//              <Hotspot Name="NW of Temple of Uldum" X="-9223.614" Y="666.8814" Z="188.2858" />
//              <Hotspot Name="W of Temple of Uldum" X="-9401.372" Y="686.7933" Z="185.5984" />
//              <Hotspot Name="SW of Temple of Uldum" X="-9523.359" Y="618.4017" Z="137.2736" />
//              <Hotspot Name="S of Temple of Uldum (upper ridge)" X="-9589.536" Y="399.2485" Z="132.335" />
//              <Hotspot Name="SW of Temple of Uldum (lower ridge)" X="-9743.044" Y="579.4816" Z="75.00929" />
//          </HuntingGrounds>
//      </CustomBehavior>
//
#endregion


#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Bots.Grind;
using CommonBehaviors.Actions;
using Honorbuddy.QuestBehaviorCore;
using Honorbuddy.QuestBehaviorCore.XmlElements;
using JetBrains.Annotations;
using Styx;
using Styx.Common.Helpers;
using Styx.CommonBot;
using Styx.CommonBot.POI;
using Styx.CommonBot.Profiles;
using Styx.Helpers;
using Styx.Pathing;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using Action = Styx.TreeSharp.Action;
#endregion


namespace Honorbuddy.Quest_Behaviors.CombatUseItemOnV2
{
    [CustomBehaviorFileName(@"CombatUseItemOnV2")]
    public class CombatUseItemOnV2 : QuestBehaviorBase
    {
        #region Consructor and Argument Processing

        public enum UseItemStrategyType
        {
            UseItemOncePerTarget,
            UseItemOncePerTargetDontDefend,
            [UsedImplicitly] UseItemContinuouslyOnTarget,
            UseItemContinuouslyOnTargetDontDefend,
        }

        public CombatUseItemOnV2(Dictionary<string, string> args)
            : base(args)
        {
            try
            {
                // NB: Core attributes are parsed by QuestBehaviorBase parent (e.g., QuestId, NonCompeteDistance, etc)

                // Primary attributes...
                ItemId = GetAttributeAsNullable<int>("ItemId", true, ConstrainAs.ItemId, null) ?? 0;

                string itemUseAlwaysSucceeds = GetAttributeAs<string>("ItemAppliesAuraId", false, ConstrainAs.StringNonEmpty, null) ?? string.Empty;
                if (itemUseAlwaysSucceeds == "AssumeItemUseAlwaysSucceeds")
                {
                    ItemUseAlwaysSucceeds = true;
                    ItemAppliesAuraId = 0;
                }
                else
                    { ItemAppliesAuraId = GetAttributeAsNullable<int>("ItemAppliesAuraId", false, ConstrainAs.AuraId, null) ?? 0; } 

                MobIds = GetNumberedAttributesAsArray<int>("MobId", 1, ConstrainAs.MobId, null);
                UseWhenMeHasAuraId = GetAttributeAsNullable<int>("UseWhenMeHasAuraId", false, ConstrainAs.AuraId, null) ?? 0;
                UseWhenMeMissingAuraId = GetAttributeAsNullable<int>("UseWhenMeMissingAuraId", false, ConstrainAs.AuraId, null) ?? 0;
                UseWhenMobCastingSpellId = GetAttributeAsNullable<int>("UseWhenMobCastingSpellId", false, ConstrainAs.SpellId, null) ?? 0;
                UseWhenMobHasAuraId = GetAttributeAsNullable<int>("UseWhenMobHasAuraId", false, ConstrainAs.AuraId, null) ?? 0;
                UseWhenMobMissingAuraId = GetAttributeAsNullable<int>("UseWhenMobMissingAuraId", false, ConstrainAs.AuraId, null) ?? 0;
                UseWhenMobHasHealthPercent = GetAttributeAsNullable<double>("UseWhenMobHasHealthPercent", false, ConstrainAs.Percent, null) ?? 0;

                // Either HuntingGroundCenter or <HuntingGrounds> subelement must be provided...
                // The sanity check for this is done in OnStart() since that's where we must do
                // all sub-element processing due to the way CustomForcedBehavior is architected.
                HuntingGroundCenter = GetAttributeAsNullable<WoWPoint>("", false, ConstrainAs.WoWPointNonEmpty, null);

                // Tunables...
                CollectionDistance = GetAttributeAsNullable<double>("CollectionDistance", false, ConstrainAs.Range, null) ?? 100;
                MaxRangeToUseItem = GetAttributeAsNullable<double>("MaxRangeToUseItem", false, ConstrainAs.Range, null) ?? 25.0;
                NumOfTimesToUseItem = GetAttributeAsNullable<int>("NumOfTimesToUseItem", false, ConstrainAs.RepeatCount, null) ?? 1;
                RecallPetAtMobPercentHealth = GetAttributeAsNullable<double>("RecallPetAtMobPercentHealth", false, ConstrainAs.Percent, null) ?? UseWhenMobHasHealthPercent;
                UseItemStrategy = GetAttributeAsNullable<UseItemStrategyType>("UseItemStrategy", false, null, null) ?? UseItemStrategyType.UseItemOncePerTarget;
                WaitTimeAfterItemUse = GetAttributeAsNullable<int>("WaitTimeAfterItemUse", false, ConstrainAs.Milliseconds, null) ?? 0;
            }

            catch (Exception except)
            {
                // Maintenance problems occur for a number of reasons.  The primary two are...
                // * Changes were made to the behavior, and boundary conditions weren't properly tested.
                // * The Honorbuddy core was changed, and the behavior wasn't adjusted for the new changes.
                // In any case, we pinpoint the source of the problem area here, and hopefully it can be quickly
                // resolved.
                LogError("[MAINTENANCE PROBLEM]: " + except.Message
                        + "\nFROM HERE:\n"
                        + except.StackTrace + "\n");
                IsAttributeProblem = true;
            }
        }


        // Variables for Attributes provided by caller
        private double CollectionDistance { get; set; }
        private WoWPoint? HuntingGroundCenter { get; set; }
        private int ItemId { get; set; }
        private int ItemAppliesAuraId { get; set; }
        private bool ItemUseAlwaysSucceeds { get; set; }
        private double MaxRangeToUseItem { get; set; }
        private int[] MobIds { get; set; }
        private int NumOfTimesToUseItem { get; set; }
        private double RecallPetAtMobPercentHealth { get; set; }
        private UseItemStrategyType UseItemStrategy { get; set; }
        private int UseWhenMobCastingSpellId { get; set; }
        private int UseWhenMeMissingAuraId { get; set; }
        private int UseWhenMeHasAuraId { get; set; }
        private int UseWhenMobMissingAuraId { get; set; }
        private int UseWhenMobHasAuraId { get; set; }
        private double UseWhenMobHasHealthPercent { get; set; }
        private int WaitTimeAfterItemUse { get; set; }

        // DON'T EDIT THESE--they are auto-populated by Subversion
        public override string SubversionId { get { return ("$Id: CombatUseItemOnV2.cs 539 2013-05-31 05:11:02Z chinajade $"); } }
        public override string SubversionRevision { get { return ("$Revision: 539 $"); } }


        protected override void EvaluateUsage_DeprecatedAttributes(XElement xElement)
        {
            // empty, for now
        }


        protected override void EvaluateUsage_SemanticCoherency(XElement xElement)
        {
            UsageCheck_SemanticCoherency(xElement,
                ((UseWhenMeHasAuraId <= 0)
                    && (UseWhenMeMissingAuraId <= 0)
                    && (UseWhenMobCastingSpellId <= 0)
                    && (UseWhenMobHasAuraId <= 0)
                    && (UseWhenMobMissingAuraId <= 0)
                    && (UseWhenMobHasHealthPercent <= 0)),
                context => "One or more of the following attributes must be specified:\n"
                            + "UseWhenMeHasAuraId, UseWhenMeMissingAuraId, UseWhenMobCastingSpellId,"
                            + " UseWhenMobHasAuraId, UseWhenMobMissingAuraId, UseWhenMobHasHealthPercent");

            UsageCheck_SemanticCoherency(xElement,
                ((ItemAppliesAuraId <= 0)
                    && (!ItemUseAlwaysSucceeds)
                    && ((UseItemStrategy == UseItemStrategyType.UseItemOncePerTarget)
                        || (UseItemStrategy == UseItemStrategyType.UseItemOncePerTargetDontDefend)
                        || (QuestId <= 0))),
                context => string.Format("For a UseItemStrategy of {0}, ItemAppliesAuraId must be specified",
                                        UseItemStrategy));
        }
        #endregion


        #region Private and Convenience variables
        private int Counter { get; set; }
        private HuntingGroundsType HuntingGrounds { get; set; }
        private WoWItem ItemToUse { get; set; }
        private WoWUnit SelectedTarget { get; set; }
        
        private readonly WaitTimer _waitTimerAfterUsingItem = new WaitTimer(TimeSpan.Zero);
        private readonly WaitTimer _waitTimerForItemToAppear = new WaitTimer(TimeSpan.Zero);
        #endregion


        #region Destructor, Dispose, and cleanup
        // Empty, for now
        #endregion


        #region Overrides of CustomForcedBehavior
        public override void OnStart()
        {
            // Hunting ground processing...
            // NB: We had to defer this processing from the constructor, because XElement isn't available
            // to parse child XML nodes until OnStart() is called.
            HuntingGrounds = HuntingGroundsType.GetOrCreate(Element, "HuntingGrounds", HuntingGroundCenter);
            IsAttributeProblem |= HuntingGrounds.IsAttributeProblem;
            
            // Let QuestBehaviorBase do basic initializaion of the behavior, deal with bad or deprecated attributes,
            // capture configuration state, install BT hooks, etc.  This will also update the goal text.
            OnStart_QuestBehaviorCore(
                string.Format("Using {0} on {1}",
                    GetItemNameFromId(ItemId),
                    string.Join(", ", MobIds.Select(m => GetObjectNameFromId(m)).Distinct())));

            // If the quest is complete, this behavior is already done...
            // So we don't want to falsely inform the user of things that will be skipped.
            if (!IsDone)
            {
                CharacterSettings.Instance.PullDistance = 0;

                _waitTimerAfterUsingItem.WaitTime = TimeSpan.FromMilliseconds(WaitTimeAfterItemUse);

                // NB: This clumsiness is because Honorbuddy can launch and start using the behavior before the pokey
                // WoWclient manages to put the item into our bag after accepting a quest.  This delay waits
                // for the item to show up, if its going to.
                _waitTimerForItemToAppear.WaitTime = TimeSpan.FromSeconds(5);
                _waitTimerForItemToAppear.Reset();
            }
        }
        #endregion


        #region Main Behaviors
        protected override Composite CreateBehavior_CombatMain()
        {
            return new Decorator(context => !IsDone,
                new PrioritySelector(
                    // Since we do the bulk of the work in the CombatMain hook, we have to allow for looting, also...
                    new Decorator(context => !Me.Combat,
                        LevelBot.CreateLootBehavior()),

                    // Done due to count completing?
                    new Decorator(context => (QuestObjectiveIndex <= 0) && (Counter >= NumOfTimesToUseItem),
                        new Action(context => { BehaviorDone(); })),

                    // If WoWclient has not placed items in our bag, wait for it...
                    // NB: This clumsiness is because Honorbuddy can launch and start using the behavior before the pokey
                    // WoWclient manages to put the item into our bag after accepting a quest.  This delay waits
                    // for the item to show up, if its going to.
                    new Decorator(context => (ItemId > 0) && !IsViable(ItemToUse),
                        new PrioritySelector(
                            UtilityBehaviorPS_WaitForInventoryItem(context => ItemId),
                            new Action(context =>
                            {
                                ItemToUse = Me.CarriedItems.FirstOrDefault(i => (i.Entry == ItemId));
                            })
                        )),

                    // If item is no longer viable to use, warn user and we're done...
                    new Decorator(context => !IsViable(ItemToUse),
                        new Action(context =>
                        {
                            LogWarning(BuildMessageWithContext(Element, 
                                "We no longer have a viable {0} to use--terminating behavior.",
                                GetItemNameFromId(ItemId)));
                            BehaviorDone();
                        })),

                    // Take out any nearby mobs that aggro on us while conducting our business...
                    new Decorator(context => !Me.IsFlying,
                        UtilityBehaviorPS_SpankMobTargetingUs(context => FindViableTargets().ToList())),
                
                    // Wait additional time requested by profile writer...
                    new Decorator(context => !_waitTimerAfterUsingItem.IsFinished,
                        new Action(context =>
                        {
                            TreeRoot.StatusText = string.Format("Completing {0} wait of {1}",
                                PrettyTime(TimeSpan.FromSeconds((int)_waitTimerAfterUsingItem.TimeLeft.TotalSeconds)),
                                PrettyTime(_waitTimerAfterUsingItem.WaitTime));
                        })),
                        
                    // If no viable target, find a new mob to harass...
                    new Decorator(context => !IsViableForItemUse(SelectedTarget),
                        new Action(context =>
                        {
                            Me.ClearTarget();
                            SelectedTarget = FindViableTargets().FirstOrDefault();

                            // Target selected mob as feedback to user...
                            if ((SelectedTarget != null) && (Me.CurrentTarget != SelectedTarget))
                                { SelectedTarget.Target(); }

                            return RunStatus.Failure;   // fall through
                        })),
                    
                    // Pick a fight, if needed...
                    new Decorator(context => !Me.Combat && IsViableForItemUse(SelectedTarget),
                        UtilityBehaviorPS_GetMobsAttention(context => SelectedTarget)),

                    // No mobs in immediate vicinity...
                    // NB: if the terminateBehaviorIfNoTargetsProvider argument evaluates to 'true', calling
                    // this sub-behavior will terminate the overall behavior.
                    new Decorator(context => !Me.Combat && !IsViable(SelectedTarget),
                        UtilityBehaviorPS_NoMobsAtCurrentWaypoint(
                            context => HuntingGrounds,
                            context => false,
                            context => MobIds.Select(m => GetObjectNameFromId(m)).Distinct(),
                            context => Debug_BuildExclusions()))
                ));
        }


        protected override Composite CreateBehavior_CombatOnly()
        {
            return new Decorator(context => IsViableForItemUse(SelectedTarget),
                new PrioritySelector(
                    new Decorator(context => (Me.CurrentTarget != null) && !Me.CurrentTarget.Attackable,
                        new Action(context =>
                        {
                            BotPoi.Clear();
                            Me.ClearTarget();
                        })),

                    // If we're fighting some other mob, make certain pet is helping...
                    new Decorator(context => Me.CurrentTarget != SelectedTarget,
                        UtilityBehaviorPS_PetSetStance(context => "Defensive")),
                    
                    // Go after our chosen target...
                    // NB: If someone else tagged the mob, it will no longer be viable (already checked).
                    new Decorator(context => !IsDone && (Me.CurrentTarget == SelectedTarget),
                        new PrioritySelector(

                            // Recall pet, if necessary...
                            new Decorator(context => (SelectedTarget.HealthPercent < RecallPetAtMobPercentHealth)
                                                    && (Me.GotAlivePet && (Me.Pet.CurrentTarget == SelectedTarget)),
                                new PrioritySelector(
                                    new Action(context =>
                                    {
                                        LogInfo("Recalling Pet from '{0}' (health: {1:F1})",
                                            SelectedTarget.Name, SelectedTarget.HealthPercent);
                                        return RunStatus.Failure;
                                    }),
                                    UtilityBehaviorPS_PetActionFollow(),
                                    UtilityBehaviorPS_PetSetStance(context => "Passive")
                                )),

                            // If we are beyond the max range allowed to use the item, move within range...
                            new Decorator(context => SelectedTarget.Distance > MaxRangeToUseItem,
                                UtilityBehaviorPS_MoveTo(
                                    context => SelectedTarget.Location,
                                    context => string.Format("within {0} feet of {1}", MaxRangeToUseItem, SelectedTarget.Name))),
                            
                            // If time to use the item, do so...
                            new Decorator(context => IsViable(ItemToUse) && IsUseItemNeeded(SelectedTarget),
                                new PrioritySelector(
                                    // Halt combat until we are able to use the item...
                                    new Decorator(context => !ItemToUse.Usable
                                                                && ((UseItemStrategy == UseItemStrategyType.UseItemContinuouslyOnTargetDontDefend)
                                                                    || (UseItemStrategy == UseItemStrategyType.UseItemOncePerTargetDontDefend)),
                                        new Action(context =>
                                        {
                                            // We use LUA to stop casting, since SpellManager.StopCasting() doesn't seem to work...
                                            if (Me.IsCasting)
                                                { Lua.DoString("SpellStopCasting()"); }
                                            
                                            if (Me.IsMoving)
                                                { WoWMovement.MoveStop(); }

                                            TreeRoot.StatusText = string.Format("Combat halted--waiting for {0} to become usable.", ItemToUse.Name);
                                        })),

                                    new Sequence(
                                        UtilityBehaviorSeq_UseItemOn(context => ItemToUse, context => SelectedTarget),
                                        // Allow a brief time for WoWclient to apply aura to mob...
                                        new WaitContinue(TimeSpan.FromMilliseconds(5000),
                                            context => ItemUseAlwaysSucceeds || SelectedTarget.HasAura(ItemAppliesAuraId),
                                            new ActionAlwaysSucceed()),
                                        new Action(context =>
                                        {
                                            _waitTimerAfterUsingItem.Reset();

                                            if (ItemUseAlwaysSucceeds || SelectedTarget.HasAura(ItemAppliesAuraId))
                                            {
                                                // Count our success if no associated quest...
                                                if (QuestId == 0)
                                                    { ++Counter; }

                                                // If we can only use the item once per target, blacklist this target from subsequent selection...
                                                if ((UseItemStrategy == UseItemStrategyType.UseItemOncePerTarget)
                                                    || (UseItemStrategy == UseItemStrategyType.UseItemOncePerTargetDontDefend))
                                                {
                                                    BlacklistForInteracting(SelectedTarget, TimeSpan.FromSeconds(180));
                                                }

                                                // If we can't defend ourselves from the target, blacklist it for combat and move on...
                                                if (IsViable(SelectedTarget)
                                                    && ((UseItemStrategy == UseItemStrategyType.UseItemContinuouslyOnTargetDontDefend)
                                                        || (UseItemStrategy == UseItemStrategyType.UseItemOncePerTargetDontDefend)))
                                                {
                                                    Blacklist.Add(SelectedTarget, BlacklistFlags.Combat, TimeSpan.FromSeconds(180));
                                                    BotPoi.Clear();
                                                    Me.ClearTarget();
                                                    SelectedTarget = null;
                                                }
                                            }
                                        })
                                )))
                        ))
                    ));
        }


        protected override Composite CreateBehavior_DeathMain()
        {
            return new PrioritySelector(
                // empty, for now
                );
        }


        protected override Composite CreateMainBehavior()
        {
            return new PrioritySelector(
                // Delay, if necessary...
                // NB: We must do this prior to checking for 'behavior done'.  Otherwise, profiles
                // that don't have an associated quest, and put the behavior in a <While> loop will not behave
                // as the profile writer expects.  They expect the delay to be executed if the interaction
                // succeeded.
                new Decorator(context => !_waitTimerAfterUsingItem.IsFinished,
                    new PrioritySelector(
                        // Take out any nearby mobs that aggro on us while we wait...
                        new Decorator(context => !Me.IsFlying,
                            UtilityBehaviorPS_SpankMobTargetingUs(context => FindViableTargets())),
                        new Action(context =>
                        {
                            TreeRoot.StatusText = string.Format("Completing {0} wait of {1}",
                                PrettyTime(TimeSpan.FromSeconds((int)_waitTimerAfterUsingItem.TimeLeft.TotalSeconds)),
                                PrettyTime(_waitTimerAfterUsingItem.WaitTime));
                        })
                    ))
            );
        }
        #endregion


        #region Helpers
        private IEnumerable<WoWUnit> FindViableTargets()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                var isFlyingOrSwimming = Me.IsFlying || Me.IsSwimming;

                var targets =
                   from wowUnit in FindUnitsFromIds(MobIds)
                    where
                        IsViableForItemUse(wowUnit)
                        && (wowUnit.Distance < CollectionDistance)
                   orderby
                        (isFlyingOrSwimming 
                            ? Me.Location.Distance(wowUnit.Location)
                            : Me.Location.SurfacePathDistance(wowUnit.Location))
                    select wowUnit;

                return targets;
            }
        }


        private double HealthPercentToStopCombat(WoWUnit target)
        {
            if (target == null)
                { return 0.0; }

            var harmfulAuraCount = target.Debuffs.Values.Sum(a => Math.Max(1, a.StackCount));

            // We stop attacking mob early by 3% health for each harmful aura on the target...
            return Math.Min(100.0, UseWhenMobHasHealthPercent + (harmfulAuraCount * 3.0));
        }


        private bool IsUseItemNeeded(WoWUnit wowUnit)
        {
            return
                IsViableForItemUse(wowUnit)
                && ((UseWhenMeHasAuraId > 0) && Me.HasAura(UseWhenMeHasAuraId))
                    || ((UseWhenMeMissingAuraId > 0) && !Me.HasAura(UseWhenMeMissingAuraId))
                    || ((UseWhenMobCastingSpellId > 0) && (wowUnit.CastingSpellId == UseWhenMobCastingSpellId))
                    || ((UseWhenMobHasAuraId > 0) && wowUnit.HasAura(UseWhenMobHasAuraId))
                    || ((UseWhenMobMissingAuraId > 0) && !wowUnit.HasAura(UseWhenMobMissingAuraId))
                    || ((UseWhenMobHasHealthPercent > 0) && wowUnit.IsAlive && (wowUnit.HealthPercent <= HealthPercentToStopCombat(wowUnit)));
        }


        // 24Feb2013-08:11UTC chinajade
        private bool IsViableForItemUse(WoWUnit wowUnit)
        {
            return
                IsViableForInteracting(wowUnit)
                && wowUnit.IsAlive
                && wowUnit.Attackable
                && (ItemUseAlwaysSucceeds || !wowUnit.HasAura(ItemAppliesAuraId));
        }
        #endregion


        #region Debug
        private string Debug_BuildExclusions()
        {
            IEnumerable<WoWObject> interactCandidates =
                from wowObject in FindUnitsFromIds(MobIds)
                where
                    IsViable(wowObject)
                select wowObject;

            var excludedUnitReasons = new StringBuilder();

            foreach (var wowObject in interactCandidates)
            {
                excludedUnitReasons.Append("    ");
                excludedUnitReasons.Append(Debug_TellWhyExcluded(wowObject));
                excludedUnitReasons.AppendLine();
            }

            if (excludedUnitReasons.Length > 0)
            {
                excludedUnitReasons.Insert(0, string.Format("{0}Excluded Units:{0}",
                    Environment.NewLine));
                excludedUnitReasons.AppendFormat("{0}    {1}",
                    Environment.NewLine,
                    GetXmlFileReference(Element));
            }

            return excludedUnitReasons.ToString();
        }


        private string Debug_TellWhyExcluded(WoWObject wowObject)
        {
            var reasons = new List<string>();

            if (!IsViable(wowObject))
                { return "[NotViable]"; }

            if ((MovementBy == MovementByType.NavigatorOnly) && !Navigator.CanNavigateFully(Me.Location, wowObject.Location))
                { reasons.Add("NotMeshNavigable"); }

            if (IsBlacklistedForInteraction(wowObject))
                { reasons.Add("Blacklisted"); }

            if (IgnoreMobsInBlackspots && Targeting.IsTooNearBlackspot(ProfileManager.CurrentProfile.Blackspots, wowObject.Location))
                { reasons.Add(string.Format("InBlackspot(object @{0})", wowObject.Location)); }

            if (IsInCompetition(wowObject, NonCompeteDistance))
            {
                reasons.Add(string.Format("InCompetition({0} players within {1:F1})",
                    FindPlayersNearby(wowObject.Location, NonCompeteDistance).Count(),
                    NonCompeteDistance));
            }

            WoWUnit wowUnit = wowObject.ToUnit();
            if (wowUnit != null)
            {
                var wowUnitAuras = wowUnit.GetAllAuras().ToList();
                
                if (!wowUnit.IsAlive)
                {
                    reasons.Add("!IsAlive");
                }
                
                var itemAppliedAura = wowUnitAuras.FirstOrDefault(a => a.SpellId == ItemAppliesAuraId);
                if ((ItemAppliesAuraId > 0) && (itemAppliedAura != null))
                {
                    reasons.Add(string.Format("HasItemAura({0},{1})", itemAppliedAura.Name, itemAppliedAura.SpellId));
                }

                if (!wowUnit.IsUntagged())
                    { reasons.Add("Tagged"); }
            }

            return string.Format("{0} [{1}]", wowObject.SafeName(), string.Join(",", reasons));
        }
        #endregion
    }
}
