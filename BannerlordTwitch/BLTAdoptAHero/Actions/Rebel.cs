using System;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace BLTAdoptAHero
{
    [LocDisplayName("{=taztq4PM}Rebel"),
     LocDescription("{=jNfiU6Bt}Rebel against your lord"), 
     UsedImplicitly]
    public class Rebel : ActionHandlerBase
    {

        public class Settings : IDocumentable
        {
            [LocDisplayName("{=Whn2gXwU}Price"),
             LocDescription("{=sUlA28HE}The price of the rebellion"), 
             PropertyOrder(1), ExpandableObject, Expand, UsedImplicitly]
            public int Price { get; set; } = 0;

            [LocDisplayName("{=EmyPYxym}OnlyAgainstPlayer"),
             LocDescription("{=mcXKSmQu}If the rebellion can only occur against the player."), 
             PropertyOrder(2), UsedImplicitly]
            public bool OnlyAgainstPlayer { get; set; } = false;
            
            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("{=EmyPYxym}OnlyAgainstPlayer".Translate(), $"{OnlyAgainstPlayer}");
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

            if (settings.OnlyAgainstPlayer)
            {
                if (Clan.PlayerClan.Kingdom == null)
                {
                    onFailure("{=CncsL1ix}You cannot rebel because the player is not in a kingdom".Translate());
                    return;
                }
                if (adoptedHero.Clan.Kingdom.RulingClan.Leader != Hero.MainHero)
                {
                    onFailure("{=KuvT6DAO}Cannot rebel while not under the player kingdom".Translate());
                    return;
                }
                
                
                if ( Clan.PlayerClan.Kingdom.RulingClan != Clan.PlayerClan)
                {
                    onFailure ("{=dG1tCXE4}Can't declare war the player is not the ruler of his kingdom or is not in a kingdom".Translate());
                    return;
                }

            }
            
            if (adoptedHero.Clan == null)
            {
                onFailure("{=Xa83dxVg}Wanderers cannot rebel".Translate());
                return;
            }

            if (adoptedHero.Clan.Kingdom == null)
            {
                onFailure("{=rnI1oCBk}Cannot rebel you are not in a kingdom".Translate());
                return;
                
            }

            if (adoptedHero.Clan.Leader != adoptedHero)
            {
                onFailure("{=sXTHqwAg}Cannot rebel you are not the leader of your clan".Translate());
                return;
            }

            if (adoptedHero.Clan == adoptedHero.Clan.Kingdom.RulingClan)
            {
                onFailure("{=ve1C1aCl}You already are the ruling clan".Translate());
                return;
            }
            
            ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(adoptedHero.Clan);
 
            IFaction oldBoss = adoptedHero.Clan.Kingdom;
            adoptedHero.Clan.ClanLeaveKingdom();
            DeclareWarAction.ApplyByRebellion(adoptedHero.Clan,oldBoss);
            //FactionManager.DeclareWar(adoptedHero.Clan,oldBoss);
            onSuccess("{=I4JTuTy3}You separated from {oldBoss} and declared war to his Kingdom".Translate(("oldBoss",oldBoss)));
        }
    }
}