/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest
{
  [TestFixture]
  public class SqlGenerationDataTest
  {
    [Test]
    public void SetSelectClause_DistinctTrue ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);
      data.SetSelectClause (true, fieldDescriptors, evaluation);

      Assert.That (data.Distinct, Is.True);
    }

    [Test]
    public void SetSelectClause_DistinctFalse ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);
      data.SetSelectClause (false, fieldDescriptors, evaluation);

      Assert.That (data.Distinct, Is.False);
    }

    [Test]
    public void SetSelectClause_Evaluation ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);
      data.SetSelectClause (true, fieldDescriptors, evaluation);

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
      data.SetSelectClause (true, fieldDescriptors, evaluation);

      Assert.That (data.Joins[sourcePath.FirstSource], Is.EqualTo (new[] {join}));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "There can only be one select clause.")]
    public void SetSelectClause_Twice ()
    {
      var data = new SqlGenerationData ();
      var fieldDescriptors = new List<FieldDescriptor> ();
      var evaluation = new Constant (0);
      data.SetSelectClause (true, fieldDescriptors, evaluation);
      data.SetSelectClause (true, fieldDescriptors, evaluation);
    }
  }
}