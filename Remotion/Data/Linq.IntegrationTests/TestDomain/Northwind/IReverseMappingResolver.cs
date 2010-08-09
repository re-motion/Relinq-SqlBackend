using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  public interface IReverseMappingResolver
  {
    PropertyInfo[] GetEntityMembers (Type entityType);
  }
}
