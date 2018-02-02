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
  Public Class SelectTests
    Inherits TestBase
    Private Class ProductData
      Public Property ProductID() As Integer
        Get
          Return m_ProductID
        End Get
        Private Set(value As Integer)
          m_ProductID = value
        End Set
      End Property
      Private m_ProductID As Integer

      Public Property Name() As String
        Get
          Return m_Name
        End Get
        Set(value As String)
          m_Name = value
        End Set
      End Property
      Private m_Name As String

      Public Property Discontinued() As Boolean
        Get
          Return m_Discontinued
        End Get
        Set(value As Boolean)
          m_Discontinued = value
        End Set
      End Property
      Private m_Discontinued As Boolean

      Public Property HasUnitsInStock() As Boolean
        Get
          Return m_HasUnitsInStock
        End Get
        Set(value As Boolean)
          m_HasUnitsInStock = value
        End Set
      End Property
      Private m_HasUnitsInStock As Boolean

      Public Sub New(productId1 As Integer)
        ProductID = productId1
      End Sub
    End Class

    <Test> _
    <Ignore("RM-3306: Support for MemberInitExpressions")> _
    Public Sub WithMemberInitExpression_InOuterMostLevel()
      Dim query = DB.Products.Select(Function(p) New ProductData(p.ProductID) With { _
        .Name = p.ProductName, _
        .Discontinued = p.Discontinued, _
        .HasUnitsInStock = p.UnitsInStock > 0 _
      })
      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub SimpleQuery_WithRelatedEntity()
      Dim query = From od In DB.OrderDetails _
                  Select od.Order

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub MethodCallOnCoalesceExpression()
      Dim query = From o In DB.Orders _
                  Where (If(o.ShipRegion, o.Customer.Region)).ToUpper() = "ISLE OF WIGHT" _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub MethodCallOnConditionalExpression()
      Dim query = From o In DB.Orders _
                  Where (If(o.ShipRegion = "Isle of Wight", o.ShipRegion, o.Customer.Region)).ToUpper() = "ISLE OF WIGHT" _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub LogicalMemberAccessOnCoalesceExpression()
      Dim query = From o In DB.Orders _
                  Where (If(o.ShipRegion, o.Customer.Region)).Length = 2 _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub LogicalMemberAccessOnConditionalExpression()
      Dim query = From o In DB.Orders _
                  Where (If(o.ShipRegion = "Isle of Wight", o.ShipRegion, o.Customer.Region)).Length = 13 _
                  Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-93: When the Coalesce operator is used with relations" + "(.i.e. their ID and ForeignKey-columns), nullable<valueType> gets compared with valueType, resulting in an ArgumentException")> _
    Public Sub CoalesceExpression_ColumnMember()
      Dim query = From e In DB.Employees _
                  Where (If(e.ReportsToEmployee, e)).EmployeeID = 1 _
                  Select e

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithConstant()
      Dim query = (From o In DB.Orders _
                   Where o.OrderID = 10248 _
                   Select 1).Single()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Query_WithObjectID()
      Dim query = (From o In DB.Orders _
                   Where o.OrderID = 10248 _
                   Select o.OrderID).Single()

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace