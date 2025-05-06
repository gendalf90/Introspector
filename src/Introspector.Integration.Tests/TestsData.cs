namespace Introspector.Integration.Tests;

/// <case name="use case one">info about case one</case>
/// <component name="service one" type="unknown" scale="1.0"/>
public class ServiceOne
{
    /// <call>
    ///     <case cref="ServiceOne" order="3.0"/>
    ///     <from cref="ServiceThree"/>
    ///     <to cref="ServiceOne"/>
    ///     <text>result of the call</text>
    /// </call>
    /// <call>
    ///     <case cref="ServiceOne" order="1.0"/>
    ///     <from cref="ServiceOne"/>
    ///     <to cref="ServiceThree"/>
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
    ///     <from name="service three" />
    ///     <text>result of the call</text>
    /// </call>
    /// <call>
    ///     <case name="use case two" order="1.0"/>
    ///     <from name="service two"/>
    ///     <to name="service three" />
    ///     <text>call service three</text>
    /// </call>
    public void CallThree()
    {
    }
}

/// <case name="use case one">info about case one</case>
/// <component name="service three" type="participant" scale="2.0"/>
public class ServiceThree
{
    /// <component name="database" type="database" scale="3.0"/>
    /// <component name="not called service"/>
    /// <call>
    ///     <case cref="ServiceOne" order="2.0"/>
    ///     <case name="use case two" order="2.0"/>
    ///     <from cref="ServiceThree"/>
    ///     <to name="database" />
    ///     <text>call to database</text>
    /// </call>
    /// <call>
    ///     <case cref="ServiceOne" order="2.2"/>
    ///     <case name="use case two" order="2.2"/>
    ///     <from name="database"/>
    ///     <to cref="ServiceThree"/>
    ///     <text>result from database</text>
    /// </call>
    /// <comment>
    ///     <case cref="ServiceOne" order="2.1"/>
    ///     <case name="use case two" order="2.1"/>
    ///     <over name="database"/>
    ///     <text>processing request to database</text>
    /// </comment>
    public void CallDatabase()
    {
    }
}