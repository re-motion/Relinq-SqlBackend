namespace Remotion.Linq.SqlBackend.UnitTests.TestDomain
{
  public class Server : Cook
  {
    public double WalkingSpeed { get; set; }

    public override string GetFullName ()
    {
      return base.GetFullName () + " (SRV)";
    }
  }
}