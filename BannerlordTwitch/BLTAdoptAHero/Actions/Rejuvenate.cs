using System;
using BannerlordTwitch;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace BLTAdoptAHero
{
    [LocDisplayName("{=YDcnEEbS}Rejuvenate"),
     LocDescription("{=22nn0uG5}Rejuvenate your hero"), 
     UsedImplicitly]
    public class Rejuvenate : ActionHandlerBase
    {

        public class Settings : IDocumentable
        {
            [LocDisplayName("{=7WIjNgF2}Price"),
             LocDescription("{=QaK58Z3j}The price of the rejuvenation"), 
             PropertyOrder(1), ExpandableObject, Expand, UsedImplicitly]
            public int Price { get; set; } = 0;

            [LocDisplayName("{=eyrNUsxM}Age"),
             LocDescription("{=oyzYoByT}The age that will be substracted from the hero."), 
             PropertyOrder(2), UsedImplicitly]
            public int Age { get; set; } = 1;
            
            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Age".Translate(), $"{Age}");
                generator.PropertyValuePair("Price".Translate(), $"{Price}");
                
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
            
            if (Mission.Current != null)
            {
                onFailure("{=wkhZ6q7b}You cannot rejuvenate, as a mission is active!".Translate());
                return;
            }

            int availableGold = BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero);
            if (availableGold < settings.Price)
            {
                onFailure("{=Z4vYZzSq}Not enough gold !".Translate());
                return;
            }

            double newAge = Math.Truncate(adoptedHero.Age - settings.Age);

            if (newAge < Campaign.Current.Models.AgeModel.BecomeChildAge)
            {
                onFailure("{=yWo2v3yu}You cannot rejuvenate bellow child age".Translate());
                return;
            }
            
            adoptedHero.SetBirthDay(adoptedHero.BirthDay + CampaignTime.Years(settings.Age));

            onSuccess("{=XidEZXAO}Your rejuvenated of {Age} years you are now {newAge}".Translate(("Age",settings.Age),("newAge",newAge)));
        }
    }
}