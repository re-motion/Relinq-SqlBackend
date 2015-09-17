' Microsoft Public License (Ms-PL)
' 
' This license governs use of the accompanying software. If you use the software, you
' accept this license. If you do not accept the license, do not use the software.
' 
' 1. Definitions
' The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
' same meaning here as under U.S. copyright law.
' A "contribution" is the original software, or any additions or changes to the software.
' A "contributor" is any person that distributes its contribution under this license.
' "Licensed patents" are a contributor's patent claims that read directly on its contribution.
' 
' 2. Grant of Rights
' (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
' each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
' prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
' (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
' each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
' sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
' 
' 3. Conditions and Limitations
' (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
' (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
' such contributor to the software ends automatically.
' (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
' in the software.
' (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
' this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
' license that complies with this license.
' (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
' You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
' the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
Imports System.Linq
Imports System.Reflection
Imports NUnit.Framework
Imports Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind

Namespace SystemTests
  <TestFixture> _
  Public Class ResultOperatorTests
    Inherits TestBase
    <Test> _
    Public Sub Query_WithDistinct()
      Dim query = (From o In DB.Orders _
                   Where o.Employee.ReportsToEmployee IsNot Nothing Select o.Customer) _
                 .Distinct()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithDistinct_NonObjectColumn()
      Dim query = (From o In DB.Orders _
                   Where o.Employee.ReportsToEmployee IsNot Nothing Select o.OrderDate) _
                 .Distinct()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithEntitySetContains()
      Dim orderDetail = DB.Orders.Single(Function(x) x.OrderID = 10248)

      Dim query = From o In DB.Customers _
                  Where o.Orders.Contains(orderDetail) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-99: Support .Count on Collections")> _
    Public Sub Query_WithEntitySetCount()
      Dim query = From o In DB.Orders _
                  Where o.OrderDetails.Count = 2 _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithCastOnResultSet()
      Dim query = (From c In DB.Contacts Where c.ContactID = 1 Select c).Cast(Of FullContact)()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithCastInSubQuery()
      Dim query = From c In (From contact In DB.Contacts _
                             Where Contact.ContactID = 1 _
                             Select Contact).Cast(Of FullContact)() _
                           Where c.City = "Berlin" _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithFirst()
      Dim query = (From o In DB.Orders Order By o.OrderID Select o).First()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithFirst_AndInterface()
      Dim query = (From c In DB.Contacts Order By c.ContactID _
                   Select DirectCast(c, IContact)).First()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithFirst_Throws_WhenNoItems()
      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      Assert.That(Sub()
                    Dim query = (From o In DB.Orders Where False Select o).First()

                    TestExecutor.Execute(query, currentMethod)

                  End Sub, Throws.TypeOf(Of InvalidOperationException)().With.Message.Contains("Sequence contains no elements"))
    End Sub

    <Test> _
    Public Sub QueryWithFirstOrDefault()
      Dim query = (From o In DB.Orders Order By o.OrderID Select o).FirstOrDefault()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithFirstOrDefault_ReturnsNull_WhenNoItems()
      Dim query = (From o In DB.Orders Where False Select o).FirstOrDefault()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSingle()
      Dim query = (From o In DB.Orders Where o.OrderID = 10248 Select o).Single()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSingle_ThrowsException_WhenMoreThanOneElement()
      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      Assert.That(Sub()
                    Dim query = (From o In DB.Orders Select o).[Single]()

                    TestExecutor.Execute(query, currentMethod)

                  End Sub, Throws.TypeOf(Of InvalidOperationException)().With.Message.Contains("Sequence contains more than one element"))
    End Sub

    <Test> _
    Public Sub QueryWithSingleOrDefault_ReturnsSingleItem()
      Dim query = (From o In DB.Orders Where o.OrderID = 10248 Select o).SingleOrDefault()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSingleOrDefault_ReturnsNull_WhenNoItem()
      Dim query = (From o In DB.Orders Where o.OrderID = 999999 Select o).SingleOrDefault()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSingleOrDefault_ThrowsException_WhenMoreThanOneElement()
      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      Assert.That(Sub()
                    Dim query = (From o In DB.Orders Select o).SingleOrDefault()

                    TestExecutor.Execute(query, currentMethod)

                  End Sub, Throws.TypeOf(Of InvalidOperationException)().With.Message.Contains("Sequence contains more than one element"))
    End Sub

    <Test> _
    Public Sub QueryWithCount()
      Dim query = (From o In DB.Orders Select o).Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithCount_InSubquery()
      Dim query = (From o In DB.Orders Where (From od In DB.OrderDetails Where od.Order Is o Select od).Count() = 2 Select o)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryDistinctTest()
      Dim query = (From o In DB.Orders From od In o.OrderDetails Where od.OrderID = 10248 Select o).Distinct()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithConvertToString()
      Dim query = From o In DB.OrderDetails Where Convert.ToString(o.OrderID).Contains("4") Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithArithmeticOperations()
      Dim query = From od In DB.OrderDetails Where (od.Quantity + od.Quantity) = 30 Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubstring()
      Dim query = From c In DB.Customers Where c.ContactName.Substring(1, 3).Contains("Ana") Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithTake()
      Dim query = (From o In DB.Orders Order By o.OrderID Select o).Take(3)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithTake_SubQueryAsArgument()
      Dim query = From o In DB.Orders _
                  From od In o.OrderDetails.Take(o.OrderDetails.Where(Function(od) od.Quantity < 25).Count()) _
                  Where o.OrderID = 10248 _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithContainsInWhere_OnCollection()
      Dim possibleItems = {10248, 10249, 10250}

      Dim orders = From o In DB.Orders Where possibleItems.Contains(o.OrderID) Select o

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithContainsInWhere_OnEmptyCollection()
      Dim possibleItems = New Object(-1) {}

      Dim orders = From o In DB.Orders Where possibleItems.Contains(o) Select o

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithContainsInWhere_OnCollection_WithObjectIDs()
      Dim possibleItems = {10248, 10249}

      Dim orders = From o In DB.Orders Where possibleItems.Contains(o.OrderID) Select o

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithSupportForObjectList()
      Dim orders = (From o In DB.Orders From od In DB.OrderDetails Where od.Order Is o Select o).Distinct()

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithOfType_SelectingBaseType()
      Dim query = DB.Contacts.OfType(Of EmployeeContact)().OfType(Of FullContact)()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithOfType_SameType()
      Dim query = DB.Customers.OfType(Of Customer)()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithOfType_UnrelatedType()
      Dim query = DB.Customers.OfType(Of Order)()

      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      If IsLinqToSqlActive Then
        TestExecutor.Execute(query, currentMethod)
      ElseIf IsRelinqSqlBackendActive Then
        Assert.That(Sub()

                      TestExecutor.Execute(query, currentMethod)

                    End Sub, Throws.TypeOf(Of InvalidOperationException)())
      End If
    End Sub

    <Test> _
    Public Sub QueryWithAny_WithoutPredicate()
      Dim query = DB.Orders.Any()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAny_WithPredicate()
      Dim query = DB.Contacts.Any(Function(c) c.ContactID = 1)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAny_InSubquery()
      Dim query = From o In DB.Orders Where Not o.OrderDetails.Any() Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAll()
      Dim query = DB.Customers.All(Function(c) c.City = "Berlin")

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithAll_WithConditionStringNotEmpty()
      Dim query = DB.Customers.All((Function(c) c.Fax <> String.Empty AndAlso c.Fax IsNot Nothing))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAll_AfterIncompatibleResultOperator()
      Dim query = DB.Customers.Take(10).Take(20).All(Function(c) c.City = "Berlin")

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithOrderBy_BeforeDistinct()
      Dim query = DB.Customers.OrderBy(Function(c) c.City).Distinct().Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithOrderBy_BeforeCount()
      Dim query = DB.Customers.OrderBy(Function(c) c.City).Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithOrderBy_BeforeCount_DueToIncompatibleResultOperators()
      Dim query = DB.Customers.OrderBy(Function(c) c.City).Take(10).Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAll_InSubquery()
      Dim query = From o In DB.Orders Where o.OrderDetails.All(Function(od) od.ProductID = 11) Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithAll_InSubquery_CountInAll()
      Dim query = From c In DB.Customers _
                  Where c.Orders.All(Function(o) DirectCast(o.OrderDetails, IEnumerable(Of OrderDetail)).Count() > 0) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub DefaultIsEmpty_WithoutJoin()
      Dim query = (From o In DB.Orders Where o.OrderID = 10248 Select o).DefaultIfEmpty()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-101: DefaultIfEmpty on empty resultset returns Entity with all Fields null instead of just null")> _
    Public Sub DefaultIsEmpty_WithoutJoin_EmptyResult()
      Dim query = (From o In DB.Orders Where o.OrderID = -1 Select o).DefaultIfEmpty()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub DefaultIsEmpty_WithJoin()
      Dim query = (From o In DB.Orders _
                   Group Join c In DB.Customers On o.Customer Equals c Into goc = Group _
                   From oc In goc.DefaultIfEmpty() _
                   Where o.OrderID = 10248 Select oc)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Max_OnTopLevel()
      Dim query = (From o In DB.Orders Select o.OrderID).Max()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Max_InSubquery()
      Dim query = (From o In DB.Orders Where (From s2 In DB.Orders Select s2.OrderID).Max() = o.OrderID Select o)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Max_WithStrings()
      Dim query = DB.Customers.Max(Function(c) c.ContactName)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Max_WithDateTimes()
      Dim query = DB.Orders.Max(Function(o) o.OrderDate)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Max_WithNullableInt()
      Dim query = DB.Employees.Max(Function(o) o.ReportsTo)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Min_OnTopLevel()
      Dim query = (From o In DB.Orders Select o.OrderID).Min()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Min_InSubquery()
      Dim query = (From o In DB.Orders Where (From s2 In DB.Orders Select s2.OrderID).Min() = o.OrderID Select o)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-102: .Average DataType conversion")> _
    Public Sub Average_OnTopLevel_WithIntProperty()
      Dim average As Double = (From o In DB.Orders Where o.OrderID <= 10255 Select o).Average(Function(o) o.OrderID)

      If IsLinqToSqlActive Then
        'Linq to SQL rounds to an int
        Assert.That(average, [Is].EqualTo(10251))
      ElseIf IsRelinqSqlBackendActive Then
        Assert.That(average, [Is].EqualTo(10251.5))
      End If
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-102: .Average DataType conversion")> _
    Public Sub Average_OnTopLevel_WithIntProperty_CastToFloat()
      Dim query = (From o In DB.Orders Where o.OrderID <= 10255 Select o).Average(Function(o) CSng(o.OrderID))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Average_InSubquery_WithIntProperty()
      Dim query = From c In DB.Customers Where c.Orders.Average(Function(o) o.OrderID) = 1.5 Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Sum_OnTopLevel()
      Dim query = (From o In DB.Orders Select o).Sum(Function(o) o.OrderID)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-100: Sum with empty Result Set and aggregated value is not nullable property should throw not supported exception")> _
    Public Sub Sum_WithEmptyResultSet_AndAggregatedValueIsNotNullableProperty_ThrowsNotSupportedException()
      If IsLinqToSqlActive Then
        Assert.That(Sub()
                      Dim query = (From o In DB.Orders Where o.OrderID = -1 Select o).Sum(Function(o) o.OrderID)

                    End Sub, Throws.Exception.[TypeOf](Of InvalidOperationException)().[With].Message.EqualTo("The null value cannot be assigned to a member with type System.Int32 which is a non-nullable value type."))
      ElseIf IsRelinqSqlBackendActive Then
        Assert.That(Sub()
                      Dim query = (From o In DB.Orders Where o.OrderID = -1 Select o).Sum(Function(o) o.OrderID)

                    End Sub, Throws.Exception.[TypeOf](Of NotSupportedException)().[With].Message.EqualTo("Null cannot be converted to type 'System.Int32'."))
      End If
    End Sub


    <Test>
    <Ignore("Cast to nullable not possible in VB.Net")> _
    Public Sub Sum_WithEmptyResultSet_AndAggregatedValueIsNotNullablePropertyButCastToNullable_ReturnsNull()
      'Dim query = (From o In DB.Orders Where o.OrderID = -1 Select o).Sum(Function(o) DirectCast(o.OrderID, System.Nullable(Of Integer)))

      'TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Sum_InSubquery()
      Dim query = (From o In DB.Orders Where (From s2 In DB.Orders Select s2.OrderID).Sum() = 20497 Select o)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Skip_WithEntity()
      Dim query = (From o In DB.Orders Order By o.OrderID Select o).Skip(6)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Skip_WithEntity_WithoutExplicitOrdering()
      Dim query = (From o In DB.Orders Select o).Skip(6).Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub TakeAfterSkip()
      Dim query = (From o In DB.Orders Order By o.OrderID Select o).Skip(3).Take(2)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithCastToInterface_ThrowsNoException()
      Dim query = (From c In DB.Contacts Select c).Cast(Of IContact)()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace