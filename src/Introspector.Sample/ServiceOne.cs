namespace Introspector.Sample
{
    /// <case name="Use Case 1">info about case 1</case>
    /// <component name="Service 1" type="participant">info about service 1</component>
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
        /// <comment>
        ///     <case cref="ServiceOne" order="1.1"/>
        ///     <over cref="ServiceThree"/>
        ///     <text>processing request from service one</text>
        /// </comment>
        public int GetResult()
        {
            var serviceThree = new ServiceThree();

            return serviceThree.ReturnThree();
        }
    }
}