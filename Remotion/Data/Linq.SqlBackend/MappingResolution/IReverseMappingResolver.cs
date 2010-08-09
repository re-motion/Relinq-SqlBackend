using System;
using System.Reflection;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public interface IReverseMappingResolver
  {
    PropertyInfo[] GetEntityMembers (Type entityType);
  }
}
