namespace Introspector.Sample
{
    /*
    is: participant
    name: service three
    scale: 2.0
    */
    public class ServiceThree
    {
        /*
        is: message
        of-list:
        - of: use case 1
          order: 1.1
        - of: use case 2
          order: 2.1
        from: service three
        to: database
        text: call database
        */
        public int ReturnThree()
        {
            /*
            is: message
            of-list:
            - of: use case 1
            - of: use case 2
              order: 2.2
            over: database
            text: calculate some result
            order: 1.2
            */
            return 3;
            /*
            is: message
            of-list:
            - of: use case 1
              order: 1.3
            - of: use case 2
            from: database
            to: service three
            text: return value 3
            order: 2.3
            */
        }
    }
}