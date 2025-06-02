namespace Introspector.Sample
{
    /// <case name="Use Case 2">info about case 2</case>
    /// <component name="service 2" type="participant"/>
    public class ServiceTwo
    {
        /// <call>
        ///     <case name="Use Case 2" order="1.0"/>
        ///     <from name="service 2"/>
        ///     <to name="Service 3" />
        ///     <text>call service three</text>
        /// </call>
        /// <call>
        ///     <case name="Use Case 2" order="3"/>
        ///     <from name="Service 3"/>
        ///     <to name="service 2"/>
        ///     <text>result of the call</text>
        /// </call>
        public string GetResult()
        {
            var serviceThree = new ServiceThree();

            return serviceThree.ReturnThree().ToString();
        }
    }
}