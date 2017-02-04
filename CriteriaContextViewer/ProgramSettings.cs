using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriteriaContextViewer
{
    public static class ProgramSettings
    {
        public static bool UseDungeonEncounters
        {
            get
            {
                return Properties.Settings.Default.UseDungeonEncounters;
            }
            set
            {
                Properties.Settings.Default.UseDungeonEncounters = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool UseItems
        {
            get
            {
                return Properties.Settings.Default.UseItems;
            }
            set
            {
                Properties.Settings.Default.UseItems = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool VerboseCriteriaTree
        {
            get { return Properties.Settings.Default.VerboseCriteriaTree; }
            set
            {
                Properties.Settings.Default.VerboseCriteriaTree = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
