using System;
using System.Data;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Represents a scalar row in the database result for an executed SQL command.
  /// Reads the value from a row.
  /// Implementation for Linq2Sql 
  /// </summary>
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
      if (columnIDs == null)
        throw new ArgumentException ("You must provide 1 ColumnID!");

      if(columnIDs.Length!=1)
        throw new ArgumentException ("Only Scalar values are alowed!");

      return (T) GetValue<T>(columnIDs[0]);
    }
  }
}
