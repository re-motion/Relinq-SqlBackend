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
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.IntegrationTests.TestDomain;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class TestResultSerializerTest
  {
    private StringWriter _writer;
    private TestResultSerializer _serializer;
    private TestResultSerializer _serializerWithLevel1;

    [SetUp]
    public void SetUp ()
    {
      _writer = new StringWriter ();
      _serializer = new TestResultSerializer (_writer);
      _serializerWithLevel1 = new TestResultSerializer (_writer, "..", 1);
    }

    [Test]
    public void Serialize_SimpleValue ()
    {
      _serializer.Serialize (12);

      Assert.That (_writer.ToString (), Is.EqualTo ("12" + Environment.NewLine));
    }

    [Test]
    public void Serialize_WithLevel ()
    {
      _serializerWithLevel1.Serialize (12);

      Assert.That (_writer.ToString (), Is.EqualTo ("..12" + Environment.NewLine));
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
    public void Serialize_Enums ()
    {
      _serializer.Serialize (DateTimeKind.Utc);

      Assert.That (_writer.ToString (), Is.EqualTo ("Utc" + Environment.NewLine));
    }

    [Test]
    public void Serialize_TwoValues ()
    {
      _serializer.Serialize (12);
      _serializer.Serialize (13);

      Assert.That (_writer.ToString (), Is.EqualTo ("12" + Environment.NewLine + "13" + Environment.NewLine));
    }

    [Test]
    public void Serialize_WithName ()
    {
      _serializer.Serialize (12, "Name");

      Assert.That (_writer.ToString (), Is.EqualTo ("Name: 12" + Environment.NewLine));
    }

    [Test]
    public void Serialize_WithName_AndLevel ()
    {
      _serializerWithLevel1.Serialize (12, "Name");

      Assert.That (_writer.ToString (), Is.EqualTo ("..Name: 12" + Environment.NewLine));
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

    [Test]
    public void Serialize_ReferenceType_WithReferenceType ()
    {
      var instance = new SerializerTestClassWithReferenceTypes();
      instance.FieldWithFields = new SerializerTestClassWithFields { PublicField1 = 11, PublicField2 = "test 0" };
      instance.FieldWithProperties = new SerializerTestClassWithProperties { PublicProperty1 = 12, PublicProperty2 = "test 1"};
      instance.PropertyWithFields = new SerializerTestClassWithFields { PublicField1 = 13, PublicField2 = "test 2" };
      instance.PropertyWithProperties = new SerializerTestClassWithProperties { PublicProperty1 = 14, PublicProperty2 = "test 3" };

      _serializer.Serialize (instance);

      var expected =   "SerializerTestClassWithReferenceTypes" + Environment.NewLine // 0
                     + "  FieldWithFields: SerializerTestClassWithFields" + Environment.NewLine // 1
                     + "    PublicField1: 11" + Environment.NewLine // 2
                     + "    PublicField2: 'test 0'" + Environment.NewLine // 2
                     + "  FieldWithProperties: SerializerTestClassWithProperties" + Environment.NewLine // 1
                     + "    PublicProperty1: 12" + Environment.NewLine // 2
                     + "    PublicProperty2: 'test 1'" + Environment.NewLine // 2
                     + "  PropertyWithFields: SerializerTestClassWithFields" + Environment.NewLine // 1
                     + "    PublicField1: 13" + Environment.NewLine // 2
                     + "    PublicField2: 'test 2'" + Environment.NewLine // 2
                     + "  PropertyWithProperties: SerializerTestClassWithProperties" + Environment.NewLine // 1
                     + "    PublicProperty1: 14" + Environment.NewLine // 2
                     + "    PublicProperty2: 'test 3'" + Environment.NewLine; // 2
      
      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }

    // TODO Review: Add a separate test for anonymous types with different numbers of properties

    [Test]
    public void Serialize_SimpleEnumerables ()
    {
      IEnumerable enumerable = new[] { 1, 2, 3 };

      _serializer.Serialize (enumerable);

      var expected = "Enumerable {" + Environment.NewLine
                     + "  1" + Environment.NewLine
                     + "  2" + Environment.NewLine
                     + "  3" + Environment.NewLine
                     + "}" + Environment.NewLine;

      Assert.That (_writer.ToString(), Is.EqualTo (expected));
    }

    [Test]
    public void Serialize_SimpleEnumerables_WithSpacing ()
    {
      IEnumerable enumerable = new[] { 1, 2, 3 };

      _serializerWithLevel1.Serialize (enumerable);

      var expected = "..Enumerable {" + Environment.NewLine
                     + "....1" + Environment.NewLine
                     + "....2" + Environment.NewLine
                     + "....3" + Environment.NewLine
                     + "..}" + Environment.NewLine;

      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }

    [Test]
    public void Serialize_ComplexEnumerable ()
    {
      var instance0 = new SerializerTestClassWithFields { PublicField1 = 11, PublicField2 = "test 0" };
      var instance1 = new SerializerTestClassWithFields { PublicField1 = 12, PublicField2 = "test 1" };
      var enumerable = new[] { instance0, instance1 };

      _serializer.Serialize (enumerable);

      var expected = "Enumerable {" + Environment.NewLine
                     + "  SerializerTestClassWithFields" + Environment.NewLine
                     + "    PublicField1: 11" + Environment.NewLine
                     + "    PublicField2: 'test 0'" + Environment.NewLine
                     + "  SerializerTestClassWithFields" + Environment.NewLine
                     + "    PublicField1: 12" + Environment.NewLine
                     + "    PublicField2: 'test 1'" + Environment.NewLine
                     + "}" + Environment.NewLine;

      Assert.That (_writer.ToString(), Is.EqualTo (expected));
    }
    

    [Test]
    public void Serialize_EnumerableInProperty ()
    {
      var instance0 = new SerializerTestClassWithFields { PublicField1 = 11, PublicField2 = "test 0" };
      var instance1 = new SerializerTestClassWithFields { PublicField1 = 12, PublicField2 = "test 1" };
      var instance = new
                     {
                       EnumerableProperty = new[] { instance0, instance1 }
                     };

      _serializer.Serialize (instance);

      var expected = "AnonymousType0" + Environment.NewLine
                    + "  EnumerableProperty: Enumerable {" + Environment.NewLine
                    + "    SerializerTestClassWithFields" + Environment.NewLine
                    + "      PublicField1: 11" + Environment.NewLine
                    + "      PublicField2: 'test 0'" + Environment.NewLine
                    + "    SerializerTestClassWithFields" + Environment.NewLine
                    + "      PublicField1: 12" + Environment.NewLine
                    + "      PublicField2: 'test 1'" + Environment.NewLine
                    + "  }" + Environment.NewLine;

      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }

    [Test]
    public void Serialize_WithTestMethod ()
    {
      _serializer.Serialize (12, MethodBase.GetCurrentMethod());

      var expected = "Serialize_WithTestMethod:" + Environment.NewLine + "  12";
    }

    [Test]
    public void Serialize_WithMemberFilter ()
    {
      var instance = new SerializerTestClassWithProperties { PublicProperty1 = 17, PublicProperty2 = "test" };
      TestResultSerializer serializer = new TestResultSerializer (_writer, memberInfo => memberInfo.Name.Contains ("1"));

      serializer.Serialize (instance);
      var expected = "SerializerTestClassWithProperties" + Environment.NewLine
          + "  PublicProperty1: 17" + Environment.NewLine;

      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }

    [Test]
    public void Serialize_WithMemberFilterNested ()
    {
      var instance0 = new SerializerTestClassWithFields { PublicField1 = 11, PublicField2 = "test 0" };
      var instance1 = new SerializerTestClassWithFields { PublicField1 = 12, PublicField2 = "test 1" };
      var instance = new
      {
        EnumerableProperty = new[] { instance0, instance1 }
      };
      TestResultSerializer serializer = new TestResultSerializer (_writer, memberInfo => !memberInfo.Name.Equals("PublicField2"));

      serializer.Serialize (instance);
      var expected = "AnonymousType0" + Environment.NewLine
                    + "  EnumerableProperty: Enumerable {" + Environment.NewLine
                    + "    SerializerTestClassWithFields" + Environment.NewLine
                    + "      PublicField1: 11" + Environment.NewLine
                    //+ "      PublicField2: 'test 0'" + Environment.NewLine
                    + "    SerializerTestClassWithFields" + Environment.NewLine
                    + "      PublicField1: 12" + Environment.NewLine
                    //+ "      PublicField2: 'test 1'" + Environment.NewLine
                    + "  }" + Environment.NewLine;

      Assert.That (_writer.ToString (), Is.EqualTo (expected));
    }
  }
}