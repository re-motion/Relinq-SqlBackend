using System;
using System.Data;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  public class ScalarRowWrapper:IDatabaseResultRow
  {
    private readonly IDataReader _dataReader;
    private readonly IReverseMappingResolver _resolver;

    public ScalarRowWrapper (IDataReader dataReader, IReverseMappingResolver resolver)
    {
      _dataReader = dataReader;
      _resolver = resolver;
    }

    public T GetValue<T> (ColumnID id)
    {
      if (_dataReader.IsDBNull (id.Position))
        return default (T);

      if (id.Position != 0)
        throw new ArgumentException ("Only Columns with the position 0 are valid Scalar Columns!");

      return (T) _dataReader.GetValue (id.Position);
    }


    public T GetEntity<T> (params ColumnID[] columnIDs)
    {
      throw new ArgumentException ("Only Scalar values are alowed!");
    }
  }
}
