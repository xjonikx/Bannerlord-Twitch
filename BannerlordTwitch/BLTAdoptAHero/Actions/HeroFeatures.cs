using System;
using System.Linq;
using BannerlordTwitch;
using BannerlordTwitch.Helpers;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Util;
using BLTAdoptAHero.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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
            [LocDisplayName("{=TLrDxhlh}Change Hero Gender Enabled"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Enable ability to change gender"),
             PropertyOrder(1), UsedImplicitly]
            public bool GenderEnabled { get; set; } = true;

            [LocDisplayName("{=TLrDxhlh}Change Hero Gender Gold Cost"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Cost of changing gender"),
             PropertyOrder(2), UsedImplicitly]
            public int GenderCost { get; set; } = 50000;

            [LocDisplayName("{=TLrDxhlh}Only on created heroes?"),
             LocCategory("General", "{=C5T5nnix}General"),
             LocDescription("{=F1KDzuZZ}Only allow swapping gender for heroes that are created, instead of adopted"),
             PropertyOrder(3), UsedImplicitly]
            public bool GenderDisabledonNative { get; set; } = true;

            //[LocDisplayName("{=TLrDxhlh}Hero Marriage Enabled"),
            // LocCategory("General", "{=C5T5nnix}General"),
            // LocDescription("{=F1KDzuZZ}Enable ability for heroes to marry"),
            // PropertyOrder(4), UsedImplicitly]
            //public bool MarriageEnabled { get; set; } = true;

            //[LocDisplayName("{=TLrDxhlh}Hero Marriage Gold Cost"),
            // LocCategory("General", "{=C5T5nnix}General"),
            // LocDescription("{=F1KDzuZZ}Cost of marry action"),
            // PropertyOrder(5), UsedImplicitly]
            //public int MarriageCost { get; set; } = 50000;

            public void GenerateDocumentation(IDocumentationGenerator generator)
            {
                generator.PropertyValuePair("Change Hero Gender Enabled".Translate(), $"{GenderEnabled}");
                generator.PropertyValuePair("Change Hero Gender Gold Cost".Translate(), $"{GenderCost}");
                generator.PropertyValuePair("Only on created heroes?".Translate(), $"{GenderDisabledonNative}");
                //generator.PropertyValuePair("Hero Marriage Enabled".Translate(), $"{MarriageEnabled}");
                //generator.PropertyValuePair("Hero Marriage Gold Cost".Translate(), $"{MarriageCost}");
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
                onFailure("{=wkhZ6q7d}You cannot manage your hero, as a mission is active!".Translate());
                return;
            }
            if (adoptedHero.HeroState == Hero.CharacterStates.Prisoner)
            {
                onFailure("{=wkhZ6q7d}You cannot manage your hero, as you are a prisoner!".Translate());
                return;
            }
            var splitArgs = context.Args.Split(' ');
            var command = splitArgs[0];
            switch (command.ToLower())
            {
                //case ("marry"):
                //    if (!settings.MarriageEnabled)
                //    {
                //        onFailure("{=wkhZ6q7d}Hero marriage is not enabled".Translate());
                //        return;
                //    }
                //    if (adoptedHero.Spouse != null)
                //    {
                //        onFailure("{=wkhZ6q7d}You are already married".Translate());
                //        return;
                //    }
                //    if (settings.MarriageCost > BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero))
                //    {
                //        onFailure("{=ve1C1aCl}You do not have enough gold ({price}) to marry".Translate(("price", settings.GenderCost.ToString())));
                //        return;
                //    }
                //    var spouse = CampaignHelpers.AliveHeroes.Where(n => (n.Name?.Contains(BLTAdoptAHeroModule.Tag) == false) && (n.Spouse == null) && (adoptedHero.IsFemale == n.IsFemale)).SelectRandom();
                //    if(spouse == null)
                //    {
                //        onFailure("{=wkhZ6q7d}No valid spouse found".Translate());
                //        return;
                //    }

                //    BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.GenderCost);
                //    onSuccess("{=wkhZ6q7d}Marriage message".Translate());
                //    return;

                case ("gender"):
                    if (!settings.GenderEnabled)
                    {
                        onFailure("{=wkhZ6q7d}Changing heroes gender is not enabled".Translate());
                        return;
                    }
                    if (!BLTAdoptAHeroCampaignBehavior.Current.GetIsCreatedHero(adoptedHero) && settings.GenderDisabledonNative)
                    {
                        onFailure("{=wkhZ6q7d}Changing heroes gender is only enabled for created heroes".Translate());
                        return;
                    }
                    if (settings.GenderCost > BLTAdoptAHeroCampaignBehavior.Current.GetHeroGold(adoptedHero))
                    {
                        onFailure("{=ve1C1aCl}You do not have enough gold ({price}) to change gender".Translate(("price", settings.GenderCost.ToString())));
                        return;
                    }
                    if (string.Equals(splitArgs[1].ToLower(), "female", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (adoptedHero.IsFemale)
                        {
                            onFailure("{=wkhZ6q7d}Your hero is already female".Translate());
                            return;
                        }
                        onSuccess("{=wkhZ6q7d}Your hero has changed their gender to female".Translate());
                        BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.GenderCost);
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
                            onFailure("{=wkhZ6q7d}Your hero is already male".Translate());
                            return;
                        }
                        onSuccess("{=wkhZ6q7d}Your hero has changed their gender to male".Translate());
                        BLTAdoptAHeroCampaignBehavior.Current.ChangeHeroGold(adoptedHero, -settings.GenderCost);
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
