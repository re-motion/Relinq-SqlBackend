// Microsoft Public License (Ms-PL)
// 
// This license governs use of the accompanying software. If you use the software, you
// accept this license. If you do not accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
// same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
// prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
// sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
// such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
// in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
// this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
// license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
// the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind;


namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class ResultOperatorTests : TestBase
  {
    [Test]
    public void Query_WithDistinct ()
    {
      var query =
          (from o in DB.Orders
            where o.Employee.ReportsToEmployee != null
            select o.Customer).Distinct();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithDistinct_NonObjectColumn ()
    {
      var query =
          (from o in DB.Orders
            where o.Employee.ReportsToEmployee != null
            select o.OrderDate).Distinct();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithEntitySetContains ()
    {
      var orderDetail = DB.Orders.Single (x => x.OrderID == 10248);

      var query =
          from o in DB.Customers
          where o.Orders.Contains (orderDetail)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-99: Support .Count on Collections")]
    public void Query_WithEntitySetCount ()
    {
      var query =
          from o in DB.Orders
          where o.OrderDetails.Count == 2
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithCastOnResultSet ()
    {
      var query =
          (from c in DB.Contacts
            where c.ContactID == 1
            select c).Cast<FullContact>();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithCastInSubQuery ()
    {
      var query =
          from c in
              (from contact in DB.Contacts
                where contact.ContactID == 1
                select contact).Cast<FullContact>()
          where c.City == "Berlin"
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithFirst ()
    {
      var query =
          (from o in DB.Orders
            orderby o.OrderID
            select o).First();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithFirst_AndInterface ()
    {
      var query =
          (from c in DB.Contacts
            orderby c.ContactID
            select (IContact) c).First();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithFirst_Throws_WhenNoItems ()
    {
      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      Assert.That (
          () =>
          {
            var query =
                (from o in DB.Orders
                  where false
                  select o
                    ).First();

            TestExecutor.Execute (query, currentMethod);
          },
          Throws.TypeOf<InvalidOperationException>().With.Message.Contains ("Sequence contains no elements"));
    }

    [Test]
    public void QueryWithFirstOrDefault ()
    {
      var query =
          (from o in DB.Orders
            orderby o.OrderID
            select o).FirstOrDefault();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithFirstOrDefault_ReturnsNull_WhenNoItems ()
    {
      var query =
          (from o in DB.Orders
            where false
            select o).FirstOrDefault();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSingle ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select o).Single();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSingle_ThrowsException_WhenMoreThanOneElement ()
    {
      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      Assert.That (
          () =>
          {
            var query =
                (from o in DB.Orders
                  select o
                    ).Single();

            TestExecutor.Execute (query, currentMethod);
          },
          Throws.TypeOf<InvalidOperationException>()
              .With.Message.Contains ("Sequence contains more than one element"));
    }

    [Test]
    public void QueryWithSingleOrDefault_ReturnsSingleItem ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select o).SingleOrDefault();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSingleOrDefault_ReturnsNull_WhenNoItem ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 999999
            select o).SingleOrDefault();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSingleOrDefault_ThrowsException_WhenMoreThanOneElement ()
    {
      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      Assert.That (
          () =>
          {
            var query =
                (from o in DB.Orders
                  select o).SingleOrDefault();

            TestExecutor.Execute (query, currentMethod);
          },
          Throws.TypeOf<InvalidOperationException>()
              .With.Message.Contains ("Sequence contains more than one element"));
    }

    [Test]
    public void QueryWithCount ()
    {
      var query =
          (from o in DB.Orders
            select o).Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithCount_InSubquery ()
    {
      var query =
          (from o in DB.Orders
            where (from od in DB.OrderDetails where od.Order == o select od).Count() == 2
            select o);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryDistinctTest ()
    {
      var query =
          (from o in DB.Orders
            from od in o.OrderDetails
            where od.OrderID == 10248
            select o).Distinct();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithConvertToString ()
    {
      var query =
          from o in DB.OrderDetails
          where Convert.ToString (o.OrderID).Contains ("4")
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithArithmeticOperations ()
    {
      var query =
          from od in DB.OrderDetails
          where (od.Quantity + od.Quantity) == 30
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithSubstring ()
    {
      var query =
          from c in DB.Customers
          where c.ContactName.Substring (1, 3).Contains ("Ana")
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithTake ()
    {
      var query =
          (from o in DB.Orders
            orderby o.OrderID
            select o).Take (3);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithTake_SubQueryAsArgument ()
    {
      var query =
          from o in DB.Orders
          from od in o.OrderDetails.Take (o.OrderDetails.Where(od => od.Quantity < 25).Count())
          where o.OrderID == 10248
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContainsInWhere_OnCollection ()
    {
      var possibleItems = new[] { 10248, 10249, 10250 };

      var orders =
          from o in DB.Orders
          where possibleItems.Contains (o.OrderID)
          select o;

      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContainsInWhere_OnEmptyCollection ()
    {
      var possibleItems = new object[0];

      var orders =
          from o in DB.Orders
          where possibleItems.Contains (o)
          select o;

      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithContainsInWhere_OnCollection_WithObjectIDs ()
    {
      var possibleItems = new[] { 10248, 10249 };

      var orders =
          from o in DB.Orders
          where possibleItems.Contains (o.OrderID)
          select o;

      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithSupportForObjectList ()
    {
      var orders =
          (from o in DB.Orders
            from od in DB.OrderDetails
            where od.Order == o
            select o).Distinct();

      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithOfType_SelectingBaseType ()
    {
      var query = DB.Contacts.OfType<EmployeeContact>().OfType<FullContact>();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithOfType_SameType ()
    {
      var query = DB.Customers.OfType<Customer>();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithOfType_UnrelatedType ()
    {
      var query = DB.Customers.OfType<Order>();

      MethodBase currentMethod = MethodBase.GetCurrentMethod();

      if (IsLinqToSqlActive)
      {
        TestExecutor.Execute (query, currentMethod);
      }
      else if (IsRelinqSqlBackendActive)
      {
        Assert.That (
            () => { TestExecutor.Execute (query, currentMethod); },
            Throws.TypeOf<InvalidOperationException>());
      }
    }

    [Test]
    public void QueryWithAny_WithoutPredicate ()
    {
      var query = DB.Orders.Any();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAny_WithPredicate ()
    {
      var query = DB.Contacts.Any (c => c.ContactID == 1);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAny_InSubquery ()
    {
      var query =
          from o in DB.Orders
          where !o.OrderDetails.Any()
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAll ()
    {
      var query = DB.Customers.All (c => c.City == "Berlin");

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Query_WithAll_WithConditionStringNotEmpty ()
    {
      var query = DB.Customers.All ((c => c.Fax != string.Empty && c.Fax != null));

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAll_AfterIncompatibleResultOperator ()
    {
      var query = DB.Customers.Take (10).Take (20).All (c => c.City == "Berlin");

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithOrderBy_BeforeDistinct ()
    {
      var query = DB.Customers.OrderBy (c => c.City).Distinct().Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithOrderBy_BeforeCount ()
    {
      var query = DB.Customers.OrderBy (c => c.City).Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithOrderBy_BeforeCount_DueToIncompatibleResultOperators ()
    {
      var query = DB.Customers.OrderBy (c => c.City).Take (10).Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAll_InSubquery ()
    {
      var query =
          from o in DB.Orders
          where o.OrderDetails.All (od => od.ProductID == 11)
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithAll_InSubquery_CountInAll ()
    {
      //ReSharper disable UseMethodAny.0
      var query =
          from c in DB.Customers
          where c.Orders.All (o => o.OrderDetails.Count() > 0)
          select c;
      //ReSharper restore UseMethodAny.0

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void DefaultIsEmpty_WithoutJoin ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == 10248
            select o).DefaultIfEmpty();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-101: DefaultIfEmpty on empty resultset returns Entity with all Fields null instead of just null")]
    public void DefaultIsEmpty_WithoutJoin_EmptyResult ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == -1
            select o).DefaultIfEmpty();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void DefaultIsEmpty_WithJoin ()
    {
      var query =
          (from o in DB.Orders
            join c in DB.Customers on o.Customer equals c into goc
            from oc in goc.DefaultIfEmpty()
            where o.OrderID == 10248
            select oc);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Max_OnTopLevel ()
    {
      var query =
          (from o in DB.Orders
            select o.OrderID).Max();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Max_InSubquery ()
    {
      var query =
          (from o in DB.Orders
            where (from s2 in DB.Orders select s2.OrderID).Max() == o.OrderID
            select o);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Max_WithStrings ()
    {
      var query = DB.Customers.Max (c => c.ContactName);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Max_WithDateTimes ()
    {
      var query = DB.Orders.Max (o => o.OrderDate);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Max_WithNullableInt ()
    {
      var query = DB.Employees.Max (o => (int?) o.ReportsTo);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Min_OnTopLevel ()
    {
      var query =
          (from o in DB.Orders
            select o.OrderID).Min();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Min_InSubquery ()
    {
      var query =
          (from o in DB.Orders
            where (from s2 in DB.Orders select s2.OrderID).Min() == o.OrderID
            select o);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-102: .Average DataType conversion")]
    public void Average_OnTopLevel_WithIntProperty ()
    {
      double average =
          (from o in DB.Orders
            where o.OrderID <= 10255
            select o).Average (o => o.OrderID);

      if (IsLinqToSqlActive)
      {
        //Linq to SQL rounds to an int
        Assert.That (average, Is.EqualTo (10251));
      }
      else if (IsRelinqSqlBackendActive)
      {
        Assert.That (average, Is.EqualTo (10251.5));
      }
    }

    [Test]
    [Ignore ("RMLNQSQL-102: .Average DataType conversion")]
    public void Average_OnTopLevel_WithIntProperty_CastToFloat ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID <= 10255
            select o).Average (o => (float) o.OrderID);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Average_InSubquery_WithIntProperty ()
    {
      var query =
          from c in DB.Customers
          where c.Orders.Average (o => o.OrderID) == 1.5
          select c;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Sum_OnTopLevel ()
    {
      var query =
          (from o in DB.Orders
            select o).Sum (o => o.OrderID);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    [Ignore ("RMLNQSQL-100: Sum with empty Result Set and aggregated value is not nullable property should throw not supported exception")]
    public void Sum_WithEmptyResultSet_AndAggregatedValueIsNotNullableProperty_ThrowsNotSupportedException ()
    {
      if (IsLinqToSqlActive)
      {
        Assert.That (
            () =>
            {
              var query =
                  (from o in DB.Orders
                    where o.OrderID == -1
                    select o).Sum (o => o.OrderID);
            },
            Throws.Exception.TypeOf<InvalidOperationException>()
                .With.Message.EqualTo ("The null value cannot be assigned to a member with type System.Int32 which is a non-nullable value type."));
      }
      else if (IsRelinqSqlBackendActive)
      {
        Assert.That (
            () =>
            {
              var query =
                  (from o in DB.Orders
                    where o.OrderID == -1
                    select o).Sum (o => o.OrderID);
            },
            Throws.Exception.TypeOf<NotSupportedException>()
                .With.Message.EqualTo ("Null cannot be converted to type 'System.Int32'."));
      }
    }

    [Test]
    public void Sum_WithEmptyResultSet_AndAggregatedValueIsNotNullablePropertyButCastToNullable_ReturnsNull ()
    {
      var query =
          (from o in DB.Orders
            where o.OrderID == -1
            select o).Sum (o => (int?) o.OrderID);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Sum_InSubquery ()
    {
      var query =
          (from o in DB.Orders
            where (from s2 in DB.Orders select s2.OrderID).Sum() == 20497
            select o);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Skip_WithEntity ()
    {
      var query =
          (from o in DB.Orders
            orderby o.OrderID
            select o).Skip (6);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void Skip_WithEntity_WithoutExplicitOrdering ()
    {
      var query =
          (from o in DB.Orders
            select o).Skip (6).Count();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void TakeAfterSkip ()
    {
      var query =
          (from o in DB.Orders
            orderby o.OrderID
            select o).Skip (3).Take (2);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void QueryWithCastToInterface_ThrowsNoException ()
    {
      var query =
          (from c in DB.Contacts
            select c).Cast<IContact>();

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}