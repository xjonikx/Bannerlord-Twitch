using System;
using System.Linq;
using System.Text;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.Library;

namespace BLTAdoptAHero.Actions
{
    [LocDisplayName("{=NkZXnSQI}Clan Management"),
     LocDescription("{=fd7G5N0Q}Allow viewer to change their clan or make leader decisions"),
     UsedImplicitly]
    public class ClanManagement : ActionHandlerBase
    {
        [CategoryOrder("Join", 0),
         CategoryOrder("Create", 1),
         CategoryOrder("Lead", 2),
         CategoryOrder("Rename", 3),
         CategoryOrder("Stats", 4)]
        private class Settings : IDocumentable
        {
            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Enable joining clans command"),
             PropertyOrder(1), UsedImplicitly]
            public bool JoinEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Max Heroes"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Maximum heroes (includes NPC's) before join is disallowed"),
             PropertyOrder(2), UsedImplicitly]
            public int JoinMaxHeroes { get; set; } = 50;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Cost of joining a clan"),
             PropertyOrder(3), UsedImplicitly]
            public int JoinPrice { get; set; } = 150000;

            [LocDisplayName("{=TLrDxhlh}Players Clan?"),
             LocCategory("Join", "{=C5T5nnix}Join"),
             LocDescription("{=F1KDzuZZ}Allow viewers to join the players clan"),
             PropertyOrder(4), UsedImplicitly]
            public bool JoinAllowPlayer { get; set; } = false;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Create", "{=C5T5nnix}Create"),
             LocDescription("{=F1KDzuZZ}Enable creating clans command"),
             PropertyOrder(1), UsedImplicitly]
            public bool CreateEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Create", "{=C5T5nnix}Create"),
             LocDescription("{=F1KDzuZZ}Cost of creating a clan"),
             PropertyOrder(2), UsedImplicitly]
            public int CreatePrice { get; set; } = 2500000;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Lead", "{=C5T5nnix}Lead"),
             LocDescription("{=F1KDzuZZ}Enable leading clans command"),
             PropertyOrder(1), UsedImplicitly]
            public bool LeadEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Lead", "{=C5T5nnix}Lead"),
             LocDescription("{=F1KDzuZZ}Cost of leading a clan"),
             PropertyOrder(2), UsedImplicitly]
            public int LeadPrice { get; set; } = 1000000;

            [LocDisplayName("{=TLrDxhlh}Challenge Heroes"),
             LocCategory("Lead", "{=C5T5nnix}Lead"),
             LocDescription("{=F1KDzuZZ}Toggle whether or not trying to lead a clan already led by a BLT hero is possible - random chance they win based on skill difference"),
             PropertyOrder(3), UsedImplicitly]
            public bool LeadChallengeHeroes { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Rename", "{=C5T5nnix}Rename"),
             LocDescription("{=F1KDzuZZ}Enable renaming clans command"),
             PropertyOrder(1), UsedImplicitly]
            public bool RenameEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Gold Cost"),
             LocCategory("Rename", "{=C5T5nnix}Rename"),
             LocDescription("{=F1KDzuZZ}Cost of renaming a clan"),
             PropertyOrder(2), UsedImplicitly]
            public int RenamePrice { get; set; } = 1000000;

            [LocDisplayName("{=TLrDxhlh}Enabled"),
             LocCategory("Stats", "{=C5T5nnix}Stats"),
             LocDescription("{=F1KDzuZZ}Enable stats command"),
             PropertyOrder(1), UsedImplicitly]
            public bool StatsEnabled { get; set; } = true;

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Join Enabled".Translate(), $"{JoinEnabled}");
                generator.PropertyValuePair("Max Heroes".Translate(), $"{JoinMaxHeroes}");
                generator.PropertyValuePair("Join Price".Translate(), $"{JoinPrice}");
                generator.PropertyValuePair("Allow Join Players Clan?".Translate(), $"{JoinAllowPlayer}");
                generator.PropertyValuePair("Create Enabled".Translate(), $"{CreateEnabled}");
                generator.PropertyValuePair("Create Price".Translate(), $"{CreatePrice}");
                generator.PropertyValuePair("Lead Enabled".Translate(), $"{LeadEnabled}");
                generator.PropertyValuePair("Lead Price".Translate(), $"{LeadPrice}");
                generator.PropertyValuePair("Lead Challenge Heroes?".Translate(), $"{LeadChallengeHeroes}");
                generator.PropertyValuePair("Rename Enabled".Translate(), $"{RenameEnabled}");
                generator.PropertyValuePair("Rename Price".Translate(), $"{RenamePrice}");
                generator.PropertyValuePair("Stats Enabled".Translate(), $"{StatsEnabled}");
            }
        }
        protected override Type ConfigType => typeof(Settings);

        protected override void ExecuteInternal(ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
        {
            if (config is not Settings settings) return;
            var adoptedHero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
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
                onFailure("{=wkhZ6q7d}You cannot manage your clan, as you are a prisoner!".Translate());
                return;
            }

            if (context.Args.IsEmpty())
            {
                if (adoptedHero.Clan == null)
                {
                    onFailure("{=wkhZ6q7d}You are not in a clan".Translate());
                    return;
                }
                onSuccess("{=wkhZ6q7d}Your clan is {clanName}".Translate(("clanName", adoptedHero.Clan.Name.ToString())));
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
                case "create":
                    HandleCreateCommand(settings, adoptedHero, desiredName, onSuccess, onFailure);
                    break;
                case "lead":
                    HandleLeadCommand(settings, adoptedHero, onSuccess, onFailure);
                    break;
                case "rename":
                    HandleRenameCommand(settings, adoptedHero, desiredName, onSuccess, onFailure);
                    break;
                case "stats":
                    HandleStatsCommand(settings, adoptedHero, onSuccess, onFailure);
                    break;
                default:
                    onFailure("{=wkhZ6q7d}Invalid or empty clan action, try <join/create/lead/rename/stats>".Translate());
                    break;
            }
        }

        private void HandleJoinCommand(Settings settings, Hero adoptedHero, string desiredName, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.JoinEnabled)
            {
                onFailure("{=wkhZ6q7d}Joining clans is disabled".Translate());
                return;
            }
            if (adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You cannot join another clan as you are the leader of your clan".Translate());
                return;
            }
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                onFailure("{=wkhZ6q7d}(join) (clan name)".Translate());
                return;
            }

            var desiredClan = CampaignHelpers.AllHeroes.Select(h => h.Clan).Distinct().FirstOrDefault(c => c?.Name.ToString().Equals(desiredName, StringComparison.OrdinalIgnoreCase) == true);
            if (desiredClan == null)
            {
                onFailure("{=CVcUtTWc}Could not find the clan with the name {name}".Translate(("name", desiredName)));
                return;
            }
            if(desiredClan.Heroes.Count >= settings.JoinMaxHeroes)
            {
                onFailure("{=CVcUtTWc}The clan {name} is full".Translate(("name", desiredName)));
                return;
            }
            if (desiredClan == Hero.MainHero.Clan && !settings.JoinAllowPlayer)
            {
                onFailure("{=CVcUtTWc}Joining the players clan is disabled".Translate());
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.JoinPrice)
            {
                onFailure("{=CVcUtTWc}You do not have enough gold ({price}) to join a clan".Translate(("price", settings.JoinPrice.ToString())));
                return;
            }

            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.JoinPrice, true);
            adoptedHero.Clan = desiredClan;
            onSuccess("{=CVcUtTWc}Joined clan {name}".Translate(("name", desiredName)));
            Log.ShowInformation("{=K7nuJVCN}{heroName} has joined clan {clanName}!".Translate(("heroName", adoptedHero.Name.ToString()), ("clanName", adoptedHero.Clan.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
        }

        private void HandleCreateCommand(Settings settings, Hero adoptedHero, string desiredName, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.CreateEnabled)
            {
                onFailure("{=wkhZ6q7d}Creating clans is disabled".Translate());
                return;
            }
            if (adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You cannot create another clan as you are the leader of your clan".Translate());
                return;
            }
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                onFailure("{=wkhZ6q7d}(create) (clan name)".Translate());
                return;
            }

            var fullClanName = $"[BLT Clan] {desiredName}";
            var existingClan = CampaignHelpers.AllHeroes.Select(h => h.Clan).Distinct().FirstOrDefault(c => c?.Name.ToString().Equals(fullClanName, StringComparison.OrdinalIgnoreCase) == true);
            if (existingClan != null)
            {
                onFailure("{=CVcUtTWc}A clan with the name {name} already exists".Translate(("name", desiredName)));
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.CreatePrice)
            {
                onFailure("{=CVcUtTWc}You do not have enough gold ({price}) to create a clan".Translate(("price", settings.CreatePrice.ToString())));
                return;
            }

            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.CreatePrice, true);
            var newClan = Clan.CreateClan(fullClanName);
            var clanCulture = adoptedHero.Culture;
            var clanBanner = Banner.CreateRandomBanner();
            newClan.InitializeClan(new TextObject(fullClanName), new TextObject(fullClanName), clanCulture, clanBanner);
            newClan.UpdateHomeSettlement(Settlement.All.SelectRandom());
            adoptedHero.Clan = newClan;
            newClan.SetLeader(adoptedHero);
            onSuccess("{=CVcUtTWc}Created and leading clan {name}".Translate(("name", fullClanName)));
            Log.ShowInformation("{=K7nuJVCN}{heroName} has created and is leading clan {clanName}!".Translate(("heroName", adoptedHero.Name.ToString()), ("clanName", adoptedHero.Clan.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
        }

        private void HandleLeadCommand(Settings settings, Hero adoptedHero, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.LeadEnabled)
            {
                onFailure("{=wkhZ6q7d}Leading clans is disabled".Translate());
                return;
            }
            if (adoptedHero.Clan == null)
            {
                onFailure("{=wkhZ6q7d}You are not in a clan".Translate());
                return;
            }
            if (adoptedHero.Clan == Hero.MainHero.Clan)
            {
                onFailure("{=wkhZ6q7d}You cannot lead the players clan".Translate());
                return;
            }
            if (adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You are already the leader of your clan".Translate());
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.LeadPrice)
            {
                onFailure("{=CVcUtTWc}You do not have enough gold ({price}) to lead a clan".Translate(("price", settings.LeadPrice.ToString())));
                return;
            }
            if (adoptedHero.Clan.Leader.Name.Contains(BLTAdoptAHeroModule.Tag))
            {
                if (!settings.LeadEnabled)
                {
                    onFailure("{=wkhZ6q7d}Leading clans led by other BLT Heroes is disabled".Translate());
                    return;
                }
                Hero oldLeader = adoptedHero.Clan.Leader;
                if (MBRandom.RandomInt(0, 10) < MBMath.ClampInt(oldLeader.Level - adoptedHero.Level, 0, 10))
                {
                    BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.LeadPrice, true);
                    onFailure("{=wkhZ6q7d}You have been bested in battle by {oldLeader} and failed to lead your clan.".Translate(("oldLeader", oldLeader.Name.ToString())));
                    return;
                }
                else
                {
                    BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.LeadPrice, true);
                    adoptedHero.Clan.SetLeader(adoptedHero);
                    Log.ShowInformation("{=K7nuJVCN}{heroName} has usurped {oldLeader} and is now leading clan {clanName}!".Translate(("heroName", adoptedHero.Name.ToString()), ("oldLeader", oldLeader.Name.ToString()), ("clanName", adoptedHero.Clan.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
                    onSuccess("{=wkhZ6q7d}You have successfully taken over the leadership of your clan".Translate());
                    return;
                }
            }
            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.LeadPrice, true);
            adoptedHero.Clan.SetLeader(adoptedHero);
            onSuccess("{=wkhZ6q7d}You are now the leader of your clan".Translate());
            Log.ShowInformation("{=K7nuJVCN}{heroName} is now leading clan {clanName}!".Translate(("heroName", adoptedHero.Name.ToString()), ("clanName", adoptedHero.Clan.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
        }

        private void HandleRenameCommand(Settings settings, Hero adoptedHero, string desiredName, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.RenameEnabled)
            {
                onFailure("{=wkhZ6q7d}Renaming clans is disabled".Translate());
                return;
            }
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                onFailure("{=wkhZ6q7d}(rename) (clan name)".Translate());
                return;
            }
            if (!adoptedHero.IsClanLeader)
            {
                onFailure("{=wkhZ6q7d}You are not the leader of your clan".Translate());
                return;
            }
            if (BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero) < settings.RenamePrice)
            {
                onFailure("{=CVcUtTWc}You do not have enough gold ({price}) to rename a clan".Translate(("price", settings.RenamePrice.ToString())));
                return;
            }

            BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.RenamePrice, true);
            var fullClanName = $"[BLT Clan] {desiredName}";
            var oldName = adoptedHero.Clan.Name.ToString();
            adoptedHero.Clan.ChangeClanName(new TextObject(fullClanName), new TextObject(fullClanName));
            onSuccess("{=wkhZ6q7d}Renamed clan to {name}".Translate(("name", fullClanName)));
            Log.ShowInformation("{=K7nuJVCN}{heroName} has renamed clan {oldName} to {clanName}!".Translate(("heroName", adoptedHero.Name.ToString()), ("oldName", oldName), ("clanName", adoptedHero.Clan.Name.ToString())), adoptedHero.CharacterObject, Log.Sound.Horns2);
        }

        private void HandleStatsCommand(Settings settings, Hero adoptedHero, Action<string> onSuccess, Action<string> onFailure)
        {
            if (!settings.StatsEnabled)
            {
                onFailure("{=wkhZ6q7d}Clan stats is disabled".Translate());
                return;
            }
            if (adoptedHero.Clan == null)
            {
                onFailure("{=wkhZ6q7d}You are not in a clan".Translate());
                return;
            }

            var clanStats = new StringBuilder();
            clanStats.Append("{=wkhZ6q7d}Clan Name: {name} | ".Translate(("name", adoptedHero.Clan.Name.ToString())));
            clanStats.Append("{=wkhZ6q7d}Leader: {leader} | ".Translate(("leader", adoptedHero.Clan.Leader.Name.ToString())));
            clanStats.Append("{=wkhZ6q7d}Kingdom: {kingdom} | ".Translate(("kingdom", adoptedHero.Clan.Kingdom.Name.ToString())));
            clanStats.Append("{=wkhZ6q7d}Tier: {tier} | ".Translate(("tier", adoptedHero.Clan.Tier.ToString())));
            clanStats.Append("{=wkhZ6q7d}Strength: {strength} | ".Translate(("strength", Math.Round(adoptedHero.Clan.TotalStrength).ToString())));
            clanStats.Append("{=wkhZ6q7d}Members: {members}".Translate(("members", adoptedHero.Clan.Heroes.Count.ToString())));
            onSuccess("{=wkhZ6q7d}{stats}".Translate(("stats", clanStats.ToString())));
        }
    }
}
