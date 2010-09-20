// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="LikeEscapeUtility"/> is an utility class to escape strings as required by the LIKE SQL function.
  /// </summary>
  public static class LikeEscapeUtility // TODO Review 3090: Move to SqlLikeExpression and remove this class.
  {
    public static string Escape (string text, string escapeSequence)
    {
      ArgumentUtility.CheckNotNull ("text", text);
      ArgumentUtility.CheckNotNull ("escapeSequence", escapeSequence);

      var escapedString = new StringBuilder (text);
      escapedString.Replace (escapeSequence, escapeSequence + escapeSequence);
      escapedString.Replace ("%", string.Format (@"{0}%", escapeSequence));
      escapedString.Replace ("_", string.Format (@"{0}_", escapeSequence));
      escapedString.Replace ("[", string.Format (@"{0}[", escapeSequence));
      return escapedString.ToString ();
    }

    public static Expression Escape (Expression expression, string escapeSequence)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("escapeSequence", escapeSequence);

      SqlFunctionExpression result = Escape (expression, escapeSequence, escapeSequence);
      result = Escape (result, "%", escapeSequence);
      result = Escape (result, "_", escapeSequence);
      result = Escape (result, "[", escapeSequence);
      
      return result;
    }

    private static SqlFunctionExpression Escape (Expression expression, string replacedSequence, string escapeSequence)
    {
      return new SqlFunctionExpression (
          typeof (string),
          "REPLACE",
          expression,
          new SqlLiteralExpression (replacedSequence),
          new SqlLiteralExpression (escapeSequence + replacedSequence));
    }
  }
}