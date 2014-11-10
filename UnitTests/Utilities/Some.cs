using System;

namespace Remotion.Linq.SqlBackend.UnitTests.Utilities
{
  /// <summary>
  /// Provides randomly chosen items for use in tests.
  /// </summary>
  public static class Some
  {
    private static readonly Random s_random = new Random();

    public static int Int32(int exclusiveUpperBound = int.MaxValue)
    {
      return s_random.Next (exclusiveUpperBound);
    }

    public static T Item<T>(params T[] items)
    {
      var index = Int32 (items.Length);
      return items[index];
    }
  }
}