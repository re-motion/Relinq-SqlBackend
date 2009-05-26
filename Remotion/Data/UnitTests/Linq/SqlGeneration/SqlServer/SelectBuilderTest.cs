// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SelectBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private SelectBuilder _selectBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      _selectBuilder = new SelectBuilder (_commandBuilder);
    }

    [Test]
    public void CombineColumnItems()
    {
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor(Type.EmptyTypes), 
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2"),
        new Column (new Table ("s3", "s3"), "c3")
      );

      _selectBuilder.BuildSelectPart (evaluation, null);
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText());
    }
    
    [Test]
    public void BinaryEvaluations_Add ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");
      
      BinaryEvaluation binaryEvaluation = new BinaryEvaluation(c1,c2,BinaryEvaluation.EvaluationKind.Add);

      _selectBuilder.BuildSelectPart (binaryEvaluation, null);
      Assert.AreEqual ("SELECT ([s1].[c1] + [s2].[c2]) ", _commandBuilder.GetCommandText ());
    }
  }
}
