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

Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind

Namespace LinqSamples101
  Public Class GroupInheritance
    Inherits Executor
    ' <Category("Inheritance")> _
    '<Title("Simple")> _
    '<Description("This sample returns all contacts where the city is London.")> _
    Public Sub LinqToSqlInheritance01()

      Dim cons = From contact In db.Contacts _
            Select contact

      For Each con As Contact In cons
        serializer.Serialize ("Company name: " & con.CompanyName)
        serializer.Serialize ("Phone: " & con.Phone)
        serializer.Serialize ("This is a " & con.GetType().ToString)
        serializer.Serialize (Environment.NewLine)
      Next

    End Sub

    '<Category("Inheritance")> _
    '<Title("TypeOf")> _
    '<Description("This sample uses OfType to return all customer contacts.")> _
    Public Sub LinqToSqlInheritance02()

      Dim cons = From contact In db.Contacts.OfType (Of CustomerContact)() _
            Select contact

      serializer.Serialize (cons)
    End Sub

    '<Category("Inheritance")> _
    '<Title("IS")> _
    '<Description("This sample uses IS to return all shipper contacts.")> _
    Public Sub LinqToSqlInheritance03()

      Dim cons = From contact In db.Contacts _
            Where TypeOf contact Is ShipperContact _
            Select contact

      serializer.Serialize (cons)
    End Sub


    '<Category("Inheritance")> _
    '<Title("CType")> _
    '<Description("This sample uses CType to return FullContact or Nothing.")> _
    Public Sub LinqToSqlInheritance04()
      Dim cons = From contact In db.Contacts _
            Select CType (contact, FullContact)

      serializer.Serialize (cons)
    End Sub

    '<Category("Inheritance")> _
    '<Title("Cast")> _
    '<Description("This sample uses a cast to retrieve customer contacts who live in London.")> _
    Public Sub LinqToSqlInheritance05()
      Dim cons = From contact In db.Contacts _
            Where contact.ContactType = "Customer" _
                  AndAlso (DirectCast (contact, CustomerContact)).City = "London"

      serializer.Serialize (cons)
    End Sub
  End Class
End Namespace
