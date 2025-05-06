namespace Introspector.Sample
{
  /// <component name="service 3" type="participant" scale="2.0"/>
  public class ServiceThree
  {
    /// <call>
    ///     <case cref="ServiceOne" order="2.0"/>
    ///     <case cref="ServiceTwo" order="2.0"/>
    ///     <from cref="ServiceThree"/>
    ///     <to name="database" />
    ///     <text>call to database</text>
    /// </call>
    /// <call>
    ///     <case cref="ServiceOne" order="2.2"/>
    ///     <case cref="ServiceTwo" order="2.2"/>
    ///     <from name="database"/>
    ///     <to cref="ServiceThree"/>
    ///     <text>result from database</text>
    /// </call>
    /// <comment>
    ///     <case cref="ServiceOne" order="2.1"/>
    ///     <case cref="ServiceTwo" order="2.1"/>
    ///     <over name="database"/>
    ///     <text>processing request to database</text>
    /// </comment>
    /// <component name="database" type="database" scale="3.0"/>
    public int ReturnThree()
    {
      return 3;
    }
  }
}