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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Backend.FieldResolving
{
  [TestFixture]
  public class SelectFieldAccessPolicyTest : FieldAccessPolicyTestBase
  {
    private SelectFieldAccessPolicy _policy;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();
      _policy = new SelectFieldAccessPolicy ();
    }

    [Test]
    public void AdjustMemberInfosForDirectAccessOfQuerySource ()
    {
      var result = _policy.AdjustMemberInfosForDirectAccessOfQuerySource (CookReference);
      Assert.That (result.AccessedMember, Is.Null);
      Assert.That (result.JoinedMembers, Is.Empty);
    }

    [Test]
    public void AdjustMemberInfosForRelation()
    {
      var result = _policy.AdjustMemberInfosForRelation (new[] { CompanyKitchenMember }, KitchenCookMember);
      var expected = new MemberInfoChain (new[] {CompanyKitchenMember, KitchenCookMember}, null);

      Assert.AreEqual (expected.AccessedMember, result.AccessedMember);
      Assert.That (result.JoinedMembers, Is.EqualTo (expected.JoinedMembers));
    }

    [Test]
    public void OptimizeRelatedKeyAccess_False ()
    {
      Assert.That (_policy.OptimizeRelatedKeyAccess (), Is.False);
    }
  }
}
