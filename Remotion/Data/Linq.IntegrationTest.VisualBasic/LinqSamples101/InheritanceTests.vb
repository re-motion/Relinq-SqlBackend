' This file is part of the re-motion Core Framework (www.re-motion.org)
' Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
' 
' The re-motion Core Framework is free software; you can redistribute it 
' and/or modify it under the terms of the GNU Lesser General Public License 
' as published by the Free Software Foundation; either version 2.1 of the 
' License, or (at your option) any later version.
' 
' re-motion is distributed in the hope that it will be useful, 
' but WITHOUT ANY WARRANTY; without even the implied warranty of 
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
' GNU Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public License
' along with re-motion; if not, see http://www.gnu.org/licenses.
' 
Option Infer On
Option Strict On

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind

Namespace LinqSamples101
  Public Class InheritanceTests
    Inherits TestBase

    'This sample returns all contacts where the city is London.
    Public Sub LinqToSqlInheritance01()

      Dim cons = From contact In db.Contacts _
            Select contact

      TestExecutor.Execute(cons, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses OfType to return all customer contacts.")> _
    Public Sub LinqToSqlInheritance02()

      Dim cons = From contact In DB.Contacts.OfType(Of CustomerContact)() _
            Select contact

      TestExecutor.Execute(cons, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses IS to return all shipper contacts.")> _
    Public Sub LinqToSqlInheritance03()

      Dim cons = From contact In DB.Contacts _
            Where TypeOf contact Is ShipperContact _
            Select contact

      TestExecutor.Execute(cons, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses CType to return FullContact or Nothing.")> _
    Public Sub LinqToSqlInheritance04()
      Dim cons = From contact In DB.Contacts _
            Select CType(contact, FullContact)

      TestExecutor.Execute(cons, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a cast to retrieve customer contacts who live in London.")> _
    Public Sub LinqToSqlInheritance05()
      Dim cons = From contact In DB.Contacts _
            Where contact.ContactType = "Customer" _
                  AndAlso (DirectCast(contact, CustomerContact)).City = "London"

      TestExecutor.Execute(cons, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
