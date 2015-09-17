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

Imports System.Reflection
Imports NUnit.Framework

Namespace SystemTests
  <TestFixture>
  Public Class LetTests
    Inherits TestBase

    <Test> _
    Public Sub QueryWithLet_LetWithTable()
      Dim query = From o In DB.Orders _
            Let x = o _
            Select x

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithLet_LetWithColumn()
      Dim query = From o In DB.Orders _
            Let y = o.OrderID _
            Where y > 10248 AndAlso y < 10255 _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithLet_LetWithColumn2()
      Dim query = From o In DB.Orders _
            Let x = o.Customer.ContactName _
            Where x = "Liz Nixon" _
            Select o

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSeveralJoinsAndLet()
      Dim query = From od In DB.OrderDetails _
            Let x = od.Order.Customer _
            Where x.ContactName = "Liz Nixon" _
            Select x

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithSeveralLets()
      Dim query = From o In DB.Orders _
            Let x = o _
            Let y = o.Customer _
            Select x

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithLet_AndMultipleFromClauses()
      Dim query = From od In DB.OrderDetails _
            From o In DB.Orders _
            Let x = od.Order _
            Where od.Order.OrderID = 10248 _
            Where o Is od.Order _
            Select x

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub QueryWithMemberFromClause_WithLet()
      Dim query = From c In DB.OrderDetails _
            Let x = c.Order _
            From od In x.OrderDetails _
            Where c.Order.OrderID = 10248 _
            Select od

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace