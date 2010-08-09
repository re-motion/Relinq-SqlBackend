using System;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public interface IReverseMappingResolver
  {
    MetaDataMember[] GetMetaDataMembers (Type entityType);
  }
}
