// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using IDatabaseResultRow = Remotion.Linq.SqlBackend.SqlGeneration.IDatabaseResultRow;

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

    public T GetValue<T> (Remotion.Linq.SqlBackend.SqlGeneration.ColumnID id)
    {
      if (_dataReader.IsDBNull (id.Position))
        return default (T);

      return (T) _dataReader.GetValue (id.Position);
    }


    public T GetEntity<T> (Remotion.Linq.SqlBackend.SqlGeneration.ColumnID[] columnIDs)
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
          if (member.Member.Type == typeof (Binary))
            value = new Binary ((byte[]) value);
        }

        if (value != null)
          member.Member.MemberAccessor.SetBoxedValue (ref instance, value);
      }

      return (T) instance;
    }
  }
}