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
using System.IO;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.IntegrationTests.CSharp
{
  [TestFixture]
  public class TestResultSerializerTest
  {
    private StringWriter _writer;
    private TestResultSerializer _serializer;

    [SetUp]
    public void SetUp ()
    {
      _writer = new StringWriter ();
      _serializer = new TestResultSerializer (_writer);
    }

    [Test]
    public void Serialize_SimpleValue ()
    {
      _serializer.Serialize (12);

      Assert.That (_writer.ToString (), Is.EqualTo ("12" + Environment.NewLine));
    }

    [Test]
    public void Serialize_String ()
    {
      _serializer.Serialize ("test");

      Assert.That (_writer.ToString (), Is.EqualTo ("'test'" + Environment.NewLine));
    }

    [Test]
    public void Serialize_String_Escaping ()
    {
      _serializer.Serialize ("te's't");

      Assert.That (_writer.ToString (), Is.EqualTo ("'te''s''t'" + Environment.NewLine));
    }

    [Test]
    public void Serialize_Null ()
    {
      _serializer.Serialize (null);

      Assert.That (_writer.ToString (), Is.EqualTo ("null" + Environment.NewLine));
    }

    [Test]
    public void Serialize_TwoValues ()
    {
      _serializer.Serialize (12);
      _serializer.Serialize (13);

      Assert.That (_writer.ToString (), Is.EqualTo ("12" + Environment.NewLine + "13" + Environment.NewLine));
    }

    [Test]
    public void Serialize_ReferenceType_WithProperties ()
    {
      var instance = new SerializerTestClassWithProperties { PublicProperty1 = 17, PublicProperty2 = "test" };

      _serializer.Serialize (instance);

      var expected = "SerializerTestClassWithProperties" + Environment.NewLine 
          + "  PublicProperty1: 17" + Environment.NewLine 
          + "  PublicProperty2: 'test'" + Environment.NewLine;
      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }

    [Test]
    public void Serialize_ReferenceType_WithFields ()
    {
      var instance = new SerializerTestClassWithFields { PublicField1 = 17, PublicField2 = "test" };

      _serializer.Serialize (instance);

      var expected = "SerializerTestClassWithFields" + Environment.NewLine
          + "  PublicField1: 17" + Environment.NewLine
          + "  PublicField2: 'test'" + Environment.NewLine;
      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }
  }
}