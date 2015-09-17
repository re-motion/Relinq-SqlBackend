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
  Public Class ExplicitJoinsTests
    Inherits TestBase

    <Test> _
    Public Sub ExplicitJoin()
      Dim query = From o In DB.Orders _
            Join c In DB.Customers On o.Customer Equals c _
            Where o.CustomerID = "QUEEN" _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_Once()
      Dim query = From o In DB.Orders _
            Group Join od In DB.OrderDetails On o Equals od.Order Into odo = Group _
            From ode In odo _
            Select ode

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_Twice()
      Dim query = From o In DB.Orders _
            Group Join od In DB.OrderDetails On o Equals od.Order Into god = Group _
            Group Join c In DB.Customers On o.Customer Equals c Into goc = Group _
            From odo In god From oc In goc _
            Select odo

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_InSubstatement_Once()
      Dim query = From o In DB.Orders _
            Where _
            o.OrderID = (From so In DB.Orders Group Join si In DB.OrderDetails On so Equals si.Order Into goi = Group _
              From oi In goi Select oi.Order.OrderID).First() _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-104: Support conversion operator for Query Source")> _
    Public Sub ExplicitJoinWithInto_InSubstatement_Twice()
      Dim query = From o In DB.Orders _
            Where o.OrderID = (From so In DB.Orders _
                    Group Join si In DB.OrderDetails On so Equals si.Order Into goi = Group _
                    Group Join si In DB.Customers On so.Customer Equals si Into goc = Group _
                    From oi In goi _
                    From oc In goc Select oi.Order.OrderID).OrderBy(Function(x) x).First() _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_InTwoSubStatements()
      Dim query = From o In DB.Orders _
            Where o.OrderID = (From so In DB.Orders _
                    Group Join si In DB.OrderDetails On so Equals si.Order Into goi = Group _
                    From oi In goi Select oi.Order.OrderID).First() _
                  AndAlso o.Customer.ContactName =
                          (From so In DB.Orders _
                            Group Join sc In DB.Customers On so.Customer Equals sc Into goc = Group _
                            From oc In goc Select oc.ContactName).First() _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_InSameStatementAndInSubStatement()
      Dim query = From o In DB.Orders _
            Group Join d In DB.OrderDetails On o Equals d.Order Into god = Group _
            From od In god Where o.ShipCity = (From so In DB.Orders _
                                   Group Join sd In DB.OrderDetails On so Equals sd.Order Into gda = Group _
                                   From da In gda Select so.ShipCity).First() _
            Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub ExplicitJoinWithInto_WithOrderBy()
      Dim query = From o In DB.Orders _
            Group Join d In DB.OrderDetails.OrderBy(Function(od) od.Quantity) On o Equals d.Order Into god = Group _
            From od In god _
            Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace