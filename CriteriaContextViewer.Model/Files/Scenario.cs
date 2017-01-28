using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CriteriaContextViewer.Model.Readers;

namespace CriteriaContextViewer.Model.Files
{
    public class Scenario : IDBObjectReader
    {
        public const string FileName = @"Scenario.db2";

        public uint Id { get; set; }
        public string Name { get; set; }
        public ushort Data { get; set; }
        public ScenarioFlags Flags { get; set; }
        public ScenarioType Type { get; set; }
        public List<ScenarioStep> Steps { get; set; }

        public Scenario()
        {
            Steps = new List<ScenarioStep>();
        }

        public void ReadObject(IWowClientDBReader dbReader, BinaryReader reader)
        {
            using (BinaryReader br = reader)
            {
                if (dbReader.HasSeparateIndexColumn)
                    Id = br.ReadUInt32();

                if (dbReader.HasInlineStrings)
                    Name = br.ReadStringNull();
                else if (dbReader is STLReader)
                {
                    int offset = br.ReadInt32();
                    Name = (dbReader as STLReader).ReadString(offset);
                }
                else
                {
                    Name = dbReader.StringTable[br.ReadInt32()];
                }
                Data = br.ReadUInt16();
                Flags = (ScenarioFlags)br.ReadByte();
                Type = (ScenarioType)br.ReadByte();
            }
        }
    }

    public enum ScenarioType : byte
    {
        [Description("Scenario")]
        Scenario        = 0,
        [Description("Challenge Mode")]
        ChallengeMode   = 1,
        [Description("Unknown 1")]
        Unk1            = 2,
        [Description("Dungeon")]
        Dungeon         = 3,
        [Description("Invasion")]
        Invasion        = 4,
        [Description("Combat Training")]
        CombatTraining  = 5
    }

    [Flags]
    public enum ScenarioFlags : byte
    {
        [Description("Unknown 1")]
        Unknown1        = 0x1,
        [Description("Unknown 2")]
        Unknown2        = 0x2,
        [Description("Unknown 3")]
        Unknown3        = 0x4,
        [Description("Unknown 4")]
        Unknown4        = 0x8,
        [Description("Unknown 5")]
        Unknown5        = 0x10,
    }
}
