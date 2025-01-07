using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BannerlordTwitch;
using BannerlordTwitch.Annotations;
using BannerlordTwitch.Localization;
using BannerlordTwitch.Rewards;
using BannerlordTwitch.UI;
using BannerlordTwitch.Util;
using BLTAdoptAHero;
using BLTAdoptAHero.Achievements;
using BLTAdoptAHero.Actions.Util;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using YamlDotNet.Serialization;

namespace BLTConfigure
{
    [LocDisplayName("{=vDjnDtoL}Overlay Config")]
    internal class GlobalOverlayConfig : IUpdateFromDefault, IDocumentable, INotifyPropertyChanged
    {
        #region Static
        private const string ID = "BLT Configure - Overlay Config";

        internal static void Register() => ActionManager.RegisterGlobalConfigType(ID, typeof(GlobalOverlayConfig));
        internal static GlobalOverlayConfig Get() => ActionManager.GetGlobalConfig<GlobalOverlayConfig>(ID);
        internal static GlobalOverlayConfig Get(BannerlordTwitch.Settings fromSettings) => fromSettings.GetGlobalConfig<GlobalOverlayConfig>(ID);
        #endregion

        #region User Editable
        #region General
        [LocDisplayName("{=xwcKN7sH}Overlay Heroes List Max Height (Not Functioning)"),
         LocDescription("{=rX68wbfF}Max height in pixels before the hero list starts scrolling"),
         PropertyOrder(1), Document, UsedImplicitly,
         Range(0, 10)]
        public float HeroesListMaxHeight { get; set; } = 1;
        #endregion
        #endregion

        #region Public Interface
        //[YamlIgnore, Browsable(false)]
        //public float DifficultyScalingClamped => MathF.Clamp(DifficultyScaling, 0, 5);

        //[YamlIgnore, Browsable(false)]
        //public IEnumerable<AchievementDef> ValidAchievements => Achievements.Where(a => a.Enabled);

        //public AchievementDef GetAchievement(Guid id) => ValidAchievements?.FirstOrDefault(a => a.ID == id);

        //public float GetCooldownTime(int summoned)
        //   => (float)(Math.Pow(SummonCooldownUseMultiplier, Mathf.Max(0, summoned - 1)) * SummonCooldownInSeconds);
        #endregion

        #region IUpdateFromDefault
        public void OnUpdateFromDefault(BannerlordTwitch.Settings defaultSettings)
        {
            //SettingsHelpers.MergeCollectionsSorted(
            //    KillStreaks,
            //    Get(defaultSettings).KillStreaks,
            //    (a, b) => a.ID == b.ID,
            //    (a, b) => a.KillsRequired.CompareTo(b.KillsRequired)
            //);
            //SettingsHelpers.MergeCollections(
            //    Achievements,
            //    Get(defaultSettings).Achievements,
            //    (a, b) => a.ID == b.ID
            //);
        }
        #endregion

        #region IDocumentable
        public void GenerateDocumentation(IDocumentationGenerator generator)
        {
            //TODO: Implement documentation
            //generator.Div("common-config", () =>
            //{
            //    generator.H1("{=F6vM1OJo}Common Config".Translate());
            //    DocumentationHelpers.AutoDocument(generator, this);

            //    var killStreaks = KillStreaks.Where(k => k.Enabled).ToList();
            //    if (killStreaks.Any())
            //    {
            //        generator.H2("{=3DZYc6hN}Kill Streaks".Translate());
            //        generator.Table("kill-streaks", () =>
            //        {
            //            generator.TR(() => generator
            //                .TH("{=uUzmy7Lh}Name".Translate())
            //                .TH("{=mG7HzT0z}Kills Required".Translate())
            //                .TH("{=sHWjkhId}Reward".Translate())
            //            );
            //            foreach (var k in killStreaks
            //                .OrderBy(k => k.KillsRequired))
            //            {
            //                generator.TR(() =>
            //                    generator.TD(k.Name.ToString()).TD($"{k.KillsRequired}").TD(() =>
            //                    {
            //                        if (k.GoldReward > 0) generator.P($"{k.GoldReward}{Naming.Gold}");
            //                        if (k.XPReward > 0) generator.P($"{k.XPReward}{Naming.XP}");
            //                    }));
            //            }
            //        });
            //    }

            //    var achievements = ValidAchievements.Where(a => a.Enabled).ToList();
            //    if (achievements.Any())
            //    {
            //        generator.H2("{=ZW9XlwY7}Achievements".Translate());
            //        generator.Table("achievements", () =>
            //        {
            //            generator.TR(() => generator
            //                .TH("{=uUzmy7Lh}Name".Translate())
            //                .TH("{=TFbiD0CZ}Requirements".Translate())
            //                .TH("{=sHWjkhId}Reward".Translate())
            //            );
            //            foreach (var a in achievements
            //                .OrderBy(a => a.Name.ToString()))
            //            {
            //                generator.TR(() =>
            //                    generator.TD(a.Name.ToString())
            //                        .TD(() =>
            //                        {
            //                            foreach (var r in a.Requirements)
            //                            {
            //                                // ReSharper disable once SuspiciousTypeConversion.Global
            //                                if (r is IDocumentable d)
            //                                {
            //                                    d.GenerateDocumentation(generator);
            //                                }
            //                                else
            //                                {
            //                                    generator.P(r.ToString());
            //                                }
            //                            }
            //                        })
            //                        .TD(() =>
            //                        {
            //                            if (a.GoldGain > 0) generator.P($"{a.GoldGain}{Naming.Gold}");
            //                            if (a.XPGain > 0) generator.P($"{a.XPGain}{Naming.XP}");
            //                            if (a.GiveItemReward) generator.P($"{Naming.Item}: {a.ItemReward}");
            //                        })
            //                    );
            //            }
            //        });
            //    }
            //});
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}