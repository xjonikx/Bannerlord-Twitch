using System;
using System.Linq;
using System.Text;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace BLTAdoptAHero.Actions
{
    [LocDisplayName("{=NkZXnSQI}Kingdom Management"),
     LocDescription("{=fd7G5N0Q}Allow viewer to change their clans Kingdom or make leader decisions"),
     UsedImplicitly]
    public class KingdomManagement : HeroCommandHandlerBase
    {
        [CategoryOrder("Join", 0),
         CategoryOrder("Rebel", 1),
         CategoryOrder("Stats", 4)]
        private class Settings : IDocumentable
        {
            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Enable joining kingdoms command"),
             PropertyOrder(1), UsedImplicitly]
            public bool JoinEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Max Clans"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Maximum clans (includes NPC's) before join is disallowed"),
             PropertyOrder(2), UsedImplicitly]
            public int JoinMaxClans { get; set; } = 20;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Cost of joining a kingdom"),
             PropertyOrder(3), UsedImplicitly]
            public int JoinPrice { get; set; } = 150000;

            [LocDisplayName("{=TLrDxhlh}Players Kingdom?"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Allow viewers to join the players kingdom"),
             PropertyOrder(4), UsedImplicitly]
            public bool JoinAllowPlayer { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Rebel", "{=C5T5nnix}Rebel"),
             LocDescription("{=F1KDzuZZ}Enable viewer clan rebelling against their kingdom"),
             PropertyOrder(1), UsedImplicitly]
            public bool RebelEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Rebel", "{=C5T5nnix}Rebel"),
             LocDescription("{=F1KDzuZZ}Cost of starting a rebellion"),
             PropertyOrder(2), UsedImplicitly]
            public int RebelPrice { get; set; } = 2500000;

            [LocDisplayName("{=TLrDxhlh}Minimum Clan Tier"),
             LocCategory("Rebel", "{=C5T5nnix}Rebel"),
             LocDescription("{=F1KDzuZZ}Minimum clan tier to start a rebellion"),
             PropertyOrder(3), UsedImplicitly]
            public int RebelClanTierMinimum { get; set; } = 2;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Stats", "{=C5T5nnix}Stats"),
             LocDescription("{=F1KDzuZZ}Enable stats command"),
             PropertyOrder(1), UsedImplicitly]
            public bool StatsEnabled { get; set; } = true;

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Join Enabled".Translate(), $"{JoinEnabled}");
                generator.PropertyValuePair("Max Clans".Translate(), $"{JoinMaxClans}");
                generator.PropertyValuePair("Join Price".Translate(), $"{JoinPrice}");
                generator.PropertyValuePair("Allow Join Players Kingdom?".Translate(), $"{JoinAllowPlayer}");
                generator.PropertyValuePair("Rebel Enabled".Translate(), $"{RebelEnabled}");
                generator.PropertyValuePair("Rebel Price".Translate(), $"{RebelPrice}");
                generator.PropertyValuePair("Rebel Minimum Clan Tier".Translate(), $"{RebelClanTierMinimum}");
                generator.PropertyValuePair("Stats Enabled".Translate(), $"{StatsEnabled}");
            }
        }
        public override Type HandlerConfigType => typeof(Settings);

        protected override void ExecuteInternal(Hero adoptedHero, ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
        {
            if (config is not Settings settings) return;
            //var adoptedHero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
            if (adoptedHero == null)
            {
                onFailure(AdoptAHero.NoHeroMessage);
                return;
            }
            if (Mission.Current != null)
            {
                onFailure("{=wkhZ6q7d}You cannot manage your clan, as a mission is active!".Translate());
                return;
            }
            if (adoptedHero.HeroState == Hero.CharacterStates.Prisoner)
            {
                onFailure("{=wkhZ6q7d}You cannot manage your kingdom, as you are a prisoner!".Translate());
                return;
            }
            if (adoptedHero.Clan == null)
            {
                onFailure("{=wkhZ6q7d}You cannot manage your kingdom, as you are not in a clan".Translate());
                return;
            }

            if (context.Args.IsEmpty())
            {
                if (adoptedHero.Clan.Kingdom == null)
                {
                    onFailure("{=wkhZ6q7d}Your clan is not in a Kingdom".Translate());
                    return;
                }
                onSuccess("{=wkhZ6q7d}Your clan {clanName} is a member of the kingom {kingdomName}".Translate(("clanName", adoptedHero.Clan.Name.ToString()), ("kingdomName", adoptedHero.Clan.Kingdom.Name.ToString())));
                return;
            }

            var splitArgs = context.Args.Split(' ');
            var command = splitArgs[0];
            var desiredName = string.Join(" ", splitArgs.Skip(1)).Trim();

            switch (command.ToLower())
            {
                case "join":
                    HandleJoinCommand(settings, adoptedHero, desiredName, onSuccess, onFailure);
                    break;
                case "rebel":
                    HandleRebelCommand(settings, adoptedHero, onSuccess, onFailure);
                    break;
                case "stats":
                    HandleStatsCommand(settings, adoptedHero, onSuccess, onFailure);
                    break;
                default:
                    onFailure("{=wkhZ6q7d}Invalid or empty kingdom action, try <join/rebel/stats>".Translate());
                    break;
            }
        }

        private void HandleJoinCommand(Settings settings, Hero adoptedHero, string desiredName, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.JoinEnabled)
            {
                onFailure("{=wkhZ6q7d}Joining kingdoms is disabled".Translate());
                return;
            }
            if (adoptedHero.Clan.Kingdom != null)
            {
                onFailure("{=wkhZ6q7d}Your clan is already in a kingdom, in order to leave you must rebel against them".Translate());
                return;
            }
            if (adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You cannot manage your kingdom, as you are not your clans leader!".Translate());
                return;
            }
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                onFailure("{=wkhZ6q7d}(join) (kingdom name)".Translate());
                return;
            }

            var desiredKingdom = CampaignHelpers.AllHeroes.Select(h => h.Clan.Kingdom).Distinct().FirstOrDefault(c => c?.Name.ToString().Equals(desiredName, StringComparison.OrdinalIgnoreCase) == true);
            if (desiredKingdom == null)
            {
                onFailure("{=CVcUtTWc}Could not find the kingdom with the name {name}".Translate(("name", desiredName)));
                return;
            }
            if (desiredKingdom.Clans.Count >= settings.JoinMaxClans)
            {
                onFailure("{=CVcUtTWc}The kingdom {name} is full".Translate(("name", desiredName)));
                return;
            }
            if (desiredKingdom == Hero.MainHero.Clan.Kingdom && !settings.JoinAllowPlayer)
            {
                onFailure("{=CVcUtTWc}Joining the players kingdom is disabled".Translate());
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.JoinPrice)
            {
                onFailure("{=CVcUtTWc}You do not have enough gold ({price}) to join a kingdom".Translate(("price", settings.JoinPrice.ToString())));
                return;
            }

            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.JoinPrice, true);
            ChangeKingdomAction.ApplyByJoinToKingdom(adoptedHero.Clan, desiredKingdom);
            onSuccess("{=CVcUtTWc}Your clan {clanName} has joined the kingom {kingdomName}".Translate(("clanName", adoptedHero.Clan.Name.ToString()), ("kingdomName", adoptedHero.Clan.Kingdom.Name.ToString())));
            Log.ShowInformation("{=K7nuJVCN}{clanName} has joined kingdom {kingdomName}!".Translate(("clanName", adoptedHero.Clan.Name.ToString()), ("kingdomName", adoptedHero.Clan.Kingdom.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
        }

        private void HandleRebelCommand(Settings settings, Hero adoptedHero, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.RebelEnabled)
            {
                onFailure("{=wkhZ6q7d}Clan rebellion is disabled".Translate());
                return;
            }
            if (adoptedHero.Clan.Kingdom == null)
            {
                onFailure("{=wkhZ6q7d}Your clan is not in a kingdom".Translate());
                return;
            }
            if (adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You cannot lead a rebellion agaisnt your kingdom, as you are not your clans leader!".Translate());
                return;
            }
            if (adoptedHero.Clan == adoptedHero.Clan.Kingdom.RulingClan)
            {
                onFailure("{=ve1C1aCl}You already are the ruling clan".Translate());
                return;
            }
            if (adoptedHero.Clan.Tier < settings.RebelClanTierMinimum)
            {
                onFailure("{=ve1C1aCl}Your clan is not high enough tier to rebel".Translate());
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.RebelPrice)
            {
                onFailure("{=ve1C1aCl}You do not have enough gold ({price}) to start a rebellion".Translate(("price", settings.RebelPrice.ToString())));
                return;
            }
            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.RebelPrice, true);
            IFaction oldBoss = adoptedHero.Clan.Kingdom;
            adoptedHero.Clan.ClanLeaveKingdom();
            DeclareWarAction.ApplyByRebellion(adoptedHero.Clan, oldBoss);
            FactionManager.DeclareWar(adoptedHero.Clan, oldBoss);
            onSuccess("{=I4JTuTy3}Your clan has rebelled against {oldBoss} and declared war".Translate(("oldBoss", oldBoss)));
            return;
        }

        private void HandleStatsCommand(Settings settings, Hero adoptedHero, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.StatsEnabled)
            {
                onFailure("{=wkhZ6q7d}Kingdom stats is disabled".Translate());
                return;
            }
            if (adoptedHero.Clan.Kingdom == null)
            {
                onFailure("{=wkhZ6q7d}Your clan is not in a kingdom".Translate());
                return;
            }

            var clanStats = new StringBuilder();
            clanStats.Append("{=wkhZ6q7d}Kingdom Name: {name} | ".Translate(("name", adoptedHero.Clan.Kingdom.Name.ToString())));
            clanStats.Append("{=wkhZ6q7d}Ruling Clan: {rulingClan} | ".Translate(("rulingClan", adoptedHero.Clan.Kingdom.RulingClan.Name.ToString())));
            clanStats.Append("{=wkhZ6q7d}Clan Count: {clanCount} | ".Translate(("clanCount", adoptedHero.Clan.Kingdom.Clans.Count.ToString())));
            clanStats.Append("{=wkhZ6q7d}Strength: {strength} | ".Translate(("strength", Math.Round(adoptedHero.Clan.Kingdom.TotalStrength).ToString())));
            onSuccess("{=wkhZ6q7d}{stats}".Translate(("stats", clanStats.ToString())));
        }
    }
}
