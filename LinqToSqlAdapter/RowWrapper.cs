﻿// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Utilities;

namespace Remotion.Linq.LinqToSqlAdapter
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

      Assertion.DebugAssert (entityMembers.Length == columnIDs.Length);

      var entityMembersWithColumnIDs = entityMembers.Select ((member, index) => new { Member = member, ColumnID = columnIDs[index] }).ToArray();

      // Assumption: If the primary key is null, the whole returned entity is null.
      var idColumns = entityMembersWithColumnIDs.Where (c => c.Member.IsPrimaryKey).ToArray();
      Assertion.IsTrue (
          idColumns.Length > 0,
          "MappingResolver currently does not support entities without identity members, so we don't need to either.");
      if (idColumns.All (c => GetValue<object> (c.ColumnID) == null))
        return default(T);

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