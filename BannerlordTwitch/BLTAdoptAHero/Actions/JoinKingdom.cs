using System;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace BLTAdoptAHero
{
    [LocDisplayName("{=}Join Kingdom"),
     LocDescription("{=}Join a Kingdom"), 
     UsedImplicitly]
    public class JoinKingdom : ActionHandlerBase
    {

        public class Settings : IDocumentable
        {
            [LocDisplayName("{=Whn2gXwU}Price"),
             LocDescription("{=sUlA28HE}The price of the rebellion"), 
             PropertyOrder(1), ExpandableObject, Expand, UsedImplicitly]
            public int Price { get; set; } = 0;
            
            
            [LocDisplayName("{=vQmXPUc2}UserChooseFaction"),
             LocDescription("{=KjzB7Rvm}Is the user able to choose the faction he want to join if disabled default faction is player faction"), 
             PropertyOrder(2), ExpandableObject, Expand, UsedImplicitly]
            public bool UserChooseFaction { get; set; }

            [LocDisplayName("{=pR9CDiLB}OnlyIndependants"),
             LocDescription("{=f1rBB354}Can heroes already in a kingdom leave it to join the new kingdom"), 
             PropertyOrder(3), ExpandableObject, Expand, UsedImplicitly]
            public bool OnlyIndependants { get; set; }


            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
               generator.PropertyValuePair("{=Whn2gXwU}Price".Translate(), $"{Price}");
                
            }
        }
        
        protected override Type ConfigType => typeof(Settings);


        protected override void ExecuteInternal(ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
        {
            var settings = (Settings)config;
            var adoptedHero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
            
            if (adoptedHero == null)
            {
                onFailure(AdoptAHero.NoHeroMessage);
                return;
            }

            if (adoptedHero.Gold < settings.Price)
            {
                onFailure("{=Z4vYZzSq}Not enough gold !".Translate());
                return;
            }


            if (!adoptedHero.IsClanLeader)
            {
                onFailure("{=XmRVpiun}You are not the leader of your clan");
                return;
            }

            Kingdom desiredFaction = null;

            if (settings.UserChooseFaction)
            {
                desiredFaction = Kingdom.All.Find(c => c.Name.ToString().Equals(context.Args));
            }else {
                desiredFaction = Kingdom.All.Find(k=>k.RulingClan==Clan.PlayerClan);
            }

            if (desiredFaction == null)
            {
                onFailure("{=rnjTaxtu}Could not find the kingdom or player is not the ruler of a kingdom".Translate());
                return;
            }

            if (adoptedHero.Clan == null)
            {
                onFailure("{=LCIqZfrG}You are a wanderer you cannot join a kingdom".Translate());
                return;
            }

            if (settings.OnlyIndependants && !adoptedHero.Clan.IsMapFaction)
            {
                onFailure("{=X7JOfZyK}OnlyIndependants is enabled you cannot leave your kingdom to join another".Translate());
                return;
            }
            

            ChangeKingdomAction.ApplyByJoinToKingdom(adoptedHero.Clan, desiredFaction);
            onSuccess("{=5X6Dfjzp}You are now a vassal of the {king} kingdom".Translate(("king",desiredFaction.Name.ToString())));
        }
    }
}