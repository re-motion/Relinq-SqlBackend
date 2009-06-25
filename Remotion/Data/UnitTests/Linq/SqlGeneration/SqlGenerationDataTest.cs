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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration
{
  [TestFixture]
  public class SqlGenerationDataTest
  {
    private IClause _previousClause;

    public IClause PreviousClause
    {
      get { return _previousClause; }
    }

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateClause ();
    }

    [Test]
    public void SetSelectClause_ResultModification ()
    {
      var data = new SqlGenerationData();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);

      ICollection<ResultModificationBase> modifications = new List<ResultModificationBase> ();
      modifications.Add (new DistinctResultModification ());

      data.SetSelectClause (modifications, fieldDescriptors, evaluation);

      Assert.That (data.ResultModifiers, Is.EqualTo (modifications));
    }

    [Test]
    public void SetSelectClause_Evaluation ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);

      var resultModifications = new List<ResultModificationBase>();
      resultModifications.Add (new DistinctResultModification ());
      data.SetSelectClause (resultModifications, fieldDescriptors, evaluation);

      Assert.That (data.SelectEvaluation, Is.EqualTo (evaluation));
    }

    [Test]
    public void SetSelectClause_FieldDescriptors ()
    {
      var data = new SqlGenerationData ();
      var join = new SingleJoin();
      var sourcePath = new FieldSourcePath (new Table (), new[] { join });
      var fieldDescriptor = new FieldDescriptor (typeof (string).GetProperty ("Length"), sourcePath, null);
      var fieldDescriptors = new List<FieldDescriptor> { fieldDescriptor };
      var evaluation = new Constant (0);
      var resultModifications = new List<ResultModificationBase> ();
      resultModifications.Add (new DistinctResultModification ());
      data.SetSelectClause (resultModifications, fieldDescriptors, evaluation);

      Assert.That (data.Joins[sourcePath.FirstSource], Is.EqualTo (new[] {join}));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "There can only be one select clause.")]
    public void SetSelectClause_Twice ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);
      var resultModifications = new List<ResultModificationBase>();
      resultModifications.Add (new DistinctResultModification ());
      data.SetSelectClause (resultModifications, fieldDescriptors, evaluation);
      data.SetSelectClause (resultModifications, fieldDescriptors, evaluation);
    }
  }
}
