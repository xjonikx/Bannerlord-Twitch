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
    [LocDisplayName("{=NkZXnSQI}Hero Features"),
     LocDescription("{=fd7G5N0Q}Allow viewer to adjust characteristics about their Hero"),
     UsedImplicitly]
    public class HeroFeatures : HeroCommandHandlerBase
    {
        [CategoryOrder("General", 0)]
        private class Settings : IDocumentable
        {
            [LocDisplayName("{=TLrDxhlh}Gender Swap"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Enable ability to swap gender"),
             PropertyOrder(1), UsedImplicitly]
            public bool GenderEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Only on created heroes?"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Only allow swapping gender for heroes that are created, instead of adopted"),
             PropertyOrder(1), UsedImplicitly]
            public bool GenderDisabledonNative { get; set; } = true;

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Gender Swap Enabled".Translate(), $"{GenderEnabled}");
                generator.PropertyValuePair("Only on created heroes?".Translate(), $"{GenderDisabledonNative}");
            }
        }

        public override Type HandlerConfigType => typeof(Settings);

        protected override void ExecuteInternal(Hero adoptedHero,ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
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
                onFailure("{=wkhZ6q7d}You cannot manage your clan, as you are a prisoner!".Translate());
                return;
            }
            var splitArgs = context.Args.Split(' ');
            var command = splitArgs[0];
            switch (command.ToLower())
            {
                case ("gender"):
                    if (!settings.GenderEnabled)
                    {
                        onFailure("Gender swapping is not enabled");
                        return;
                    }
                    if (!BLTAdoptAHeroCampaignBehavior.Current.GetIsCreatedHero(adoptedHero) && settings.GenderDisabledonNative)
                    {
                        onFailure("Swapping gender is only enabled for created heroes");
                        return;
                    }
                    if (string.Equals(splitArgs[1].ToLower(), "female", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (adoptedHero.IsFemale)
                        {
                            onFailure("{=wkhZ6q7d}You are already female".Translate());
                            return;
                        }
                        onSuccess("{=wkhZ6q7d}You have changed your gender to female".Translate());
                        adoptedHero.UpdatePlayerGender(true);
                        Log.ShowInformation(
                            "{=K7nuJVCN}{Name} has changed their gender to female!".Translate(("Name", adoptedHero.Name)),
                            adoptedHero.CharacterObject);
                        return;
                    }
                    else if (string.Equals(splitArgs[1].ToLower(), "male", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!adoptedHero.IsFemale)
                        {
                            onFailure("{=wkhZ6q7d}You are already male".Translate());
                            return;
                        }
                        onSuccess("{=wkhZ6q7d}You have changed your gender to male".Translate());
                        adoptedHero.UpdatePlayerGender(false);
                        Log.ShowInformation(
                            "{=K7nuJVCN}{Name} has changed their gender to male!".Translate(("Name", adoptedHero.Name)),
                            adoptedHero.CharacterObject);
                        return;
                    }
                    onFailure("{=wkhZ6q7d}Invalid gender <male/female>".Translate());
                    return;
                default:
                    onFailure("{=wkhZ6q7d}Invalid action".Translate());
                    return;
            }
        }
    }
}
