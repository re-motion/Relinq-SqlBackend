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
  Public Class GroupCompiledQuery
    Inherits Executor
    '<Category("Compiled Query")> _
    ' <Title("Compiled Query - 1")> _
    ' <Description("This sample create a compiled query and then use it to retrieve customers of the input city")> _
    Public Sub LinqToSqlCompileQuery01()

      '' Create compiled query
      Dim fn = System.Data.Linq.CompiledQuery.Compile ( _
                                                       Function(db2 As Northwind, city As String) _
                                                        From c In db2.Customers _
                                                        Where c.City = city _
                                                        Select c)


      serializer.Serialize ("****** Call compiled query to retrieve customers from London ******")
      Dim LonCusts = fn (db, "London")
      serializer.Serialize (LonCusts)

      serializer.Serialize (Environment.NewLine)

      serializer.Serialize ("****** Call compiled query to retrieve customers from Seattle ******")
      Dim SeaCusts = fn (db, "Seattle")
      serializer.Serialize (SeaCusts)

    End Sub
  End Class
End Namespace
