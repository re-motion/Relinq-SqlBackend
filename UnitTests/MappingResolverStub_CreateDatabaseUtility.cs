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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests
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
      AppendCreateTableScript (sb, typeof (Knife));
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
      var sqlEntityDefinition = mappingResolver.ResolveSimpleTableInfo (resolvedTableInfo);
      var columnData = (from c in sqlEntityDefinition.Columns
                        let columnName = c.ColumnName
                        let columnValue =
                            GetColumnValue (entity, mappingResolver, sqlEntityDefinition, columnName)
                        select new { columnName, columnValue }).ToArray();

      var columnNames = string.Join (",", columnData.Select (d => d.columnName));
      var columnValues = string.Join (",", columnData.Select (d => GetSqlValueString (d.columnValue)));

      sb.AppendFormat ("INSERT INTO [{0}] ({1}) VALUES ({2});", resolvedTableInfo.TableName, columnNames, columnValues);
      sb.AppendLine();
    }

    private static object GetColumnValue (object entity, MappingResolverStub mappingResolver, SqlEntityDefinitionExpression sqlEntityDefinition, string columnName)
    {
      var propertiesWithColumnName =
          entity.GetType()
                .GetProperties()
                .SelectMany (p => TryResolveProperty (mappingResolver, sqlEntityDefinition, p, entity), (p, t) => new { Property = p, ColumnNameAndValue = t })
                .ToArray();
      var matchingProperty = propertiesWithColumnName.FirstOrDefault (d => d.ColumnNameAndValue.Key == columnName);
      // Assert.That (matchingProperties, Has.Length.LessThanOrEqualTo (1), entity.GetType().Name + ": " + string.Join (",", matchingProperties));

      Assert.IsNotNull (
          matchingProperty,
          "No member found for column '{0}' on entity type '{1}'.\r\n(Found: {2})",
          columnName,
          entity.GetType().Name,
          string.Join (",", propertiesWithColumnName.Select (p=> p.ToString())));
      return matchingProperty.ColumnNameAndValue.Value;
    }

    private static KeyValuePair<string, object>[] TryResolveProperty (
        MappingResolverStub mappingResolver, SqlEntityExpression sqlEntityDefinition, PropertyInfo member, object entity)
    {
      Expression expression;
      try
      {
        expression = mappingResolver.ResolveMemberExpression (sqlEntityDefinition, member);
      }
      catch (UnmappedItemException)
      {
        return new KeyValuePair<string, object>[0];
      }

      var memberValue = member.GetValue (entity, null);
      return TryResolvePropertyExpression(mappingResolver, expression, memberValue).ToArray();
    }

    private static KeyValuePair<string, object>[] TryResolvePropertyExpression (
        MappingResolverStub mappingResolver, Expression expression, object value)
    {
      var columnExpression = expression as SqlColumnExpression;
      if (columnExpression != null)
        return new[] { new KeyValuePair<string, object> (columnExpression.ColumnName, value) };

      var newExpression = expression as NewExpression;
      if (newExpression != null)
      {
        return
            newExpression.Arguments.SelectMany (
            (a, i) =>
            {
              var argumentMemberValue = value != null ? ((PropertyInfo) newExpression.Members[i]).GetValue (value, null) : null;
              return TryResolvePropertyExpression (mappingResolver, a, argumentMemberValue);
            }).ToArray();
      }

      var namedExpression = expression as NamedExpression;
      if (namedExpression != null)
        return TryResolvePropertyExpression (mappingResolver, namedExpression.Expression, value);

      var memberRefExpression = (SqlEntityRefMemberExpression) expression;
      var optimizedIdentity = mappingResolver.TryResolveOptimizedIdentity (memberRefExpression);
      if (optimizedIdentity == null)
        return new KeyValuePair<string, object>[0];

      var idOfReferencedEntity = value != null ? value.GetType().GetProperty ("ID").GetValue (value, null) : null;
      return TryResolvePropertyExpression (mappingResolver, optimizedIdentity, idOfReferencedEntity);
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
      var company1 = new Company { ID = 1, MainRestaurant = null, MainKitchen = null, DateOfIncorporation = new DateTime (2001, 01, 13)};
      var company2 = new Company { ID = 2, MainRestaurant = null, MainKitchen = null, DateOfIncorporation = new DateTime (1886, 02, 20) };

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

      var knife1 = new Knife { ID = new MetaID (1, "KnifeClass"), Sharpness = 10.0 };
      var cook1 = new Cook
                  {
                      ID = 1,
                      FirstName = "Peter Paul",
                      Name = "Rubens",
                      IsStarredCook = false,
                      IsFullTimeCook = false,
                      Substitution = null,
                      Kitchen = null,
                      KnifeID = knife1.ID,
                      Knife = knife1
                  };
      var knife2 = new Knife { ID = new MetaID (2, "DerivedKnifeClass"), Sharpness = 5.0 };
      var cook2 = new Cook
                  {
                      ID = 2,
                      FirstName = "Jamie",
                      Name = "Oliver",
                      IsStarredCook = true,
                      IsFullTimeCook = false,
                      Substitution = null,
                      Kitchen = kitchen1,
                      KnifeID = knife2.ID,
                      Knife = knife2
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

      return new object[] { company1, company2, restaurant1, restaurant2, kitchen1, kitchen2, kitchen3, chef1, chef2, knife1, knife2, cook1, cook2, cook3 };
    }

    private static void AppendCreateTableScript (StringBuilder sb, Type type)
    {
      var mappingResolver = new MappingResolverStub();
      var generator = new UniqueIdentifierGenerator();
      var tableInfo = (ResolvedSimpleTableInfo) mappingResolver.ResolveTableInfo (new UnresolvedTableInfo (type), generator);
      var entity = mappingResolver.ResolveSimpleTableInfo (tableInfo);

      var columnDeclarations = from c in entity.Columns
                               let sqlTypeName = GetColumnType(c)
                               select string.Format ("[{0}] {1}", c.ColumnName, sqlTypeName);
      var primaryKeyColumns = entity.Columns.Where (c => c.IsPrimaryKey).Select (c => c.ColumnName).ToArray();
      string primaryKeyConstraint = "";
      if (primaryKeyColumns.Length > 0)
      {
        primaryKeyConstraint = string.Format (
            " CONSTRAINT PK_{0} PRIMARY KEY ({1})", tableInfo.TableName.Replace (".", "_"), string.Join (",", primaryKeyColumns));
      }

      sb.AppendFormat (
          "CREATE TABLE [{0}]{1}({1}{2}{1} {3})",
          tableInfo.TableName,
          Environment.NewLine,
          string.Join ("," + Environment.NewLine, columnDeclarations.Select (c => "  " + c)),
          primaryKeyConstraint);
      sb.AppendLine();
    }

    private static string GetColumnType (SqlColumnExpression c)
    {
      var sqlTypeName = SqlConvertExpression.GetSqlTypeName (c.Type);
      // (MAX) types are not valid in primary key columns.
      if (c.IsPrimaryKey)
        return sqlTypeName.Replace ("(MAX)", "(100)");

      return sqlTypeName;
    }
  }
}