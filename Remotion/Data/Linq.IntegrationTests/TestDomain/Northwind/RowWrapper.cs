using System;
using System.Data;
using System.Diagnostics;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  class RowWrapper: IDatabaseResultRow
  {
    private readonly IDataReader _dataReader;
    private readonly IReverseMappingResolver _resolver;

    public RowWrapper (IDataReader dataReader, IReverseMappingResolver resolver)
    {
      _dataReader = dataReader;
      _resolver = resolver;
    }

    public T GetValue<T> (ColumnID id) //TODO ColumnID not int
    {
      return (T) _dataReader.GetValue (id.Position);
    }

    public T GetEntity<T> (ColumnID[] columnIDs)
    {
      var instance = (T) Activator.CreateInstance (typeof (T));
      var entityMembers = _resolver.GetEntityMembers (typeof (T));

      Debug.Assert (entityMembers.Length == columnIDs.Length);

      for (int i = 0; i < columnIDs.Length; ++i)
      {
        var currentMember = entityMembers[i];
        var value = GetValue<Object> (columnIDs[i]);
        currentMember.SetValue (instance, value, null);

      }
      return instance;
    }
  }
}
