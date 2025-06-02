namespace Introspector.Sample
{
  /// <component name="Service 3">
  /// info about 
  /// service three
  /// </component>
  public class ServiceThree
  {
    /// <call>
    ///     <case name="Use Case 1" order="2.0"/>
    ///     <case name="Use Case 2" order="2.0"/>
    ///     <from name="Service 3"/>
    ///     <to name="database" />
    ///     <text>call to database</text>
    /// </call>
    /// <call>
    ///     <case name="Use Case 1" order="2.2"/>
    ///     <case name="Use Case 2" order="2.2"/>
    ///     <from name="database"/>
    ///     <to name="Service 3"/>
    ///     <text>result from database</text>
    /// </call>
    /// <comment>
    ///     <case name="Use Case 1" order="2.1"/>
    ///     <case name="Use Case 2" order="2.1"/>
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