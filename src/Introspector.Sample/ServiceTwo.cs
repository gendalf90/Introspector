namespace Introspector.Sample
{
    /// <case name="use case 2">info about case 1</case>
    /// <component name="service 2" type="participant" scale="1.0"/>
    public class ServiceTwo
    {
        /// <call>
        ///     <case cref="ServiceTwo" order="1.0"/>
        ///     <from cref="ServiceTwo"/>
        ///     <to cref="ServiceThree" />
        ///     <text>call service three</text>
        /// </call>
        /// <call>
        ///     <case cref="ServiceTwo" order="1.1"/>
        ///     <from cref="ServiceThree"/>
        ///     <to cref="ServiceTwo"/>
        ///     <text>result of the call</text>
        /// </call>
        public string GetResult()
        {
            var serviceThree = new ServiceThree();

            return serviceThree.ReturnThree().ToString();
        }
    }
}