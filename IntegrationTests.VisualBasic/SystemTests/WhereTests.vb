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
  Public Class WhereTests
    Inherits TestBase
    <Test> _
    Public Sub QueryWithStringLengthProperty()
      Dim query = From c In DB.Customers _
                  Where c.City.Length = "London".Length _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithStringIsNullOrEmpty()
      Dim query As IQueryable = Nothing

      If IsLinqToSqlActive Then
        'LinqToSQL has string.IsNullOrEmpty not implemented

        query = From c In DB.Customers _
                Where Not (c.Region Is Nothing OrElse c.Region = String.Empty) _
                Select c
      ElseIf IsRelinqSqlBackendActive Then
        query = From c In DB.Customers _
                Where Not String.IsNullOrEmpty(c.Region) _
                Select c
      End If

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereConditions()
      Dim query = From c In DB.Customers _
                  Where c.City = "Berlin" OrElse c.City = "London" _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereConditionsAndNull()
      Dim query = From c In DB.Customers _
                  Where c.Region IsNot Nothing _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereConditionAndStartsWith()
      Dim query = From c In DB.Customers _
                  Where c.PostalCode.StartsWith("H1J") _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereConditionAndStartsWith_NonConstantValue()
      Dim query = From c In DB.Customers _
                  Where c.PostalCode.StartsWith(DB.Customers.Select(Function(x) x.Fax.Substring(0, 3)).First()) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereConditionAndEndsWith()
      Dim query = From c In DB.Customers _
                  Where c.PostalCode.EndsWith("876") _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereAndEndsWith_NonConstantValue()
      Dim query = From c In DB.Customers _
                  Where c.City.EndsWith(DB.Customers.Select(Function(x) x.City.Substring(x.PostalCode.Length - 2)).First()) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithContains_Like()
      Dim query = From c In DB.Customers _
                  Where c.CompanyName.Contains("restauration") _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-104: Support conversion operator for Query Source")> _
    Public Sub QueryWithContains_Like_NonConstantValue()
      Dim query = From c In DB.Customers _
                  Where c.City.Contains(DB.Customers.OrderBy(Function(x) x.City).Select(Function(y) y.City.Substring(1, 2)).First()) _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-103: Is/IsNot Operator for non primitive Types")> _
    Public Sub QueryWithWhere_OuterObject()
      Dim customer As Customer = DB.Customers.First()

      Dim query = From c In DB.Customers _
                  Where c Is customer _
                  Select c

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyOnly()
      Dim query = From p In DB.Products _
                  Where p.Discontinued _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanProperty_ExplicitComparison()
      Dim query = From p In DB.Products _
                  Where p.Discontinued = True _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyOnly_Negate()
      Dim query = From p In DB.Products _
                  Where Not p.Discontinued _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyAndAnother()
      Dim discontinuedProductID As Integer = 5

      Dim query = From p In DB.Products _
                  Where p.ProductID = discontinuedProductID AndAlso p.Discontinued _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyAndAnother_Negate()
      Dim notDiscontinuedProductID As Integer = 4

      Dim query = From p In DB.Products _
                  Where p.ProductID = notDiscontinuedProductID AndAlso Not p.Discontinued _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyAndAnother_ExplicitComparison_True()
      Dim discontinuedProductID As Integer = 5

      Dim query = From p In DB.Products _
                  Where p.ProductID = discontinuedProductID AndAlso p.Discontinued = True _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhere_BooleanPropertyAndAnother_ExplicitComparison_False()
      Dim notDiscontinuedProductID As Integer = 4

      Dim query = From p In DB.Products _
                  Where p.ProductID = notDiscontinuedProductID AndAlso p.Discontinued = False _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithVirtualKeySide_EqualsNull()
      Dim query = From e In DB.Employees _
                  Where e.ReportsToEmployee Is Nothing _
                  Select e

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithVirtualKeySide_NotEqualsNull()
      Dim query = From e In DB.Employees _
                  Where e.ReportsToEmployee IsNot Nothing _
                  Select e

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-103: Is/IsNot Operator for non primitive Types")> _
    Public Sub QueryWithVirtualKeySide_EqualsOuterObject()
      Dim product As Product = DB.Products.First()

      Dim query = From od In DB.OrderDetails _
                  Where od.Product Is product _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-103: Is/IsNot Operator for non primitive Types")> _
    Public Sub QueryWithVirtualKeySide_NotEqualsOuterObject()
      Dim product As Product = DB.Products.First()

      Dim query = From od In DB.OrderDetails _
                  Where od.Product IsNot product _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithIDInCondition()
      Dim product As Product = DB.Products.First()

      Dim query = From od In DB.OrderDetails _
                  Where od.ProductID = product.ProductID _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithWhereOnForeignKey_RealSide()
      Dim orderID As Integer = DB.Orders.[Select](Function(x) x.OrderID).First()

      Dim query = From od In DB.OrderDetails _
                  Where od.Order.OrderID = orderID _
                  Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithConditionTrueInWherePart()
      Dim firstOrder = DB.Orders.First()

      Dim query = From o In DB.Orders _
                  Where o.OrderID = (If(True, firstOrder.OrderID, o.OrderID)) _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithConditionFalseInWherePart()
      Dim query = From o1 In DB.Orders _
                  Where o1.OrderID = (If(False, 1, o1.OrderID)) _
                  Select o1

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithEqualConditionInWherePart()
      Dim query = From o2 In DB.Orders _
                  Where o2.OrderID = (If(o2.OrderID = 1, 2, 3)) _
                  Select o2

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_Is()
      Dim query = DB.Contacts.Where(Function(c) TypeOf c Is CustomerContact)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace