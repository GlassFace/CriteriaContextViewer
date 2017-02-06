using System.IO;
using CriteriaContextViewer.Model.Readers;

namespace CriteriaContextViewer.Model
{
    public interface IDBObjectReader
    {
        void ReadObject(IWowClientDBReader dbReader, BinaryReader reader, IDBCDataProvider dbcDataProvider, IDBDataProvider dbDataProvider);
    }
}