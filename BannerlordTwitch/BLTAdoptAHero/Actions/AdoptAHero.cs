using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.UI;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using YamlDotNet.Serialization;

namespace BLTAdoptAHero
{
    [LocDisplayName("{=NkZXnSQI}Adopt A Hero"),
     LocDescription("{=fd7G5N0Q}Allows viewer to 'adopt' a hero in game -- the hero name will change to the viewers name, and they can control it with further commands"),
     UsedImplicitly]
    public class AdoptAHero : IRewardHandler, ICommandHandler
    {
        public static string NoHeroMessage => "{=r8nXZgcY}Couldn't find your hero, did you adopt one yet?".Translate();

        [CategoryOrder("General", 0),
         CategoryOrder("Limits", 1),
         CategoryOrder("Initialization", 1)]
        private class Settings : IDocumentable
        {
            [LocDisplayName("{=TLrDxhlh}Create New"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Create a new hero instead of adopting an existing one (they will be a wanderer at a random tavern)"),
             PropertyOrder(1), UsedImplicitly]
            public bool CreateNew { get; set; }

            [LocDisplayName("{=nPIcT2s7}Allow Noble"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=XvZN7OOY}Allow noble heroes (if CreateNew is false)"),
             PropertyOrder(1), UsedImplicitly]
            public bool AllowNoble { get; set; } = true;

            [LocDisplayName("{=VVFsa8LQ}Allow Wanderer"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=9lE2KSvC}Allow wanderer heroes (if CreateNew is false)"),
             PropertyOrder(2), UsedImplicitly]
            public bool AllowWanderer { get; set; } = true;

            [LocDisplayName("{=FUsqiXdp}Allow Party Leader"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=L4cLwv9E}Allow heroes that lead parties (if CreateNew is false)"),
             PropertyOrder(3), UsedImplicitly]
            public bool AllowPartyLeader { get; set; } = false;

            [LocDisplayName("{=9DdalaHK}Allow Minor Faction Hero"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=NCrSbTGc}Allow heroes that lead minor factions (if CreateNew is false)"),
             PropertyOrder(4), UsedImplicitly]
            public bool AllowMinorFactionHero { get; set; } = false;

            [LocDisplayName("{=A8G9ctbn}Allow Player Companion"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=6EjGRMkt}Allow companions (not tested, if CreateNew is false)"),
             PropertyOrder(5), UsedImplicitly]
            public bool AllowPlayerCompanion { get; set; }

            [LocDisplayName("{=B2z7T1xQ}Only Same Faction"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=QvQGwFyl}Only allow heroes from same faction as player"),
             PropertyOrder(6), UsedImplicitly]
            public bool OnlySameFaction { get; set; }

            public enum ViewerSelect
            {
                Nothing,
                Culture,
                Faction,
                Name,
                Clan,
                NameClan
            }

            [LocDisplayName("{=VZ91tlYL}Create clan"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=vyIJe4pb}Create desired clan if it doesn't exist"),
             PropertyOrder(6), UsedImplicitly]
            public bool CreateClan { get; set; }

            [LocDisplayName("{=NoKO59t1}Viewer Selects"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=VSYokFLH}What criteria the viewer selects via text input (make sure to enable 'Is User Input " +
                            "Required' it in the Reward Specification > Misc section if you set this to something other than None). " +
                            "Faction selection is not compatible with Allow Wanderer or Create New, as wanderers do not have a faction until they are recruited."),
             PropertyOrder(7), UsedImplicitly]
            public ViewerSelect ViewerSelects { get; set; }

            [LocDisplayName("{=dvbkxJQz}Inheritance"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=KLJtpEjg}What fraction of assets will be inherited when a new character is adopted after an old one died (0 to 1)"),
             UIRangeAttribute(0, 1, 0.05f),
             Editor(typeof(SliderFloatEditor), typeof(SliderFloatEditor)),
             PropertyOrder(8), UsedImplicitly]
            public float Inheritance { get; set; } = 0.25f;

            [LocDisplayName("{=Bi19tTPj}Maximum Inherited Custom Items"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=tFolfAOn}How many custom items can be inherited"),
             Range(0, Int32.MaxValue),
             PropertyOrder(9), UsedImplicitly]
            public int MaxInheritedCustomItems { get; set; } = 2;

            [LocDisplayName("{=O4DGlP9Z}Subscriber Only"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=TBNkHsLC}Only subscribers can adopt"),
             PropertyOrder(10), UsedImplicitly]
            public bool SubscriberOnly { get; set; }

            [LocDisplayName("{=dO41CKIU}Minimum Subscribed Months"),
             LocCategory("Limits", "{=1lHWj3nT}Limits"),
             LocDescription("{=BVZwDqR0}Only viewers who have been subscribers for at least this many months can adopt, ignored if not specified"),
             PropertyOrder(11), UsedImplicitly]
            public int? MinSubscribedMonths { get; set; }

            [LocDisplayName("{=iOmYBC7I}Starting Gold"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=pZMkJLix}Gold the adopted hero will start with"),
             PropertyOrder(1), UsedImplicitly,
             Document]
            public int StartingGold { get; set; }

            [LocDisplayName("{=ZXwbvbbq}Override Age"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=gxQgrAey}Override the heroes age"),
             PropertyOrder(2), UsedImplicitly]
            public bool OverrideAge { get; set; }

            [LocDisplayName("{=NEBQgHiX}Starting Age Range"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=TYqEBuLW}Random range of age when overriding it"),
             PropertyOrder(3), UsedImplicitly]
            public RangeFloat StartingAgeRange { get; set; } = new(18, 35);

            [LocDisplayName("{=9IOFHQjS}Starting Skills"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=C4rV4f2F}Starting skills, if empty then default skills of the adopted hero will be left in tact"),
             Editor(typeof(DefaultCollectionEditor), typeof(DefaultCollectionEditor)),
             PropertyOrder(4), UsedImplicitly]
            public ObservableCollection<SkillRangeDef> StartingSkills { get; set; } = new();

            [YamlIgnore, Browsable(false)]
            public IEnumerable<SkillRangeDef> ValidStartingSkills
                => StartingSkills?.Where(s => s.Skill != SkillsEnum.None);

            [LocDisplayName("{=IAKQCRa1}Starting Equipment Tier"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=mQwjHXfC}Equipment tier the adopted hero will start with, if you don't specify then they get the heroes existing equipment"),
             Range(0, 6),
             PropertyOrder(5), UsedImplicitly]
            public int? StartingEquipmentTier { get; set; }

            [LocDisplayName("{=0vGFJdO1}Starting Class"),
             LocCategory("Initialization", "{=DRNO9OAl}Initialization"),
             LocDescription("{=zgjyFL6i}Starting class of the hero"),
             PropertyOrder(6), ItemsSource(typeof(HeroClassDef.ItemSource)), UsedImplicitly]
            public Guid StartingClass { get; set; }

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                if (SubscriberOnly)
                {
                    generator.Value("<strong>" +
                                    "{=4zAUTiSP}Subscriber Only".Translate() +
                                    "</strong>");
                }
                if (CreateNew)
                {
                    generator.Value("{=cJQCU33B}Newly created wanderer".Translate());
                }
                else
                {
                    var allowed = new List<string>();
                    if (AllowNoble) allowed.Add("{=fP84ES0X}Noble".Translate());
                    if (AllowWanderer) allowed.Add("{=ozRaAx6L}Wanderer".Translate());
                    if (AllowPlayerCompanion) allowed.Add("{=YucejFfO}Companions".Translate());
                    generator.PropertyValuePair("{=UNtHwNhx}Allowed".Translate(), string.Join(", ", allowed));
                }

                if (CreateClan) generator.Value("{=xjmF7XjL}Create the selected clan if it does not exist".Translate());

                if (OnlySameFaction) generator.Value("{=6W0OJKkA}Same faction only".Translate());
                if (ViewerSelects == ViewerSelect.Culture) generator.Value("{=Lg6V3rzn}Viewer selects culture".Translate());
                if (ViewerSelects == ViewerSelect.Faction) generator.Value("{=kps5JINU}Viewer selects faction".Translate());
                if (ViewerSelects is ViewerSelect.Name or ViewerSelect.NameClan) generator.Value("{=EUJZfjFj}Viewer selects leader by Name".Translate());
                if (ViewerSelects is ViewerSelect.Clan or ViewerSelect.NameClan) generator.Value("{=y2U378lu}Viewer selects leader by Clan".Translate());

                if (OverrideAge)
                {
                    generator.PropertyValuePair("{=pDP8b5HR}Starting Age Range".Translate(),
                        StartingAgeRange.IsFixed
                            ? $"{StartingAgeRange.Min}"
                            : $"{StartingAgeRange.Min} to {StartingAgeRange.Max}"
                        );
                }

                generator.PropertyValuePair("{=FvhsCSd3}Starting Gold".Translate(), $"{StartingGold}");
                generator.PropertyValuePair("{=wP0lfTf3}Inheritance".Translate(),
                    "{=mDK67efh}{Inheritance}% of gold spent on equipment and retinue"
                        .Translate(("Inheritance", (int)(Inheritance * 100))) +
                    ", " +
                    (MaxInheritedCustomItems == 0
                        ? "{=76NtQIGB}no custom items".Translate()
                        : "{=sEDQrZCp}up to {MaxInheritedCustomItems} custom items"
                            .Translate(("MaxInheritedCustomItems", MaxInheritedCustomItems))
                    ));

                if (ValidStartingSkills.Any())
                {
                    generator.PropertyValuePair("{=9IOFHQjS}Starting Skills".Translate(), () =>
                        generator.Table("starting-skills", () =>
                        {
                            generator.TR(() =>
                                generator.TH("{=OEMBeawy}Skill".Translate()).TH("{=iu0dtUP5}Level".Translate())
                            );
                            foreach (var s in ValidStartingSkills)
                            {
                                generator.TR(() =>
                                {
                                    generator.TD(s.Skill.GetDisplayName());
                                    generator.TD(s.IsFixed
                                        ? $"{s.MinLevel}"
                                        : "{=yVydxRHh}{From} to {To}".Translate(
                                            ("From", s.MinLevel), ("To", s.MaxLevel)));
                                });
                            }
                        }));
                }

                if (StartingEquipmentTier.HasValue)
                {
                    generator.PropertyValuePair("{=IAKQCRa1}Starting Equipment Tier".Translate(), $"{StartingEquipmentTier.Value}");
                }

                if (StartingClass != Guid.Empty)
                {
                    var classDef = BLTAdoptAHeroModule.HeroClassConfig.GetClass(StartingClass);
                    if (classDef != null)
                    {
                        generator.PropertyValuePair("{=0vGFJdO1}Starting Class".Translate(),
                            () => generator.LinkToAnchor(classDef.Name.ToString(), classDef.Name.ToString()));
                    }
                }
            }
        }

        Type IRewardHandler.RewardConfigType => typeof(Settings);
        void IRewardHandler.Enqueue(ReplyContext context, object config)
        {
            var hero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
            if (hero?.IsAlive == true)
            {
                ActionManager.NotifyCancelled(context, "{=mJfD7e2g}You have already adopted a hero!".Translate());
                return;
            }
            var settings = (Settings)config;
            (bool success, string message) = ExecuteInternal(context.UserName, settings, context.Args);
            if (success)
            {
                ActionManager.NotifyComplete(context, message);
            }
            else
            {
                ActionManager.NotifyCancelled(context, message);
            }
        }

        Type ICommandHandler.HandlerConfigType => typeof(Settings);
        void ICommandHandler.Execute(ReplyContext context, object config)
        {
            if (BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName) != null)
            {
                ActionManager.SendReply(context, "{=mJfD7e2g}You have already adopted a hero!".Translate());
                return;
            }

            var settings = (Settings)config;
            if (settings.MinSubscribedMonths > 0 && context.SubscribedMonthCount < settings.MinSubscribedMonths)
            {
                ActionManager.SendReply(context,
                    "{=4K7Q7gR0}You must be subscribed for at least {MinSubscribedMonths} months to adopt a hero with this command!".Translate(("MinSubscribedMonths", settings.MinSubscribedMonths)));
                return;
            }
            if (!context.IsSubscriber && settings.SubscriberOnly)
            {
                ActionManager.SendReply(context, "{=0QeQPxYi}You must be subscribed to adopt a hero with this command!".Translate());
                return;
            }

            (_, string message) = ExecuteInternal(context.UserName, settings, context.Args);
            ActionManager.SendReply(context, message);
        }

        private static (bool success, string message) ExecuteInternal(string userName, Settings settings, string contextArgs)
        {
            Hero newHero = null;

            if (settings.ViewerSelects == Settings.ViewerSelect.Faction && settings.AllowWanderer)
            {
                throw new Exception($"AdoptAHero config is incorrect: 'Viewer Select Faction' and 'Allow Wanderer' cannot both be enabled as wanderers do not have factions");
            }

            if (settings.ViewerSelects == Settings.ViewerSelect.Faction && settings.CreateNew)
            {
                throw new Exception($"AdoptAHero config is incorrect: 'Viewer Select Faction' and 'Create New' cannot both be enabled as it creates wanderers, which do not have factions");
            }

            if ((settings.ViewerSelects is Settings.ViewerSelect.Name or Settings.ViewerSelect.NameClan) && settings.CreateNew)
            {
                throw new Exception($"AdoptAHero config is incorrect: 'Viewer Select Name' and 'Create New' cannot both be enabled because name is used to search of existing heroes");
            }

            if (settings.ViewerSelects == Settings.ViewerSelect.Faction && settings.OnlySameFaction)
            {
                throw new Exception($"AdoptAHero config is incorrect: 'Viewer Select Faction' and 'Only Same Faction' cannot both be enabled as they conflict");
            }


            bool newClan = false;
            CultureObject desiredCulture = null;
            IFaction desiredFaction = null;
            String desiredName = null;
            Clan desiredClan = null;
            if (settings.ViewerSelects == Settings.ViewerSelect.Culture)
            {
                if (contextArgs.Length > 1)
                {
                    desiredCulture = CampaignHelpers.MainCultures.FirstOrDefault(c =>
                        c.Name.ToString().StartsWith(contextArgs, StringComparison.CurrentCultureIgnoreCase));
                    if (desiredCulture == null)
                    {
                        return (false, "{=dVVduPvy}No culture starting with '{Text}' found".Translate(("Text", contextArgs)));
                    }
                }
                else
                {
                    return (false, "{=SViljL0E}Please enter one of {Cultures}".Translate(("Cultures", string.Join(", ", CampaignHelpers.MainCultures.Select(c => c.Name.ToString())))));
                }
            }
            else if (settings.ViewerSelects == Settings.ViewerSelect.Faction)
            {
                if (contextArgs.Length > 1)
                {
                    desiredFaction = CampaignHelpers.MainFactions.FirstOrDefault(c =>
                        c.Name.ToString().StartsWith(contextArgs, StringComparison.CurrentCultureIgnoreCase));

                    if (desiredFaction == null)
                    {
                        return (false, "{=k4Hj2rxu}No faction starting with '{Text}' found".Translate(("Text", contextArgs)));
                    }
                }
                else
                {
                    return (false, "{=jjUmUpia}Please enter part of the name of the faction you wish to join".Translate());
                }
            }
            else if (settings.ViewerSelects == Settings.ViewerSelect.Name)
            {
                if (contextArgs.Length > 1)
                {
                    desiredName = contextArgs;
                }
                else
                {
                    return (false, "{=jjUmUpia}Please enter the name of the leader you wish to adopt".Translate());
                }
            }
            else if (settings.ViewerSelects == Settings.ViewerSelect.Clan)
            {
                if (contextArgs.Length > 1)
                {
                    desiredClan = CampaignHelpers.AllHeroes.Select(h => h.Clan).Distinct().FirstOrDefault(c =>
                    {
                        if (c != null)
                        {
                            return c.Name.ToString() == contextArgs;
                        }

                        return false;

                    });

                    if (desiredClan == null)
                    {
                        if (settings.CreateClan)
                        {
                            newClan = true;
                            desiredClan = Clan.CreateClan("[BLT Clan]" + contextArgs + "_" + (object)Clan.All.Count(t => t.Name.ToString() == contextArgs));
                            CultureObject clanCulture = CampaignHelpers.MainCultures.SelectRandom();
                            Banner clanBanner = Banner.CreateRandomBanner();
                            desiredClan.InitializeClan(new TextObject(contextArgs), new TextObject(contextArgs), clanCulture, clanBanner);
                            desiredClan.UpdateHomeSettlement(Settlement.All.SelectRandom());
                        }
                        else
                        {
                            return (false, "{=q9m9Yp1F}Error could not find a clan with the name {clanName}".Translate(("clanName", contextArgs)));
                        }

                    }
                }
                else
                {
                    return (false, "{=sN23nt5M}Please enter the name of the clan you wish to join".Translate());
                }
            }
            else if (settings.ViewerSelects == Settings.ViewerSelect.NameClan)
            {
                String[] nameClan = contextArgs.Split('/');
                if (contextArgs.Length > 1 && nameClan.Length == 2)
                {
                    desiredName = nameClan[0].Replace(" ", string.Empty);
                    string clanName = nameClan[1].Replace(" ", string.Empty);
                    desiredClan = CampaignHelpers.AllHeroes.Select(h => h.Clan).Distinct().FirstOrDefault(c => c.Name.ToString() == clanName);
                    if (desiredClan == null)
                    {
                        return (false, "{=CVcUtTWc}Could not find the clan with the name {name}".Translate(("name", clanName)));
                    }
                }
                else
                {
                    return (false, "{=4oDdDR9s}Please enter the name and clan of the leader you wish to adopt separated with /".Translate());
                }
            }

            // Create or find a hero for adopting
            if (settings.CreateNew)
            {
                var character = desiredCulture != null
                    ? CampaignHelpers.GetWandererTemplates(desiredCulture).SelectRandom()
                    : CampaignHelpers.AllWandererTemplates.SelectRandom();

                if (character != null)
                {
                    newHero = HeroCreator.CreateSpecialHero(character);
                    newHero.ChangeState(Hero.CharacterStates.Active);
                    if (settings.ViewerSelects is Settings.ViewerSelect.Clan)
                    {
                        newHero.Clan = desiredClan;
                    }
                }
            }
            else
            {
                newHero = BLTAdoptAHeroCampaignBehavior.GetAvailableHeroes(h =>
                        // Filter by allowed types
                        (settings.AllowNoble || !h.IsLord)
                        && (settings.AllowWanderer || !h.IsWanderer)
                        && (settings.AllowPartyLeader || !h.IsPartyLeader)
                        && (settings.AllowMinorFactionHero || !h.IsMinorFactionHero)
                        && (settings.AllowPlayerCompanion || !h.IsPlayerCompanion)
                        // Select correct clan faction
                        && (!settings.OnlySameFaction
                            || Clan.PlayerClan?.MapFaction != null
                            && Clan.PlayerClan?.MapFaction == h.Clan?.MapFaction)
                        // Disallow rebel clans as they may get deleted if the rebellion fails
                        && h.Clan?.IsRebelClan != true
                        && (desiredCulture == null || desiredCulture == h.Culture)
                        && (desiredFaction == null || desiredFaction == h.MapFaction)
                        && (desiredName == null || desiredName == h.Name.ToString())
                        && (desiredClan == null || (h.Clan != null && desiredClan == h.Clan))
                    ).SelectRandom();




                if (newHero == null && settings.OnlySameFaction && Clan.PlayerClan?.MapFaction?.StringId == "player_faction")
                {
                    return (false, "{=XlQUIIsg}No hero is available: player is not in a faction (disable Player Faction Only, or join a faction)!".Translate());
                }
            }

            if (newHero == null)
            {
                return (false, "{=E7wqQ2kg}You can't adopt a hero: no available hero matching the requirements was found!".Translate());
            }

            if (newClan)
            {
                desiredClan.SetLeader(newHero);
            }

            if (settings.OverrideAge)
            {
                newHero.SetBirthDay(CampaignTime.YearsFromNow(-Math.Max(Campaign.Current.Models.AgeModel.HeroComesOfAge, settings.StartingAgeRange.RandomInRange())));
            }

            // Place hero where we want them
            if (settings.CreateNew)
            {
                var targetSettlement = Settlement.All.Where(s => s.IsTown).SelectRandom();
                EnterSettlementAction.ApplyForCharacterOnly(newHero, targetSettlement);
                Log.Info($"Placed new hero {newHero.Name} at {targetSettlement.Name}");
            }

            if (settings.ValidStartingSkills?.Any() == true)
            {
                newHero.HeroDeveloper.ClearHero();

                foreach (var skill in settings.ValidStartingSkills)
                {
                    var actualSkills = SkillGroup.GetSkills(skill.Skill);
                    newHero.HeroDeveloper.SetInitialSkillLevel(actualSkills.SelectRandom(),
                        MBMath.ClampInt(
                            MBRandom.RandomInt(
                                Math.Min(skill.MinLevel, skill.MaxLevel),
                                Math.Max(skill.MinLevel, skill.MaxLevel)
                                ), 0, 300)
                        );
                }

                newHero.HeroDeveloper.InitializeHeroDeveloper();
            }

            // A wanderer MUST have at least 1 skill point, or they get killed on load 
            if (newHero.GetSkillValue(CampaignHelpers.AllSkillObjects.First()) == 0)
            {
                newHero.HeroDeveloper.SetInitialSkillLevel(CampaignHelpers.AllSkillObjects.First(), 1);
            }

            HeroClassDef classDef = null;
            if (settings.StartingClass != default)
            {
                classDef = BLTAdoptAHeroModule.HeroClassConfig.GetClass(settings.StartingClass);
                if (classDef == null)
                {
                    Log.Error($"AdoptAHero: StartingClass not found, please re-select it in settings");
                }
                else
                {
                    BLTAdoptAHeroCampaignBehavior.Current.SetClass(newHero, classDef);
                }
            }

            // Setup skills first, THEN name, as skill changes can generate feed messages for adopted characters
            string oldName = newHero.Name.ToString();
            BLTAdoptAHeroCampaignBehavior.Current.InitAdoptedHero(newHero, userName);

            // Inherit items before equipping, so we can use them DURING equipping
            var inheritedItems = BLTAdoptAHeroCampaignBehavior.Current.InheritCustomItems(newHero, settings.MaxInheritedCustomItems);
            if (settings.StartingEquipmentTier.HasValue)
            {
                EquipHero.RemoveAllEquipment(newHero);
                if (settings.StartingEquipmentTier.Value > 0)
                {
                    EquipHero.UpgradeEquipment(newHero, settings.StartingEquipmentTier.Value - 1,
                        classDef, replaceSameTier: false);
                }
                BLTAdoptAHeroCampaignBehavior.Current.SetEquipmentTier(newHero, settings.StartingEquipmentTier.Value - 1);
                BLTAdoptAHeroCampaignBehavior.Current.SetEquipmentClass(newHero, classDef);
            }

            if (!CampaignHelpers.IsEncyclopediaBookmarked(newHero))
            {
                CampaignHelpers.AddEncyclopediaBookmarkToItem(newHero);
            }

            BLTAdoptAHeroCampaignBehavior.Current.SetHeroGold(newHero, settings.StartingGold);

            int inheritedGold = BLTAdoptAHeroCampaignBehavior.Current.InheritGold(newHero, settings.Inheritance);
            int newGold = BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(newHero);

            if (settings.CreateNew)
                BLTAdoptAHeroCampaignBehavior.Current.SetIsCreatedHero(newHero, true);

            var inherited = inheritedItems.Select(i => i.GetModifiedItemName().ToString()).ToList();
            if (inheritedGold != 0)
            {
                inherited.Add($"{inheritedGold}{Naming.Gold}");
            }

            Log.ShowInformation(
                "{=K7nuJVCN}{OldName} is now known as {NewName}!".Translate(("OldName", oldName), ("NewName", newHero.Name)),
                newHero.CharacterObject, Log.Sound.Horns2);

            return inherited.Any()
                ? (true, "{=PAc5S0GY}{OldName} is now known as {NewName}, they have {NewGold} (inheriting {Inherited})!"
                    .Translate(
                        ("OldName", oldName),
                        ("NewName", newHero.Name),
                        ("NewGold", newGold + Naming.Gold),
                        ("Inherited", string.Join(", ", inherited))))
                : (true, "{=lANBKEFN}{OldName} is now known as {NewName}, they have {NewGold}!".Translate(
                    ("OldName", oldName),
                    ("NewName", newHero.Name),
                    ("NewGold", newGold + Naming.Gold)));
        }
    }
}
