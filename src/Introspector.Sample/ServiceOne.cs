namespace Introspector.Sample
{
    /// <case name="Use Case 1">info about case 1</case>
    /// <component name="Service 1" type="participant">info about service 1</component>
    public class ServiceOne
    {
        /// <call>
        ///     <case name="Use Case 1" order="3.0"/>
        ///     <from name="Service 3"/>
        ///     <to name="Service 1"/>
        ///     <text>result of the call</text>
        /// </call>
        /// <call>
        ///     <case name="Use Case 1" order="1.0"/>
        ///     <from name="Service 1"/>
        ///     <to name="Service 3"/>
        ///     <text>call service three</text>
        /// </call>
        /// <comment>
        ///     <case name="Use Case 1" order="1.1"/>
        ///     <over name="Service 3"/>
        ///     <text>processing request from service one</text>
        /// </comment>
        public int GetResult()
        {
            var serviceThree = new ServiceThree();

            return serviceThree.ReturnThree();
        }
    }
}