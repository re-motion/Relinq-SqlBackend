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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Provides a default implementation of <see cref="ISqlGenerationStage"/>.
  /// </summary>
  public class DefaultSqlGenerationStage : ISqlGenerationStage
  {
    private readonly MethodCallSqlGeneratorRegistry _registry;

    public DefaultSqlGenerationStage ()
    {
      // ReSharper disable DoNotCallOverridableMethodsInConstructor
      _registry = GenerateSqlGeneratorRegistry();
      // ReSharper restore DoNotCallOverridableMethodsInConstructor
    }

    public void GenerateTextForFromTable (SqlCommandBuilder commandBuilder, SqlTableBase table, bool isFirstTable)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("table", table);

      SqlTableAndJoinTextGenerator.GenerateSql (table, commandBuilder, this, isFirstTable);
    }

    // TODO Review 2494: Add a single protected virtual method GenerateTextForExpression (SqlCommandBuilder, Expression, SqlExpressionContext); call that method from all GenerateTextFor...Expression methods
    // TODO Review 2494: Rewrite the tests for DefaultSqlGenerationStage using a partial mock that intercepts GenerateTextForExpression

    public void GenerateTextForSelectExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry, selectedSqlContext, this);
    }

    public void GenerateTextForWhereExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry, selectedSqlContext, this);
    }

    public void GenerateTextForOrderByExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry, selectedSqlContext, this);
    }

    public void GenerateTextForTopExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry, selectedSqlContext, this);
    }

    public void GenerateTextForSqlStatement (SqlCommandBuilder commandBuilder, SqlStatement sqlStatement, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      var sqlStatementTextGenerator = new SqlStatementTextGenerator (this);
      sqlStatementTextGenerator.Build (sqlStatement, commandBuilder, selectedSqlContext);
    }

    public void GenerateTextForJoinKeyExpression (SqlCommandBuilder commandBuilder, Expression expression, SqlExpressionContext selectedSqlContext)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expression", expression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry, selectedSqlContext, this);
    }

    protected virtual MethodCallSqlGeneratorRegistry GenerateSqlGeneratorRegistry ()
    {
      var registry = new MethodCallSqlGeneratorRegistry();

      // TODO Review 2364: For each specific method call generator, add a public static readonly array holding the methods supported by that generator, similar to how SelectExpressionNode does it. Add one unit test per node to check the contents of the field.
      // TODO Review 2364: Add (and test) an overload of MethodCallSqlGeneratorRegistry.Register that takes an IEnumerable<MethodInfo>. That overload should iterate over the methods and register each of them with the given instance.
      // TODO Review 2364: Then, add an automatic registration facility; see MethodCallExpressionNodeTypeRegistry.CreateDefault. (Create instances of the types using Activator.CreateInstance().)
      // TODO Review 2364: Remove this method, instead inject the MethodCallExpressionNodeTypeRegistry via the ctor. Adapt usage in integration tests to supply the result of MethodCallExpressionNodeTypeRegistry.CreateDefault().

      //TODO: Convert methods with all overloads needed
      var containsMethod = typeof (string).GetMethod ("Contains", new[] { typeof (string) });
      var endsWithMethod = typeof (string).GetMethod ("EndsWith", new[] { typeof (string) });
      var lowerMethod = typeof (string).GetMethod ("ToLower", new Type[] { });
      var removeMethod = typeof (string).GetMethod ("Remove", new[] { typeof (int) });
      var startsWithMethod = typeof (string).GetMethod ("StartsWith", new[] { typeof (string) });
      var substringMethod = typeof (string).GetMethod ("Substring", new[] { typeof (int), typeof (int) });
      var toUpperMethod = typeof (string).GetMethod ("ToUpper", new Type[] { });

      var convertToStringMethod = typeof (Convert).GetMethod ("ToString", new[] { typeof (int) });
      var convertToBoolMethod = typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) });
      var convertToInt64Method = typeof (Convert).GetMethod ("ToInt64", new[] { typeof (int) });
      var convertToDateTimeMethod = typeof (Convert).GetMethod ("ToDateTime", new[] { typeof (int) });
      var convertToDoubleMethod = typeof (Convert).GetMethod ("ToDouble", new[] { typeof (int) });
      var convertToIntMethod = typeof (Convert).GetMethod ("ToInt32", new[] { typeof (int) });
      var convertToDecimalMethod = typeof (Convert).GetMethod ("ToDecimal", new[] { typeof (int) });
      var convertToCharMethod = typeof (Convert).GetMethod ("ToChar", new[] { typeof (int) });
      var convertToByteMethod = typeof (Convert).GetMethod ("ToByte", new[] { typeof (int) });

      registry.Register (containsMethod, new ContainsMethodCallSqlGenerator());
      registry.Register (convertToStringMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToBoolMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToInt64Method, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToDateTimeMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToDoubleMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToIntMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToDecimalMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToCharMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (convertToByteMethod, new ConvertMethodCallSqlGenerator());
      registry.Register (endsWithMethod, new EndsWithMethodCallSqlGenerator());
      registry.Register (lowerMethod, new LowerMethodCallSqlGenerator());
      registry.Register (removeMethod, new RemoveMethodCallSqlGenerator());
      registry.Register (startsWithMethod, new StartsWithMethodCallSqlGenerator());
      registry.Register (substringMethod, new SubstringMethodCallSqlGenerator());
      registry.Register (toUpperMethod, new UpperMethodCallSqlGenerator());

      return registry;
    }
  }
}