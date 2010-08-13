using System;
using System.Data.Linq.Mapping;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// provides functionality to get all MetaDataMembers of a certain type
  /// </summary>
  public interface IReverseMappingResolver
  {
    MetaDataMember[] GetMetaDataMembers (Type entityType);
  }
}
