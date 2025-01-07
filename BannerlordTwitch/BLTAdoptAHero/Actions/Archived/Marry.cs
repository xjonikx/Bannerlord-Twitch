//using System;
//using System.Linq;
//using System.Text;
//using BannerlordTwitch;
//using BannerlordTwitch.Helpers;
//using BannerlordTwitch.Localization;
//using BannerlordTwitch.Rewards;
//using BannerlordTwitch.Util;
//using BLTAdoptAHero.Annotations;
//using TaleWorlds.CampaignSystem;
//using TaleWorlds.MountAndBlade;
//using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
//using TaleWorlds.CampaignSystem.Settlements;
//using TaleWorlds.Core;
//using TaleWorlds.Localization;
//using TaleWorlds.Library;

//namespace BLTAdoptAHero.Actions
//{
//    [LocDisplayName("{=NkZXnSQI}Marriage"),
//     LocDescription("{=fd7G5N0Q}Allow adopted heroes to marry"),
//     UsedImplicitly]
//    public class Marriage : ActionHandlerBase
//    {
//        [CategoryOrder("General", 0)]
//        private class Settings : IDocumentable
//        {
//            [LocDisplayName("{=TLrDxhlh}Enabled"),
//             LocCategory("General", "{=C5T5nnix}General"),
//             LocDescription("{=F1KDzuZZ}Enable marriage"),
//             PropertyOrder(1), UsedImplicitly]
//            public bool MarriageEnabled { get; set; } = true;

//            public void GenerateDocumentation(IDocumentationGenerator generator)
//            {
//                generator.PropertyValuePair("MarriageEnabled".Translate(), $"{MarriageEnabled}");
//            }
//        }

//        protected override Type ConfigType => typeof(Settings);

//        protected override void ExecuteInternal(ReplyContext context, object config, Action<string> onSuccess, Action<string> onFailure)
//        {
//            if (config is not Settings settings) return;
//            var adoptedHero = BLTAdoptAHeroCampaignBehavior.Current.GetAdoptedHero(context.UserName);
//            if (adoptedHero == null)
//            {
//                onFailure(AdoptAHero.NoHeroMessage);
//                return;
//            }
//            if (!settings.MarriageEnabled)
//            {
//                onFailure("Marriage is not enabled");
//                return;
//            }
//            if(BLTAdoptAHeroCampaignBehavior.Current.GetIsCreatedHero(adoptedHero))
//            {
//                onFailure("You cannot marry as a created hero");
//                return;
//            }
//            var splitArgs = context.Args.Split(' ');
//            var command = splitArgs[0];
//            if (string.Equals(command, "female", StringComparison.CurrentCultureIgnoreCase))
//            {
//                onSuccess("{=wkhZ6q7d}You have changed your gender to female".Translate());
//                adoptedHero.UpdatePlayerGender(true);
//                return;
//            }
//            else if (string.Equals(command, "male", StringComparison.CurrentCultureIgnoreCase))
//            {
//                onSuccess("{=wkhZ6q7d}You have changed your gender to male".Translate());
//                adoptedHero.UpdatePlayerGender(false);
//                return;
//            }
//            onFailure("{=wkhZ6q7d}Invalid entry".Translate());
//            return;
//        }
//    }
//}