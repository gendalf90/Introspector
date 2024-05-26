namespace Introspector.Sample
{
    public static class CustomCommentSource
    {
        public static IEnumerable<string> GetList()
        {
            yield return @"
            is: participant
            type: database
            name: database
            scale: 3.0";
        }
    }
}