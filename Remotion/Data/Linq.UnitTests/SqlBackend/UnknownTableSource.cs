using System;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Data.Linq.UnitTests.SqlBackend
{
  class UnknownTableSource : AbstractTableSource
  {

    public override Type ItemType
    {
      get { return typeof (string); }
    }

    public override AbstractTableSource Accept (ITableSourceVisitor visitor)
    {
      throw new NotImplementedException ();
    }

    public override SqlTableSource GetResolvedTableSource ()
    {
      throw new NotImplementedException();
    }
  }
}