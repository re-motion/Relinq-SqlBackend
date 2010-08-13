using System;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter
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
      var entityMembers = _resolver.GetMetaDataMembers (typeof (T));

      Debug.Assert (entityMembers.Length == columnIDs.Length);

      //object instance = (T) Activator.CreateInstance (typeof (T));

      //foreach (var columnID in columnIDs)
      //{
      //  var value = GetValue<Object> (columnID);

      //  var searchID = columnID; //Access to modified closure problem --> if there is no local variable!
      //  var metaMemberCollection = from em in entityMembers where em.MappedName == searchID.ColumnName select em;
      //  var metaMember = metaMemberCollection.First ();
      //  metaMember.MemberAccessor.SetBoxedValue (ref instance, value);
      //}

      // TODO: Use this implementation instead. Before doing so, write a test showing that GetEntity<Contact> will instantiate a Customer
      // TODO: if the CustomerType discriminator column contains the string "Customer".
      // TODO: Also write a test showing that if the discriminator column contains null, T (eg. Contact) is instantiated.
      // TODO: Also write a test showing that types without discriminator column can still be instantiated.
      // TODO: Also write a test showing that when entityMembers contains members of eg. Supplier, those members do not cause an exception 
      // TODO: (because they are ignored).
      // TODO: Also write a test showing that byte[]s can be used.
      var entityMembersWithColumnIDs = entityMembers.Select ((member, index) => new { Member = member, ColumnID = columnIDs[index] });

      Type instanceType = typeof (T);
      var discriminatorMember = entityMembersWithColumnIDs.SingleOrDefault (tuple => tuple.Member.IsDiscriminator);
      if (discriminatorMember != null)
      {
        var discriminatorValue = GetValue<object> (discriminatorMember.ColumnID);
        if (discriminatorValue != null)
          instanceType = discriminatorMember.Member.DeclaringType.GetTypeForInheritanceCode (discriminatorValue).Type;
      }

      object instance = Activator.CreateInstance (instanceType);

      var relevantMembers =
         entityMembersWithColumnIDs.Where (tuple => tuple.Member.Member.DeclaringType.IsAssignableFrom (instanceType));

      foreach (var member in relevantMembers)
      {
        var value = GetValue<object> (member.ColumnID);
        if (value is byte[])
          value = new Binary ((byte[]) value);
        if(value != null) //TODO check for null value ? if discriminator column contains null, set to which value or set no value at all?
          member.Member.MemberAccessor.SetBoxedValue (ref instance, value); //TODO: can't cast to from 'System.Data.Linq.Binary' to 'System.Byte[]' when trying to set it
      }

      return (T) instance;
    }
  }
}
