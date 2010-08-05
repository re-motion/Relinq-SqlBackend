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


Namespace LinqSamples101
  Public Class GroupObjectIdentity
    Inherits Executor

    '  <Category("Object Identity")> _
    '<Title("Object Caching - 1")> _
    '<Description("This sample demonstrates how, upon executing the same query twice, " & _
    '             "you will receive a reference to the same object in memory each time.")> _
    Public Sub LinqToSqlObjectIdentity01()
      Dim cust1 = db.Customers.First(Function(cust) cust.CustomerID = "BONAP")
      Dim cust2 = (From cust In db.Customers _
            Where cust.CustomerID = "BONAP").First()

      serializer.Serialize("cust1 and cust2 refer to the same object in memory: " & _
                            Object.ReferenceEquals(cust1, cust2))
    End Sub

    '<Category("Object Identity")> _
    '<Title("Object Caching - 2")> _
    '<Description("This sample demonstrates how, upon executing different queries that " & _
    '             "return the same row from the database, you will receive a " & _
    '             "reference to the same object in memory each time.")> _
    Public Sub LinqToSqlObjectIdentity02()
      Dim cust1 = db.Customers.First(Function(cust) cust.CustomerID = "BONAP")
      Dim cust2 = (From ord In db.Orders _
            Where ord.Customer.CustomerID = "BONAP").First().Customer

      serializer.Serialize("cust1 and cust2 refer to the same object in memory: " & _
                            Object.ReferenceEquals(cust1, cust2))
    End Sub
  End Class
End Namespace
