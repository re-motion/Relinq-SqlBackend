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
using System.Data.Linq.Mapping;
using NUnit.Framework;
using Remotion.Linq.LinqToSqlAdapter;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class MetaDataMemberComparerTest
  {
    [Test]
    public void Equals ()
    {
      const string name = "equal";

      var metaDataMember1 = MockRepository.GenerateStub<MetaDataMember>();
      metaDataMember1.Stub (dataMember => dataMember.MappedName).Return (name);

      var metaDataMember2 = MockRepository.GenerateStub<MetaDataMember>();
      metaDataMember2.Stub (dataMember => dataMember.MappedName).Return (name);

      var comparer = new MetaDataMemberComparer();

      Assert.IsTrue (comparer.Equals (metaDataMember1, metaDataMember2));
    }

    [Test]
    public void Equals_ShouldReturnFalse ()
    {
      const string name = "equal";
      const string otherName = "notequal";

      var metaDataMember1 = MockRepository.GenerateStub<MetaDataMember>();
      metaDataMember1.Stub (dataMember => dataMember.MappedName).Return (name);

      var metaDataMember2 = MockRepository.GenerateStub<MetaDataMember>();
      metaDataMember2.Stub (dataMember => dataMember.MappedName).Return (otherName);

      var comparer = new MetaDataMemberComparer();

      Assert.IsFalse (comparer.Equals (metaDataMember1, metaDataMember2));
    }
  }
}