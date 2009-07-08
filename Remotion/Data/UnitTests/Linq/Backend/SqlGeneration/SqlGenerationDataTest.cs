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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration
{
  [TestFixture]
  public class SqlGenerationDataTest
  {
    private FieldDescriptor _fieldDescriptor1;
    private FieldDescriptor _fieldDescriptor2;
    private List<FieldDescriptor> _fieldDescriptors;
    private FieldSourcePath _sourcePath1;
    private SingleJoin _join1;
    private FieldSourcePath _sourcePath2;
    private SingleJoin _join2;
    private Constant _evaluation;

    [SetUp]
    public void SetUp ()
    {
      _join1 = new SingleJoin ();
      _sourcePath1 = new FieldSourcePath (new Table ("x", "y"), new[] { _join1 });
      _join2 = new SingleJoin ();
      _sourcePath2 = new FieldSourcePath (new Table ("a", "b"), new[] { _join2 });
      _fieldDescriptor1 = new FieldDescriptor (typeof (string).GetProperty ("Length"), _sourcePath1, new Column());
      _fieldDescriptor2 = new FieldDescriptor (typeof (string).GetProperty ("Length"), _sourcePath2, new Column());
      _fieldDescriptors = new List<FieldDescriptor> { _fieldDescriptor1, _fieldDescriptor2 };
      _evaluation = new Constant (0);
    }

    [Test]
    public void SetSelectEvaluation ()
    {
      var data = new SqlGenerationData ();
      data.SetSelectEvaluation (_evaluation, _fieldDescriptors);

      Assert.That (data.SelectEvaluation, Is.EqualTo (_evaluation));
    }

    [Test]
    public void SetSelectEvaluation_FieldDescriptors ()
    {
      var data = new SqlGenerationData ();
      data.SetSelectEvaluation (_evaluation, _fieldDescriptors);

      Assert.That (data.Joins[_sourcePath1.FirstSource], Is.EqualTo (new[] { _join1 }));
      Assert.That (data.Joins[_sourcePath2.FirstSource], Is.EqualTo (new[] { _join2 }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "There can only be one select clause.")]
    public void SetSelectEvaluation_Twice ()
    {
      var data = new SqlGenerationData ();
      data.SetSelectEvaluation (_evaluation, _fieldDescriptors);
      data.SetSelectEvaluation (_evaluation, _fieldDescriptors);
    }

    [Test]
    public void PrependOrderingFields ()
    {
      var orderingField1 = new OrderingField (_fieldDescriptor1, OrderingDirection.Asc);
      var orderingField2 = new OrderingField (_fieldDescriptor2, OrderingDirection.Desc);

      var data = new SqlGenerationData ();
      data.PrependOrderingFields (new[] { orderingField1, orderingField2 });

      Assert.That (data.OrderingFields, Is.EqualTo (new[] { orderingField1, orderingField2 }));
    }

    [Test]
    public void PrependOrderingFields_Twice ()
    {
      var orderingField1 = new OrderingField (_fieldDescriptor1, OrderingDirection.Asc);
      var orderingField2 = new OrderingField (_fieldDescriptor2, OrderingDirection.Desc);

      var orderingField3 = new OrderingField (_fieldDescriptor1, OrderingDirection.Desc);
      var orderingField4 = new OrderingField (_fieldDescriptor1, OrderingDirection.Desc);

      var data = new SqlGenerationData ();
      data.PrependOrderingFields (new[] { orderingField1, orderingField2 });
      data.PrependOrderingFields (new[] { orderingField3, orderingField4 });

      Assert.That (data.OrderingFields, Is.EqualTo (new[] { orderingField3, orderingField4, orderingField1, orderingField2 }));
    }

    [Test]
    public void PrependOrderingFields_Joins ()
    {
      var orderingField1 = new OrderingField (_fieldDescriptor1, OrderingDirection.Asc);
      var orderingField2 = new OrderingField (_fieldDescriptor2, OrderingDirection.Desc);

      var data = new SqlGenerationData ();
      data.PrependOrderingFields (new[] { orderingField1, orderingField2 });

      Assert.That (data.Joins[_sourcePath1.FirstSource], Is.EqualTo (new[] { _join1 }));
      Assert.That (data.Joins[_sourcePath2.FirstSource], Is.EqualTo (new[] { _join2 }));
    }
  }
}
