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

Namespace SystemTests
  <TestFixture> _
  Public Class SubQueryTests
    Inherits TestBase
    <Test> _
    Public Sub QueryWithSubQuery_InWhere()
      Dim query = From o In DB.Orders _
                  Where (From c In DB.Customers Select c).Contains(o.Customer) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQueryInWhere_AccessingOuterVariable_InMainFromClause()
      Dim orders = DB.Orders.Single(Function(x) x.OrderID = 10248)

      Dim query = From c In DB.Customers _
                  Where (From o In c.Orders Select o).Contains(orders) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQueryAndJoinInWhere()
      Dim query = From o In DB.Orders _
                  Where (From od In DB.OrderDetails Select od.Order).Contains(o) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQueryAndJoinInWhere_WithOuterVariable()
      Dim outerCustomer = DB.Orders.[Single](Function(x) x.OrderID = 10248)

      Dim query = From customer In DB.Customers _
                  Where (From order In DB.Orders Where order.Customer Is customer Select order).Contains(outerCustomer) _
                  Select customer

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQuery_InMainFrom()
      Dim query = From c In (From cu In DB.Customers Select cu) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQuery_WithResultOperator_InMainFrom()
      Dim query = From c In (From ord In DB.Orders Order By ord.OrderID Select ord).Take(1) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQuery_InAdditionalFrom()
      Dim query = From o In DB.Orders _
                  From od In (From od In DB.OrderDetails Where od.Order Is o Select od) _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQuery_InThirdFrom()
      Dim query = (From o1 In DB.Orders _
                   From o2 In DB.Orders _
                   From od In (From od In DB.OrderDetails _
                               Where od.Order Is o1 OrElse od.Order Is o2 _
                               Select od) _
                             Select od).Distinct()

      'Make query stable because of ordering
      Dim stableResult = query.AsEnumerable().OrderBy(Function(o) o.OrderID).ThenBy(Function(o) o.ProductID)

      TestExecutor.Execute(stableResult, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSubQuery_InSelectClause()
      Dim query = From o In DB.Orders _
                  Select (From p In DB.Shippers Select p)

      'Get currentMethod because calling it inside a lambda returns a false result
      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      If IsLinqToSqlActive Then
        TestExecutor.Execute(query, currentMethod)
      ElseIf IsRelinqSqlBackendActive Then
        Assert.That(Sub() TestExecutor.Execute(query, currentMethod), Throws.TypeOf(Of NotSupportedException)() _
                    .With.Message.EqualTo _
                    ("Queries selecting collections are not supported because SQL is not well-suited to returning collections. You can use " +
                     "SelectMany or an additional 'from' clause to return the elements of the collection, grouping them in-memory." +
                     Environment.NewLine + Environment.NewLine + "Ie., instead of 'from c in Cooks select c.Assistants', write the following query: " +
                     Environment.NewLine +
                     "'(from c in Cooks from a in Assistants select new { GroupID = c.ID, Element = a }).AsEnumerable().GroupBy (t => t.GroupID, t => t.Element)'" +
                     Environment.NewLine +
                     Environment.NewLine +
                     "Note that above query will group the query result in-memory, which might be inefficient, depending on the number of results returned " +
                     "by the query."))
      End If
    End Sub

    <Test> _
    Public Sub SubQueryWithNonConstantFromExpression()
      Dim query = From o In DB.Orders _
                  From od In (From od1 In o.OrderDetails Select od1) Where o.OrderID = 10248 _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub FirstOrDefault_WithEntity_InSelectAndWhere()
      Dim query = From c In DB.Customers _
                  Where c.Orders.FirstOrDefault() IsNot Nothing _
                  Select c.Orders.FirstOrDefault()

      'Make query stable because of ordering
      Dim stableResult = query.AsEnumerable().OrderBy(Function(t) t.OrderDate).ThenBy(Function(t) t.Freight).ThenBy(Function(t) t.OrderID)

      TestExecutor.Execute(stableResult, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub OrderingsInSubQuery_WithDistinct()
      Dim query = From o In (From od In DB.OrderDetails _
                             Where od.Order IsNot Nothing Order By od.Order.OrderID Select od.Order).Distinct() _
                           Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub OrderingsInSubQuery_WithTake()
      Dim query = From o In (From o In DB.Orders Order By o.OrderID Select o).Take(2) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub OrderingsInSubQuery_WithoutTakeOrDistinct()
      Dim query = From c In DB.Customers _
                  Where c.CustomerID = "ALFKI" _
                  From o In (From o In c.Orders Select o) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub OrderingsInSubQuery_WithoutTakeOrDistinct_WithAccessToMemberOfSubQuery()
      Dim query = From c In DB.Customers _
                  Where c.CustomerID = "ALFKI" _
                  From o In (From o In c.Orders Select o) _
                  Where o.OrderID < 10249 _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub MemberAccess_OnSubQuery_WithEntities()
      Dim query = (From o In DB.Orders _
                   Where o.OrderID = 10248 _
                   Select (From od In o.OrderDetails 
                           Select od).First().Product _
                  ).Single()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub MemberAccess_OnSubQuery_WithColumns()
      Dim query = (From o In DB.Orders _
                   Where o.OrderID = 10248 _
                   Select (From od In o.OrderDetails 
                           Select od.Order.CustomerID).First().Length _
                  ).Single()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace