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
  <TestFixture>
  Public Class GroupByTests
    Inherits TestBase

    <Test> _
    Public Sub GroupBy_WithAggregateFunction()
      Dim query = DB.OrderDetails.GroupBy(Function(o) o.OrderID).Count()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_GroupingWithSeveralAggregateFunction()
      Dim query =
            From od In DB.OrderDetails _
            Group od By groupKey = od.OrderID Into orderDetailsByOrderID = Group _
            Select New With { _
            .OrderID = groupKey, _
            .Count = orderDetailsByOrderID.Count(), _
            .Sum = orderDetailsByOrderID.Sum(Function(o) o.Quantity), _
            .Min = orderDetailsByOrderID.Min(Function(o) o.Quantity) _
            }

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_TopLevel()
      Dim query = DB.Orders.GroupBy(Function(o) o.OrderID)

      Dim currentMethod As MethodBase = MethodBase.GetCurrentMethod()

      If IsLinqToSqlActive Then

        TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
      ElseIf IsRelinqSqlBackendActive Then
        Assert.That(Sub()
          TestExecutor.Execute(query, currentMethod)
                     End Sub, Throws.[TypeOf] (Of NotSupportedException)().[With].Message.EqualTo _
                     (
                       "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to " +
                       "efficiently return " +
                       "LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional from clause, or perform " +
                       "an aggregation on the groupings. " + Environment.NewLine + Environment.NewLine +
                       "Eg., instead of: " + Environment.NewLine +
                       "'from c in Cooks group c.ID by c.Name', " + Environment.NewLine + "write: " +
                       Environment.NewLine +
                       "'from c in Cooks group c.ID by c.Name into groupedCooks " + Environment.NewLine +
                       " from c in groupedCooks select new { Key = groupedCooks.Key, Item = c }', " +
                       Environment.NewLine + "or: " + Environment.NewLine +
                       "'from c in Cooks group c.ID by c.Name into groupedCooks " + Environment.NewLine +
                       " select new { Key = groupedCooks.Key, Count = groupedCooks.Count() }'."))
      End If
    End Sub

    <Test> _
    Public Sub GroupBy_WithinSubqueryInFromClause()
      Dim query =
            From ordersByCustomer In DB.Orders.GroupBy(Function(o) o.Customer)
            Where ordersByCustomer.Key.ContactTitle.StartsWith("Sales") _
            Select New With { _
            ordersByCustomer.Key.ContactTitle, _
            Key .Count = ordersByCustomer.Count() _
            }

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_UseGroupInFromExpression()
      Dim query =
            From o In DB.Orders _
            Group o.OrderID By groupKey = o.CustomerID _
            Into orderByCustomerID = Group _
            From id In orderByCustomerID Select New With { _
            .Key = groupKey, _
            .OrderID = id _
            }

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupByUseGroupInFromExpression_WithObject()
      Dim query =
            From o In DB.Orders _
            Group o By groupKey = o.OrderID _
            Into orderByOrderId = Group _
            From o In orderByOrderId _
            Where o IsNot Nothing _
            Select New With { _
            .Key = groupKey, _
            .Order = o _
            }

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_UseGroupInFromExpression_WithSubQuery()
      Dim query = From o In DB.Orders _
            Group o.OrderID By groupKey = o.OrderID _
            Into orderByOrderID = Group _
            From o In (From so In orderByOrderID Select so).Distinct() _
            Select New With { _
            .Key = groupKey, _
            .Order = o _
            }

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_ResultSelector()
      Dim query = DB.Orders.GroupBy(Function(o) o.CustomerID, Function(key, group) key)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_WithSubqueryKey()
      Dim query = (From o In DB.Orders _
            Group o By groupKey = DB.OrderDetails _
            .Where(Function(od) od.Order Is o).[Select](Function(od) od.Product) _
            .Count() Into orderOrderDetails = Group).[Select](Function(g) g.groupKey)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_WithConstantKey()
      Dim query = DB.Orders.GroupBy(Function(o) 0).[Select](Function(c) c.Key)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_NonEntityKey()
      Dim query = From o In DB.Orders _
            Group o By groupKey = o.Customer.CustomerID _
            Into ordersByCustomer = Group _
            From c In DB.Customers _
            Where c.CustomerID = groupKey _
            Select c

      'Make query stable because of ordering
      Dim stableResult =
            query.AsEnumerable().OrderBy(Function(t) t.CustomerID).ThenBy(Function(t) t.PostalCode).ThenBy(
              Function(t) t.ContactTitle)

      TestExecutor.Execute(stableResult, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_EntityKey()
      Dim query =
            From o In DB.Orders _
            Group o By groupKey = o.Customer _
            Into ordersByCustomer = Group _
            Where groupKey IsNot Nothing _
            Select groupKey.ContactName

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_EntityKey_WithEmptySet()
      Dim query =
            From o In DB.Orders _
            Where o.OrderID <> -1 _
            Group o By groupKey = o.Customer Into ordersByCustomer = Group _
            Select groupKey

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_EntityKey_CustomProjection()
      Dim query =
            From o In DB.Orders _
            From od In o.OrderDetails _
            Group od.OrderID By groupKey = od _
            Into orderDetailsByOrder = Group _
            From order In orderDetailsByOrder _
            Select New With { _
            groupKey.Quantity, _
            .OrderID = order _
            }

      Dim stableResult = query.AsEnumerable().OrderBy(Function(t) t.OrderID).ThenBy(Function(t) t.Quantity)

      TestExecutor.Execute(stableResult, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_EntityKey_NotNull()
      Dim query =
            From e In DB.Employees _
            Group e By groupKey = e.ReportsToEmployee _
            Into employeesByReport = Group _
            Where groupKey IsNot Nothing _
            Select groupKey.City

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub GroupBy_AccessKey_Nesting()
      Dim query =
            From o In DB.Orders _
            From x In _
            (From c In o.OrderDetails Group c By groupKey = c.Quantity Into ordersByCustomer = Group _
            Select New With { _
            .OrderID = o.OrderID, _
            .OrderDetail = groupKey _
            }) Let customerName = x.OrderDetail
            Where customerName = 25 _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace