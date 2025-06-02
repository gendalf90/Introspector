namespace Introspector.Integration.Tests;

/// <case name="use case one">info about case one</case>
/// <case name="ServiceOne">info about case of service one</case>
/// <component name="service one" type="unknown">info about service one</component>
/// <component name="ServiceOne"/>
public class ServiceOne
{
    /// <call>
    ///     <case name="use case one" order="3.0"/>
    ///     <case name="ServiceOne" order="3.0"/>
    ///     <from name="ServiceThree"/>
    ///     <to name="service one"/>
    ///     <to name="ServiceOne"/>
    ///     <text>result of the call</text>
    /// </call>
    /// <call>
    ///     <case name="use case one" order="1.0"/>
    ///     <case name="ServiceOne" order="1.0"/>
    ///     <from name="service one"/>
    ///     <from name="ServiceOne"/>
    ///     <to name="ServiceThree"/>
    ///     <text>call service three</text>
    /// </call>
    public void CallThree()
    {
    }
}

public class ServiceTwo
{
    /// <call>
    ///     <case name="use case two" order="3.0"/>
    ///     <to name="service two"/>
    ///     <from name="ServiceThree" />
    ///     <text>result of the call</text>
    /// </call>
    /// <call>
    ///     <case name="use case two" order="1.0"/>
    ///     <from name="service two"/>
    ///     <to name="ServiceThree" />
    ///     <text>
    ///         call
    ///         service
    ///         three
    ///     </text>
    /// </call>
    /// <comment>
    ///     <case name="use case two" order="1.1"/>
    ///     <over name="ServiceThree"/>
    ///     <text>processing request to service three</text>
    /// </comment>
    public void CallThree()
    {
    }
}

/// <case name="use case one">info about case one</case>
/// <component name="ServiceThree">
/// info about ServiceThree
/// </component>
public class ServiceThree
{
    /// <component name="database" type="database"/>
    /// <component name="not called service"/>
    /// <call>
    ///     <case name="use case one" order="2.0"/>
    ///     <case name="ServiceOne" order="2.0"/>
    ///     <case name="use case two" order="2.0"/>
    ///     <case name="use case three" order="1"/>
    ///     <from name="ServiceThree"/>
    ///     <to name="database" />
    ///     <text>call to database</text>
    /// </call>
    /// <call>
    ///     <case name="use case one" order="2.2"/>
    ///     <case name="ServiceOne" order="2.2"/>
    ///     <case name="use case two" order="2.2"/>
    ///     <case name="use case three" order="3"/>
    ///     <case name="use case three" order="3.1"/>
    ///     <from name="database"/>
    ///     <to name="ServiceThree"/>
    ///     <text>result from database</text>
    /// </call>
    /// <comment>
    ///     <case name="use case one" order="2.1"/>
    ///     <case name="ServiceOne" order="2.1"/>
    ///     <case name="use case two" order="2.1"/>
    ///     <case name="use case three" order="2"/>
    ///     <over name="database"/>
    ///     <text>
    ///         processing request to database
    ///     </text>
    /// </comment>
    public void CallDatabase()
    {
    }
}