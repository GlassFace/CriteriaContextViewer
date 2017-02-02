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
    public class CriteriaTree : IDBObjectReader
    {
        public const string FileName = @"CriteriaTree.db2";

        public uint Id { get; set; }
        public uint CriteriaId { get; set; }
        public Criteria Criteria { get; set; }
        public uint Amount { get; set; }
        public string Description { get; set; }
        public ushort ParentId { get; set; }
        public CriteriaTree Parent { get; set; }
        public List<CriteriaTree> Children { get; set; }
        public CriteriaTreeFlags Flags { get; set; }
        public CriteriaTreeOperator Operator { get; set; }
        public short OrderIndex { get; set; }

        public CriteriaTree()
        {
            Children = new List<CriteriaTree>();
        }

        public override string ToString()
        {
            switch (Operator)
            {
                case CriteriaTreeOperator.Single:
                    if (Parent.Operator == CriteriaTreeOperator.SumChildrenWeight)
                        return $"CT {Id} - Increase parent criteria tree progress by {Amount}";
                    return $"CT {Id} - The following criteria is met" + (Amount > 1 ? $" {Amount} times" : "");
                case CriteriaTreeOperator.SingleNotCompleted:
                    return $"CT {Id} - The following criteria is not met";
                case CriteriaTreeOperator.All:
                    return $"CT {Id} - All of the following criterias are met";
                case CriteriaTreeOperator.SumChildren:
                    return $"CT {Id} - The following criterias are met at least {Amount} time" + (Amount > 1 ? "s" : "");
                case CriteriaTreeOperator.MaxChild:
                    return $"CT {Id} - Any of the following criterias are met at least {Amount} time" + (Amount > 1 ? "s" : "");
                case CriteriaTreeOperator.CountDirectChildren:
                    return $"CT {Id} - At least {Amount} of the following criterias are met at least once";
                case CriteriaTreeOperator.Any:
                    return $"CT {Id} - At least {Amount} criteria" + (Amount > 1 ? "s" : "") + " are met";
                case CriteriaTreeOperator.SumChildrenWeight:
                    return $"CT {Id} - Criteria tree progress is more than {Amount}";
            }

            string description = $"CT {Id}";
            if (!string.IsNullOrEmpty(Description))
                description = $"{description} - {Description}";

            return description;
        }

        public void ReadObject(IWowClientDBReader dbReader, BinaryReader reader, IDBCDataProvider dbcDataProvider)
        {
            using (BinaryReader br = reader)
            {
                if (dbReader.HasSeparateIndexColumn)
                    Id = reader.ReadUInt32();

                CriteriaId = reader.ReadUInt32();
                Amount = reader.ReadUInt32();

                if (dbReader.HasInlineStrings)
                    Description = br.ReadStringNull();
                else if (dbReader is STLReader)
                {
                    int offset = br.ReadInt32();
                    Description = (dbReader as STLReader).ReadString(offset);
                }
                else
                {
                    Description = dbReader.StringTable[br.ReadInt32()];
                }

                ParentId = reader.ReadUInt16();
                Flags = (CriteriaTreeFlags) reader.ReadUInt16();
                Operator = (CriteriaTreeOperator) reader.ReadByte();
                OrderIndex = reader.ReadInt16();
            }
        }
    }

    [Flags]
    public enum CriteriaTreeFlags : ushort
    {
        [Description("Progress bar")]
        ProgressBar         = 0x0001,
        [Description("Progress is date")]
        ProgressIsDate      = 0x0004,
        [Description("Show currency icon")]
        ShowCurrencyIcon    = 0x0008,
        [Description("Alliance only")]
        AllianceOnly        = 0x0200,
        [Description("Horde only")]
        HordeOnly           = 0x0400,
        [Description("Show required count")]
        ShowRequiredCount   = 0x0800
    };

    public enum CriteriaTreeOperator : byte
    {
        [Description("Single")]
        Single              = 0,
        [Description("SingleNotCompleted")]
        SingleNotCompleted  = 1,
        [Description("All")]
        All                 = 4,
        [Description("Sum children")]
        SumChildren         = 5,
        [Description("Max child")]
        MaxChild            = 6,
        [Description("Count direct children")]
        CountDirectChildren = 7,
        [Description("Any")]
        Any                 = 8,
        [Description("Sum children weight")]
        SumChildrenWeight   = 9
    };
}
