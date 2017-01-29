using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriteriaContextViewer.Utils.Misc
{
    public class WowheadUtils
    {
        public static List<string> WowheadLocaleMapper = new List<string>
        {
            "de",
            "es",
            "fr",
            "it",
            "pt",
            "ru",
            "ko",
            "cn"
        };

        public static string GetWowheadURL() => GetWowheadURL(CultureInfo.CurrentCulture, false);

        public static string GetWowheadURL(CultureInfo culture, bool forceUseCulture)
        {
            CultureInfo currentCulture = culture ?? CultureInfo.CurrentCulture;
            string domain = "www";
            if (forceUseCulture || WowheadLocaleMapper.Contains(currentCulture.TwoLetterISOLanguageName))
                domain = currentCulture.TwoLetterISOLanguageName;

            return $"{domain}.wowhead.com";
        }

        public static string GetWowheadDataSectionUrl(WowheadDataSection section, int value) => Path.Combine(GetWowheadURL(), $"{section.ToString().ToLower()}={value}");

        public static string GetWowheadURLForSpell(int spellId)
            => GetWowheadDataSectionUrl(WowheadDataSection.Spell, spellId);

        public static string GetWowheadURLForCreature(int creatureId)
            => GetWowheadDataSectionUrl(WowheadDataSection.Creature, creatureId);

        public static string GetWowheadURLForQuest(int questId)
            => GetWowheadDataSectionUrl(WowheadDataSection.Quest, questId);

        public static string GetWowheadURLForAchievement(int achievementId)
            => GetWowheadDataSectionUrl(WowheadDataSection.Achievement, achievementId);
    }

    public enum WowheadDataSection
    {
        Spell,
        Creature,
        Quest,
        Achievement
    }
}
