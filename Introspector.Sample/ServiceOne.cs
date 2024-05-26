namespace Introspector.Sample
{
    /*
    is: participant
    name: service one
    type: boundary
    scale: 1.0
    */
    public class ServiceOne
    {
        /*
        is: message
        of: use case 1
        from: service one
        to: service three
        text: call service three
        order: 1
        */
        public int GetResult()
        {
            var serviceThree = new ServiceThree();

            /*
            is: message
            of: use case 1
            from: service three
            to: service one
            text: get the result of the call
            order: 2
            */
            return serviceThree.ReturnThree();
        }
    }
}