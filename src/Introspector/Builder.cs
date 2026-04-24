namespace Introspector;

public static class Builder
{
    public static IEnumerable<Element> Build(Action<IBuilder> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        var builder = new BuilderImpl();

        configuration(builder);

        return builder.Build();
    }

    private record InputCase(string Key, string Description);

    private record InputComponent(string Key, string Description);

    private record InputCall(string CaseKey, string FromKey, string ToKey, string Text, float? Order);

    private record InputComment(string CaseKey, string OverKey, string Text, float? Order);

    private record InputRef(string CaseFromKey, string CaseToKey, float? Order);

    private record CaseRef(Case From, Case To, float? Order);

    private record OrderedElement(Element Element, float? Order);

    private record ComposedCaseElement(Case Case, IEnumerable<OrderedElement> Elements, float? Order);

    private class BuilderImpl : IBuilder
    {
        private readonly Dictionary<string, InputCase> inputCases = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, InputComponent> inputComponents = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<InputCall> inputCalls = [];
        private readonly List<InputComment> inputComments = [];
        private readonly List<InputRef> inputRefs = [];
        
        public void AddCall(string caseKey, string fromKey, string toKey, string text, float? order)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(caseKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(fromKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(toKey);

            inputCases.TryAdd(caseKey, new InputCase(caseKey, null));
            inputComponents.TryAdd(fromKey, new InputComponent(fromKey, null));
            inputComponents.TryAdd(toKey, new InputComponent(toKey, null));
            inputCalls.Add(new InputCall(caseKey, fromKey, toKey, text, order));
        }

        public void AddCase(string key, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            inputCases[key] = new InputCase(key, description);
        }

        public void AddRef(string caseFromKey, string caseToKey, float? order)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(caseFromKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(caseToKey);

            inputCases.TryAdd(caseFromKey, new InputCase(caseFromKey, null));
            inputCases.TryAdd(caseToKey, new InputCase(caseToKey, null));
            inputRefs.Add(new InputRef(caseFromKey, caseToKey, order));
        }

        public void AddComment(string caseKey, string text, float? order, string overKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(caseKey);

            inputCases.TryAdd(caseKey, new InputCase(caseKey, null));

            if (!string.IsNullOrWhiteSpace(overKey))
            {
                inputComponents.TryAdd(overKey, new InputComponent(overKey, null));
            }

            inputComments.Add(new InputComment(caseKey, overKey, text, order));
        }

        public void AddComponent(string key, string description)
        {
            ArgumentNullException.ThrowIfNull(key);

            inputComponents[key] = new InputComponent(key, description);
        }

        public IEnumerable<Element> Build()
        {
            var cases = BuildCases();
            var components = BuildComponents();
            var callsAndComments = BuildCallsAndComments(cases, components);
            var caseRefElements = BuildCaseRefs(callsAndComments, cases, components);

            return Enumerable.Empty<Element>()
                .Concat(cases.Values)
                .Concat(components.Values)
                .Concat(ExtractCallAndComments(callsAndComments.Concat(caseRefElements)))
                .ToList();
        }

        private IDictionary<string, Case> BuildCases()
        {
            return inputCases.Values.ToDictionary(x => x.Key, x => new Case(x.Key, x.Description), StringComparer.OrdinalIgnoreCase);
        }

        private IDictionary<string, Component> BuildComponents()
        {
            return inputComponents.Values.ToDictionary(x => x.Key, x => new Component(x.Key, x.Description), StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<ComposedCaseElement> BuildCallsAndComments(IDictionary<string, Case> cases, IDictionary<string, Component> components)
        {
            var result = new List<ComposedCaseElement>();

            foreach (var input in inputCalls)
            {
                var call = new Call
                (   
                    cases[input.CaseKey], 
                    components[input.FromKey],
                    components[input.ToKey],
                    input.Text,
                    Comparer.Null
                );

                result.Add(new ComposedCaseElement(call.Case, [new OrderedElement(call, null)], input.Order));
            }

            foreach (var input in inputComments)
            {
                var over = string.IsNullOrWhiteSpace(input.OverKey)
                    ? null
                    : components[input.OverKey];
                
                var comment = new Comment
                (   
                    cases[input.CaseKey], 
                    over,
                    input.Text,
                    Comparer.Null
                );

                result.Add(new ComposedCaseElement(comment.Case, [new OrderedElement(comment, null)], input.Order));
            }

            return result;
        }

        private IEnumerable<ComposedCaseElement> BuildCaseRefs(
            IEnumerable<ComposedCaseElement> elements, 
            IDictionary<string, Case> cases, 
            IDictionary<string, Component> components)
        {
            var results = new List<ComposedCaseElement>();
            var refs = inputRefs
                .Select(x => new CaseRef(cases[x.CaseFromKey], cases[x.CaseToKey], x.Order))
                .ToList();
            
            foreach (var caseRef in refs)
            {
                BuildCaseRef(new Stack<Case>(), caseRef, refs, elements, cases, components, results);
            }

            return results;
        }

        private void BuildCaseRef(
            Stack<Case> stack, 
            CaseRef currentRef,
            IEnumerable<CaseRef> refs,
            IEnumerable<ComposedCaseElement> elements, 
            IDictionary<string, Case> cases, 
            IDictionary<string, Component> components,
            List<ComposedCaseElement> results)
        {
            stack.Push(currentRef.From);

            var initialCase = stack.First();

            if (stack.Contains(currentRef.To))
            {
                var comment = new Comment(initialCase, null, $"<recursive call of {currentRef.To.Key}>", Comparer.Null);
                
                results.Add(new ComposedCaseElement(initialCase, [new OrderedElement(comment, null)], currentRef.Order));
            }
            else
            {
                foreach (var nextRef in refs.Where(x => x.From == currentRef.To))
                {
                    BuildCaseRef(stack, nextRef, refs, elements, cases, components, results);
                }

                var caseElements = CombineCase(elements.Where(x => x.Case == currentRef.To));

                results.Add(new ComposedCaseElement(initialCase, caseElements, currentRef.Order));
            }

            stack.Pop();
        }

        private IEnumerable<OrderedElement> CombineCase(IEnumerable<ComposedCaseElement> elements)
        {
            return elements
                .SelectMany(x => x.Elements, (x, y) => (FirstOrder: x.Order, SecondOrder: y.Order, y.Element))
                .OrderBy(x => x.FirstOrder)
                .ThenBy(x => x.SecondOrder)
                .Select((x, i) => new OrderedElement(x.Element, i));
        }

        private IEnumerable<Element> ExtractCallAndComments(IEnumerable<ComposedCaseElement> elements)
        {
            var orderVisitor = new UpdateOrderVisitor();
            var results = elements
                .GroupBy(x => x.Case)
                .Select(CombineCase)
                .SelectMany(x => x);

            foreach (var result in results)
            {
                orderVisitor.Set(result.Order);
                result.Element.Accept(orderVisitor);

                yield return result.Element;
            }
        }

        private class UpdateOrderVisitor : IVisitor
        {
            private float? order;

            public void Set(float? order)
            {
                this.order = order;
            }
            
            public void Visit(Case value)
            {
            }

            public void Visit(Component value)
            {
            }

            public void Visit(Call value)
            {
                value.Order.Set(order);
            }

            public void Visit(Comment value)
            {
                value.Order.Set(order);
            }
        }
    }
}
