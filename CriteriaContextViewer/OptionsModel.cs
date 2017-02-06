using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriteriaContextViewer
{
    public class OptionsModel
    {
        public DBSettingsModel DBSettingsModel { get; set; }
        public bool UseDungeonEncounter { get; set; }
        public bool UseItems { get; set; }
        public bool UseSpells { get; set; }
        public bool VerboseCriteriaTree { get; set; }
        public bool UseCreatureNames { get; set; }
        public bool UseGameobjectNames { get; set; }
    }
}
