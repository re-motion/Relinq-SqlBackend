// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Data.Linq.Mapping;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.LinqToSqlAdapter;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class MetaDataMemberComparerTest
  {
    [Test]
    public void Equals ()
    {
      const string name = "equal";

      MetaDataMember metaDataMember1 = MockRepository.GenerateStub<MetaDataMember>();
      metaDataMember1.Stub (dataMember => dataMember.MappedName).Return (name);

      MetaDataMember metaDataMember2 = MockRepository.GenerateStub<MetaDataMember> ();
      metaDataMember2.Stub (dataMember => dataMember.MappedName).Return (name);

      MetaDataMemberComparer comparer=new MetaDataMemberComparer();

      Assert.IsTrue (comparer.Equals (metaDataMember1, metaDataMember2));
    }

    [Test]
    public void Equals_ShouldReturnFalse ()
    {
      const string name = "equal";
      const string otherName = "notequal";

      MetaDataMember metaDataMember1 = MockRepository.GenerateStub<MetaDataMember> ();
      metaDataMember1.Stub (dataMember => dataMember.MappedName).Return (name);

      MetaDataMember metaDataMember2 = MockRepository.GenerateStub<MetaDataMember> ();
      metaDataMember2.Stub (dataMember => dataMember.MappedName).Return (otherName);

      MetaDataMemberComparer comparer = new MetaDataMemberComparer ();

      Assert.IsFalse (comparer.Equals (metaDataMember1, metaDataMember2));
    }
  }
}