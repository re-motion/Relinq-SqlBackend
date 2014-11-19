using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  // TODO RMLNQSQL-64: Test
  /// <summary>
  /// <see cref="UnresolvedCollectionJoinTableInfo"/> represents the data source defined by a member access to a collection in a LINQ expression.
  /// </summary>
  public class UnresolvedCollectionJoinTableInfo : ITableInfo
  {
     private readonly Expression _sourceExpression;
    private readonly MemberInfo _memberInfo;
    private readonly Type _itemType;

    public UnresolvedCollectionJoinTableInfo (Expression sourceExpression, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      _memberInfo = memberInfo;
      _sourceExpression = sourceExpression;

      var memberReturnType = ReflectionUtility.GetMemberReturnType (memberInfo);
      _itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (memberReturnType, "memberInfo");
    }

    public Expression SourceExpression
    {
      get { return _sourceExpression; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public Type ItemType
    {
      get { return _itemType; }
    }

    public ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      throw new NotImplementedException("TODO RMLNQSQL-64");
      // return visitor.VisitUnresolvedCollectionJoinTableInfo (this);
    }

    public IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This table info has not yet been resolved; call the resolution step first.");
    }

    public override string ToString ()
    {
      return string.Format ("{0}.{1}", MemberInfo.DeclaringType.Name, MemberInfo.Name);
    }
  }
}