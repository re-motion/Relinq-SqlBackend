// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ConstantTableSourceVisitorTest
  {
    [Test]
    public void TranslateConstantTableSource ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      var resolver = MockRepository.GenerateMock<ISqlStatementResolver>();

      var tableSource = new SqlTableSource (typeof (int), "Table", "t");
      resolver.Expect (mock => mock.ResolveConstantTableSource ((ConstantTableSource) sqlTable.TableSource)).Return (tableSource);

      ConstantTableSourceVisitor.ReplaceTableSource (sqlTable, resolver);

      Assert.That (sqlTable.TableSource, Is.TypeOf (typeof(SqlTableSource)));
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException))]
    public void TranslateTableSource_WithUnknownTableSource ()
    {
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = new UnknownTableSource();
      var resolver = MockRepository.GenerateMock<ISqlStatementResolver> ();
      ConstantTableSourceVisitor.ReplaceTableSource (sqlTable, resolver);
    }

    private class UnknownTableSource : AbstractTableSource {
      
      public override Type Type
      {
        get { return typeof (string); }
      }

      public override AbstractTableSource Accept (ITableSourceVisitor visitor)
      {
        throw new NotImplementedException();
      }
    }
  }
}