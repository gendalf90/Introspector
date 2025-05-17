namespace Introspector.Sample
{
    /// <case>info about case of service two</case>
    /// <component name="service 2" type="participant"/>
    public class ServiceTwo
    {
        /// <call>
        ///     <case cref="ServiceTwo" order="1.0"/>
        ///     <from cref="ServiceTwo"/>
        ///     <to cref="ServiceThree" />
        ///     <text>call service three</text>
        /// </call>
        /// <call>
        ///     <case cref="ServiceTwo" order="3"/>
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