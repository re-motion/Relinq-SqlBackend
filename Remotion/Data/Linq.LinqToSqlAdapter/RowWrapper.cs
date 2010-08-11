using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter.Utilities
{
  public class RowWrapper : IDatabaseResultRow
  {
    private readonly IDataReader _dataReader;
    private readonly IReverseMappingResolver _resolver;

    public RowWrapper (IDataReader dataReader, IReverseMappingResolver resolver)
    {
      _dataReader = dataReader;
      _resolver = resolver;
    }

    public T GetValue<T> (ColumnID id)
    {
      if (_dataReader.IsDBNull (id.Position))
      {
        return default (T);
      }

      return (T) _dataReader.GetValue (id.Position);
    }


    public T GetEntity<T> (ColumnID[] columnIDs)
    {
      object instance = (T) Activator.CreateInstance (typeof (T));
      var entityMembers = _resolver.GetMetaDataMembers (typeof (T));

      //TODO: WHY?
      //Debug.Assert (entityMembers.Length == columnIDs.Length);

      foreach (var columnID in columnIDs)
      {
        var value = GetValue<Object> (columnID);
        var metaMemberCollection = from em in entityMembers where em.MappedName == columnID.ColumnName select em;
        var metaMember = metaMemberCollection.First ();
        metaMember.MemberAccessor.SetBoxedValue (ref instance, value);
      }

      return (T) instance;
    }
  }
}
