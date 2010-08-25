using System;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Represents a row in the database result for an executed SQL command.
  /// Reads values and entities from a row.
  /// Implementation for Linq2Sql 
  /// </summary>
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
      var entityMembers = _resolver.GetMetaDataMembers (typeof (T)); //get metadatamembers of subtypes

      Debug.Assert (entityMembers.Length == columnIDs.Length);

      var entityMembersWithColumnIDs = entityMembers.Select ((member, index) => new { Member = member, ColumnID = columnIDs[index] });

      Type instanceType = typeof (T);
      var discriminatorMember = entityMembersWithColumnIDs.SingleOrDefault (tuple => tuple.Member.IsDiscriminator);
      if (discriminatorMember != null)
      {
        var discriminatorValue = GetValue<object> (discriminatorMember.ColumnID);

        if (discriminatorValue != null)
          instanceType = discriminatorMember.Member.DeclaringType.GetTypeForInheritanceCode (discriminatorValue).Type;
        else
          instanceType = discriminatorMember.Member.DeclaringType.InheritanceDefault.Type;
      }

      object instance = Activator.CreateInstance (instanceType);

      var relevantMembers = entityMembersWithColumnIDs.Where (tuple => tuple.Member.Member.DeclaringType.IsAssignableFrom (instanceType));

      foreach (var member in relevantMembers)
      {
        var value = GetValue<object> (member.ColumnID);

        if (value is byte[])
        {
          if (member.Member.Type == typeof(Binary))
          {
            value = new Binary ((byte[]) value);
          }
        }
          
        if (value != null)
          member.Member.MemberAccessor.SetBoxedValue (ref instance, value);
      }

      return (T) instance;
    }
  }
}
