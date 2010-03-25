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
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Backend.FieldResolving
{
  public abstract class FieldAccessPolicyTestBase
  {
    private MainFromClause _cookClause;
    private QuerySourceReferenceExpression _cookReference;
    
    private MemberInfo _company_Kitchen_Member;
    private MemberInfo _kitchen_Cook_Member;
    private MemberInfo _restaurant_Kitchen_Member;
    private MemberInfo _company_Restaurant_Member;
    private MemberInfo _kitchen_Restaurant_Member;

    public MemberInfo CompanyKitchenMember
    {
      get { return _company_Kitchen_Member; }
    }

    public MemberInfo KitchenCookMember
    {
      get { return _kitchen_Cook_Member; }
    }

    public MainFromClause CookClause
    {
      get { return _cookClause; }
    }

    public QuerySourceReferenceExpression CookReference
    {
      get { return _cookReference; }
    }

    public MemberInfo CompanyRestaurantMember
    {
      get { return _company_Restaurant_Member; }
    }

    public MemberInfo KitchenRestaurantMember
    {
      get { return _kitchen_Restaurant_Member; }
    }

    public MemberInfo RestaurantKitchenMember
    {
      get { return _restaurant_Kitchen_Member; }
    }

    public virtual void SetUp ()
    {
      _cookClause = ExpressionHelper.CreateMainFromClause_Cook ();
      _cookReference = new QuerySourceReferenceExpression (CookClause);
      _company_Kitchen_Member = typeof (Company).GetProperty ("MainKitchen");
      _kitchen_Cook_Member = typeof (Kitchen).GetProperty ("Cook");
      _company_Restaurant_Member = typeof (Company).GetProperty ("Restaurant");
      _kitchen_Restaurant_Member = typeof (Kitchen).GetProperty ("Restaurant");
      _restaurant_Kitchen_Member = typeof (Restaurant).GetProperty ("SubKitchen");
    }
  }
}