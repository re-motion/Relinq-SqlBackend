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
  Public Class CustomProjectionTests
    Inherits TestBase

    <Test> _
    Public Sub SequenceOfEntityProperties()
      Dim query = From od In DB.OrderDetails _
            Where od.Quantity <= 55 _
            Select od.Quantity

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SequenceOfPrimaryKeys()
      Dim query = (From od In DB.OrderDetails Where od.Quantity <= 55 Select od.OrderID)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SequenceOfForeignKeyIDs()
      Dim query = (From od In DB.OrderDetails Where od.Quantity = 10 Select od.Order.OrderID)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-99: Support .Count on Collections")>
    Public Sub ComplexProjection()
      Dim query = (From o In DB.Orders Where o.OrderID = 10248 _
            Select New With { _
            o.OrderID, _
            o.OrderDate, _
            Key .[Property] = { _
                                o.Customer.ContactName,
                                o.OrderDetails.Count
                              } _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-99: Support .Count on Collections")>
    Public Sub ComplexProjection_WithSingleQuery()
      Dim query = (From o In DB.Orders Where o.OrderID = 10248 _
            Select New With { _
            o.OrderID, _
            o.OrderDate, _
            Key .[Property] = { _
                                o.Customer.ContactName,
                                o.OrderDetails.Count
                              } _
            }).[Single]()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ComplexProjection_ContainingEntity()
      Dim query = (From o In DB.Orders _
            Select New With { _
            o.OrderID, _
            o _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SingleBoolean()
      Dim query = (From p In DB.Products _
            Where p.ProductID = 2 _
            Select p.Discontinued)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SingleNullableBoolean()
      Dim query = (From e In DB.Employees _
            Where e.EmployeeID = 2 _
            Select e.HasCar)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ComplexProjection_WithBooleans()
      Dim query = (From p In DB.Products _
            Where p.ProductID = 2 _
            Select New With { _
            p.Discontinued _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ComplexProjection_WithNullableBooleans()
      Dim query = (From p In DB.Employees _
            Where p.EmployeeID = 2 _
            Select New With { _
            p.HasCar _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SingleProperty()
      Dim query = (From p In DB.Products _
            Where p.ProductID = 2 _
            Select p.ProductName)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SingleValueType()
      Dim query = From e In DB.Employees _
            Where e.EmployeeID = 1 _
            Select e.EmployeeID

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SingleValueType_Nullable()
      Dim query = From e In DB.Employees _
            Where e.ReportsTo IsNot Nothing _
            Select e.ReportsTo

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ComplexProjection_WithNullableValueTypes()
      Dim query = (From e In DB.Employees _
            Where e.ReportsTo IsNot Nothing _
            Select New With { _
              e.ReportsTo _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ComplexProjection_WithValueTypes()
      Dim query = (From p In DB.Products _
            Where p.ProductID = 2 _
            Select New With { _
            p.ProductID _
            })

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace