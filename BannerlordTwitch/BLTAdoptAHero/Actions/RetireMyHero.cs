using System;
using BannerlordTwitch;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.Util;
using JetBrains.Annotations;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace BLTAdoptAHero
{
    [LocDisplayName("{=sZhEiXyn}Retire Hero"),
     LocDescription("{=Eh0m4R54}Retires the hero, allowing the viewer to adopt another"),
     UsedImplicitly]
    public class RetireMyHero : ActionHandlerBase
    {
        private class Settings : IDocumentable
        {
            [LocDisplayName("{=TLrDxhlh}Only Created Heroes?"),
             LocDescription("{=F1KDzuZZ}Only allow created heroes to retire"),
             PropertyOrder(1), UsedImplicitly]
            public bool RetireCreatedOnly { get; set; } = true;

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Only Created Heroes?".Translate(), $"{RetireCreatedOnly}");
            }
        }

        protected override Type ConfigType => typeof(Settings);
        protected override void ExecuteInternal(ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
        {
            if (config is not Settings settings)
            {
                onFailure("Invalid config");
                return;
            }
            if (context.Args != "{=xSbB2Zw5}yes".Translate())
            {
                onFailure("{=0qVpLYfb}You must enter '{Prompt}' at the prompt to retire your hero"
                    .Translate(("Prompt", "{=xSbB2Zw5}yes".Translate())));
                return;
            }
            var adoptedHero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
            if (adoptedHero == null)
            {
                onFailure(AdoptAHero.NoHeroMessage);
                return;
            }
            if (Mission.Current != null)
            {
                onFailure("{=po20lHyz}You cannot retire your hero, as a mission is active!".Translate());
                return;
            }
            if (settings.RetireCreatedOnly && !BLTAdoptAHeroCampaignBehavior.Current.GetIsCreatedHero(adoptedHero))
            {
                onFailure("{=ihG4fs1r}You can only retire created heroes".Translate());
                return;
            }
            Log.ShowInformation("{=ihG4fs1r}{Name} has retired!"
                .Translate(("Name", adoptedHero.Name)), adoptedHero.CharacterObject, Log.Sound.Horns3);
            BLTAdoptAHeroCampaignBehavior.Current.RetireHero(adoptedHero);
        }
    }
}