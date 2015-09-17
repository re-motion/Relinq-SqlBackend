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
  Public Class SetOperationsTests
    Inherits TestBase
    <Test> _
    Public Sub Union_TopLevel()
      Dim query = DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) c.ContactID) _
                  .Union(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID)) _
                  .Union(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-104: Support conversion operator for Query Source")> _
    Public Sub Union_SubQuery()
      Dim query = From p In DB.Products _
                  Where p.SupplierID IsNot Nothing AndAlso (DB.Suppliers.Where(Function(s) s.Country = "UK").Select(Function(s) s.SupplierID) _
                                                            .Union(DB.Suppliers.Where(Function(s) s.Country = "USA") _
                                                            .Select(Function(s) s.SupplierID))).Contains(CInt(p.SupplierID)) _
                  Order By p.ProductID _
                  Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Union_WithDiscardedOrderBy()
      Dim query = DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) c.ContactID).OrderByDescending(Function(c) c) _
                  .Union(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID).OrderByDescending(Function(c) c))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Union_WithNonDiscardedOrderBy()
      Dim query = DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) c.ContactID) _
                  .Union(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID)).OrderByDescending(Function(c) c)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Union_WithNonDiscardedOrderByWithTake()
      Dim query = DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) c.ContactID) _
                  .OrderByDescending(Function(c) c).Take(3).Union(DB.Contacts.OfType(Of ShipperContact)() _
                                                                  .Select(Function(c) c.ContactID).OrderByDescending(Function(c) c)).Take(3)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Union_WithSelectedDiscriminator()
      Dim query = DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) New With { _
        c.ContactID, _
        Key .Key = "Customer" _
      }).Union(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) New With { _
        c.ContactID, _
        Key .Key = "Shipper" _
      }))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Concat_TopLevel()
      Dim query = DB.Contacts.OfType(Of CustomerContact)() _
                  .Select(Function(c) c.ContactID).Concat(DB.Contacts.OfType(Of ShipperContact)() _
                                                          .Select(Function(c) c.ContactID)) _
                                                        .Concat(DB.Contacts.OfType(Of ShipperContact)() _
                                                                .Select(Function(c) c.ContactID))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    <Ignore("RMLNQSQL-104: Support conversion operator for Query Source")> _
    Public Sub Concat_SubQuery()
      Dim query = _
          From p In DB.Products _
          Where p.SupplierID IsNot Nothing AndAlso ( _
              DB.Suppliers.Where(Function(s) s.Country = "UK").Select(Function(s) s.SupplierID) _
              .Concat( _
                  DB.Suppliers.Where(Function(s) s.Country = "USA").Select(Function(s) s.SupplierID) _
              ) _
          ).Contains(CInt(p.SupplierID)) _
          Order By p.ProductID _
          Select p

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Concat_WithDiscardedOrderBy()
      Dim query = _
          DB.Contacts.OfType(Of CustomerContact)().Select(Function(c) c.ContactID).OrderByDescending(Function(c) c) _
          .Concat(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID).OrderByDescending(Function(c) c))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Concat_WithNonDiscardedOrderBy()
      Dim query = DB.Contacts.OfType(Of CustomerContact)() _
                  .Select(Function(c) c.ContactID).Concat(DB.Contacts.OfType(Of ShipperContact)() _
                                                          .Select(Function(c) c.ContactID)).OrderByDescending(Function(c) c)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Concat_WithNonDiscardedOrderByWithTake()
      Dim query = DB.Contacts.OfType(Of CustomerContact)() _
                  .Select(Function(c) c.ContactID).OrderByDescending(Function(c) c).Take(3) _
                  .Concat(DB.Contacts.OfType(Of ShipperContact)().Select(Function(c) c.ContactID).OrderByDescending(Function(c) c)).Take(3)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    <Test> _
    Public Sub Concat_WithSelectedDiscriminator()
      Dim query = _
          DB.Contacts.OfType(Of CustomerContact)() _
          .[Select](Function(c) New With { _
            c.ContactID, _
            Key .Key = "Customer" _
          }).Concat(DB.Contacts.OfType(Of ShipperContact)() _
          .[Select](Function(c) New With { _
            c.ContactID, _
            Key .Key = "Shipper" _
          }))

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace