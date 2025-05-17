namespace Introspector.Sample
{
  /// <component>
  /// info about 
  /// service three
  /// </component>
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
    /// <component name="database" type="database"/>
    public int ReturnThree()
    {
      return 3;
    }
  }
}