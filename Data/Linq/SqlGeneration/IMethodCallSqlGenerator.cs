/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.Linq.SqlGeneration
{
  /// <summary>
  /// This interface has to be implemented, when a new MethodCallGenerator is generated. This generator has to handle method calls which are not
  /// supported as default by the framework. This generator has to be registered to <see cref="MethodCallSqlGeneratorRegistry"/>.
  /// </summary>
  public interface IMethodCallSqlGenerator
  {
    /// <summary>
    /// The method has to contain the logic for generating sql code for the method call. 
    /// </summary>
    /// <param name="methodCall"><see cref="MethodCall"/></param>
    /// <param name="commandBuilder"><see cref="ICommandBuilder"/></param>
    void GenerateSql(MethodCall methodCall, ICommandBuilder commandBuilder);
  }
}