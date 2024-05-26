namespace Introspector.Sample
{
    public class ServiceTwo
    {
        /*
        is: message
        of: use case 2
        from: service two
        to: service three
        text: call service three
        order: 2
        */
        public string GetResult()
        {
            var serviceThree = new ServiceThree();

            /*
            is: message
            of: use case 2
            from: service three
            to: service two
            text: get the result of the call
            order: 3
            */
            return serviceThree.ReturnThree().ToString();
        }
    }
}