// This file is part of the re-linq project (relinq.codeplex.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend
{
  public static class MappingResolverStub_CreateDatabaseUtility
  {
    public static void DumpScripts ()
    {
      var databaseName = "RelinqSqlTestDomain";

      Console.WriteLine (GetDropDatabaseScript (databaseName));
      Console.WriteLine ("GO");
      Console.WriteLine ();
      Console.WriteLine (GetCreateDatabaseScript (databaseName));
      Console.WriteLine ("GO");
      Console.WriteLine ();
      Console.WriteLine (GetCreateTablesScript (databaseName));
      Console.WriteLine ("GO");
      Console.WriteLine ();
      Console.WriteLine (GetInsertDataScript (databaseName, GetSampleDataEntities()));
      Console.WriteLine ("GO");

    }

    public static string GetDropDatabaseScript (string databaseName)
    {
      return string.Format (@"USE master

IF EXISTS (SELECT * FROM sysdatabases WHERE name = '{0}')
BEGIN
  ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
  DROP DATABASE [{0}]
END", databaseName);
    }

    public static string GetCreateDatabaseScript (string databaseName)
    {
      return string.Format (@"USE master
  
CREATE DATABASE [{0}]
ON PRIMARY (
  Name = '{0}_Data',
  Filename = 'C:\Databases\{0}.mdf',
  Size = 10MB
)
LOG ON (
  Name = '{0}_Log',
  Filename = 'C:\Databases\{0}.ldf',
  Size = 10MB	
)", databaseName);
    }

    public static string GetCreateTablesScript (string databaseName)
    {
      var sb = new StringBuilder();
      sb.AppendFormat ("USE [{0}]", databaseName);
      sb.AppendLine();
      AppendCreateTableScript (sb, typeof (Chef));
      AppendCreateTableScript (sb, typeof (Company));
      AppendCreateTableScript (sb, typeof (Cook));
      AppendCreateTableScript (sb, typeof (Kitchen));
      AppendCreateTableScript (sb, typeof (Restaurant));
      return sb.ToString();
    }

    public static string GetInsertDataScript (string databaseName, IEnumerable<object> entities)
    {
      var sb = new StringBuilder ();
      sb.AppendFormat ("USE [{0}]", databaseName);
      sb.AppendLine ();

      foreach (var entity in entities)
        AppendInsertScript (sb, entity);

      return sb.ToString ();
    }

    private static void AppendInsertScript (StringBuilder sb, object entity)
    {
      var mappingResolver = new MappingResolverStub ();
      var generator = new UniqueIdentifierGenerator();
      var resolvedTableInfo = (ResolvedSimpleTableInfo) mappingResolver.ResolveTableInfo (new UnresolvedTableInfo (entity.GetType()), generator);
      var sqlEntityDefinition = mappingResolver.ResolveSimpleTableInfo (resolvedTableInfo, generator);
      var columnData = (from c in sqlEntityDefinition.Columns
                        let columnName = c.ColumnName
                        let columnValue =
                            GetColumnValue (entity, mappingResolver, sqlEntityDefinition, columnName)
                        select new { columnName, columnValue }).ToArray();

      var columnNames = SeparatedStringBuilder.Build (",", columnData.Select (d => d.columnName));
      var columnValues = SeparatedStringBuilder.Build (",", columnData.Select (d => GetSqlValueString (d.columnValue)));

      sb.AppendFormat ("INSERT INTO [{0}] ({1}) VALUES ({2});", resolvedTableInfo.TableName, columnNames, columnValues);
      sb.AppendLine();
    }

    private static object GetColumnValue (object entity, MappingResolverStub mappingResolver, SqlEntityDefinitionExpression sqlEntityDefinition, string columnName)
    {
      var propertiesWithColumnName =
          entity.GetType()
                .GetProperties()
                .Select (p => new { Property = p, ColumnNameAndValue = TryResolveProperty (mappingResolver, sqlEntityDefinition, p, entity) })
                .ToArray();
      var matchingProperties = propertiesWithColumnName.Where (d => d.ColumnNameAndValue != null && d.ColumnNameAndValue.Value.Key == columnName).ToArray ();
      Assert.That (matchingProperties, Has.Length.LessThanOrEqualTo (1), entity.GetType().Name + ": " + SeparatedStringBuilder.Build (",", matchingProperties));

      var matchingProperty = matchingProperties.SingleOrDefault();
      Assert.IsNotNull (
          matchingProperty,
          "No member found for column '{0}' on entity type '{1}'.\r\n(Found: {2})",
          columnName,
          entity.GetType().Name,
          SeparatedStringBuilder.Build (",", propertiesWithColumnName));
      return matchingProperty.ColumnNameAndValue.Value.Value;
    }

    private static KeyValuePair<string, object>? TryResolveProperty (MappingResolverStub mappingResolver, SqlEntityExpression sqlEntityDefinition, PropertyInfo member, object entity)
    {
      Expression expression;
      try
      {
        expression = mappingResolver.ResolveMemberExpression (sqlEntityDefinition, member);
      }
      catch (UnmappedItemException)
      {
        return null;
      }
      
      var columnExpression = expression as SqlColumnExpression;
      if (columnExpression != null)
        return new KeyValuePair<string, object> (columnExpression.ColumnName, member.GetValue (entity, null));

      var memberRefExpression = (SqlEntityRefMemberExpression) expression;
      if (typeof (IEnumerable).IsAssignableFrom (memberRefExpression.Type))
        return null;

      var resolvedJoin =
          mappingResolver.ResolveJoinInfo (
              new UnresolvedJoinInfo (memberRefExpression.OriginatingEntity, memberRefExpression.MemberInfo, JoinCardinality.One),
              new UniqueIdentifierGenerator());
      
      // TODO 4878: This won't work with Cook.Knife, change it.
      var leftKey = ((SqlColumnExpression) ((BinaryExpression) resolvedJoin.JoinCondition).Left);
      if (leftKey.IsPrimaryKey)
        return null;

      var referencedEntity = member.GetValue (entity, null);
      var foreignKeyValue = referencedEntity != null ? referencedEntity.GetType().GetProperty ("ID").GetValue (referencedEntity, null) : null;
      return new KeyValuePair<string, object> (leftKey.ColumnName, foreignKeyValue);
    }

    private static string GetSqlValueString (object columnValue)
    {
      if (columnValue == null)
        return "NULL";

      var underlyingType = columnValue.GetType();
      underlyingType = Nullable.GetUnderlyingType (underlyingType) ?? underlyingType;

      switch (underlyingType.Name)
      {
        case "String":
          return "'" + ((string) columnValue).Replace ("'", "''") + "'";
        case "Guid":
          return "'" + columnValue + "'";
        case "DateTime":
          return "'" + ((DateTime) columnValue).ToString ("yyyy-MM-dd") + "'";
        case "Boolean":
          return columnValue.Equals (true) ? "1" : "0";
        default:
          return columnValue.ToString();
      }
    }

    private static IEnumerable<object> GetSampleDataEntities ()
    {
      var company1 = new Company { ID = 1, MainRestaurant = null, MainKitchen = null };
      var company2 = new Company { ID = 2, MainRestaurant = null, MainKitchen = null };

      var restaurant1 = new Restaurant { ID = 1, CompanyIfAny = company1};
      company1.MainRestaurant = restaurant1;

      var restaurant2 = new Restaurant { ID = 2 };
      company2.MainRestaurant = restaurant2;

      var kitchen1 = new Kitchen
                     {
                         ID = 1,
                         Name = "Jamie's Kitchen",
                         Cook = null,
                         LastCleaningDay = new DateTime (2012, 5, 13),
                         LastInspectionScore = 80,
                         PassedLastInspection = true,
                         Restaurant = restaurant1,
                         RoomNumber = 17
                     };
      company1.MainKitchen = kitchen1;

      var kitchen2 = new Kitchen
                     {
                         ID = 2,
                         Name = "Perfumerie",
                         Cook = null,
                         LastCleaningDay = null,
                         LastInspectionScore = null,
                         PassedLastInspection = null,
                         Restaurant = restaurant1,
                         RoomNumber = 1
                     };
      var kitchen3 = new Kitchen
                     {
                         ID = 3,
                         Name = "Bocuse's Kitchen",
                         Cook = null,
                         LastCleaningDay = DateTime.Today,
                         LastInspectionScore = 100,
                         PassedLastInspection = true,
                         Restaurant = restaurant2,
                         RoomNumber = 2
                     };
      company2.MainKitchen = kitchen3;

      var cook1 = new Cook
                  {
                      ID = 1,
                      FirstName = "Peter Paul",
                      Name = "Rubens",
                      IsStarredCook = false,
                      IsFullTimeCook = false,
                      Substitution = null,
                      Kitchen = null
                  };
      var cook2 = new Cook
                  {
                      ID = 2,
                      FirstName = "Jamie",
                      Name = "Oliver",
                      IsStarredCook = true,
                      IsFullTimeCook = false,
                      Substitution = null,
                      Kitchen = kitchen1
                  };
      kitchen1.Cook = cook2;
      var cook3 = new Cook
                  {
                      ID = 3,
                      FirstName = "Hugo",
                      Name = "Boss",
                      IsStarredCook = false,
                      IsFullTimeCook = true,
                      Substitution = cook1,
                      Kitchen = kitchen2
                  };
      cook1.Substituted = cook3;
      kitchen2.Cook = cook3;

      var chef1 = new Chef
                  {
                      ID = 4,
                      FirstName = "Paul",
                      Name = "Bocuse",
                      IsStarredCook = true,
                      IsFullTimeCook = false,
                      Substitution = cook1,
                      Kitchen = kitchen3,
                      LetterOfRecommendation = "A really great chef!"
                  };
      cook1.Substituted = chef1;
      kitchen3.Cook = chef1;
      var chef2 = new Chef
                  {
                      ID = 5,
                      FirstName = "Caul",
                      Name = "Bopuse",
                      IsStarredCook = false,
                      IsFullTimeCook = true,
                      Substitution = cook2,
                      Kitchen = null,
                      LetterOfRecommendation = null
                  };
      cook2.Substituted = chef2;

      return new object[] { company1, company2, restaurant1, restaurant2, kitchen1, kitchen2, kitchen3, chef1, chef2, cook1, cook2, cook3 };
    }

    private static void AppendCreateTableScript (StringBuilder sb, Type type)
    {
      var mappingResolver = new MappingResolverStub();
      var generator = new UniqueIdentifierGenerator();
      var tableInfo = (ResolvedSimpleTableInfo) mappingResolver.ResolveTableInfo (new UnresolvedTableInfo (type), generator);
      var entity = mappingResolver.ResolveSimpleTableInfo (tableInfo, generator);

      var columnDeclarations = from c in entity.Columns
                               let sqlTypeName = SqlConvertExpression.GetSqlTypeName (c.Type)
                               let primaryKeyConstraint = c.IsPrimaryKey ? " PRIMARY KEY" : ""
                               select string.Format ("[{0}] {1}{2}", c.ColumnName, sqlTypeName, primaryKeyConstraint);

      sb.AppendFormat (
          "CREATE TABLE [{0}]{1}({1}{2}{1})",
          tableInfo.TableName,
          Environment.NewLine,
          SeparatedStringBuilder.Build ("," + Environment.NewLine, columnDeclarations.Select (c => "  " + c)));
      sb.AppendLine();
    }
  }
}