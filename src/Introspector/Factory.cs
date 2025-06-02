using System.Text;

namespace Introspector;

public static class Factory
{
    public static IFactory Create(string package, Action<IBuilder> configure)
    {
        var builder = new BuilderImpl();

        configure?.Invoke(builder);

        var elements = builder.Build();

        return new FactoryImpl(elements, package);
    }
}

internal class FactoryImpl : IFactory
{
    private readonly IEnumerable<Element> elements;
    private readonly string package;

    public FactoryImpl(IEnumerable<Element> elements, string package)
    {
        this.elements = elements;
        this.package = package;
    }

    public string CreateAllComponents()
    {
        return ComponentsCreator.CreateAll(elements);
    }

    public string CreateComponents(string useCase)
    {
        return ComponentsCreator.Create(useCase, elements);
    }

    public string CreateSequence(string useCase)
    {
        return SequenceCreator.Create(useCase, elements);
    }

    public string CreateUseCases()
    {
        return CasesCreator.Create(elements, package);
    }

    private class CasesCreator : IVisitor
    {
        private readonly List<Case> cases = new();
        private readonly string package;

        private CasesCreator(string package)
        {
            this.package = package;
        }

        void IVisitor.Visit(Case value)
        {
            cases.Add(value);
        }

        void IVisitor.Visit(Component value)
        {
        }

        void IVisitor.Visit(Call value)
        {
        }

        void IVisitor.Visit(Comment value)
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("@startuml");

            if (!string.IsNullOrWhiteSpace(package))
            {
                builder.AppendLine(@$"package ""{package}"" {{");
            }

            foreach (var value in cases)
            {
                value.WriteUseCase(builder);
            }

            if (!string.IsNullOrWhiteSpace(package))
            {
                builder.AppendLine("}");
            }

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        public static string Create(IEnumerable<Element> elements, string package)
        {
            var creator = new CasesCreator(package);

            foreach (var element in elements)
            {
                element.Accept(creator);
            }

            return creator.ToString();
        }
    }

    private class SequenceCreator : IVisitor
    {
        private readonly string caseName;

        private Case currentCase;
        private readonly List<Component> components = new();
        private readonly List<Call> calls = new();
        private readonly List<Comment> comments = new();

        private SequenceCreator(string caseName)
        {
            this.caseName = caseName;
        }

        void IVisitor.Visit(Case value)
        {
            if (value.HasName(caseName))
            {
                currentCase = value;
            }
        }

        void IVisitor.Visit(Component value)
        {
            components.Add(value);
        }

        void IVisitor.Visit(Call value)
        {
            if (value.HasCase(caseName))
            {
                calls.Add(value);
            }
        }

        void IVisitor.Visit(Comment value)
        {
            if (value.HasCase(caseName))
            {
                comments.Add(value);
            }
        }

        private void FilterCallsAndCommentsByComponents()
        {
            calls.RemoveAll(call => !components.Any(component => call.ContainsTo(component)));
            calls.RemoveAll(call => !components.Any(component => call.ContainsFrom(component)));
            comments.RemoveAll(comment => !components.Any(component => comment.ContainsOver(component)));
        }

        private void WriteComponents(StringBuilder builder)
        {
            var written = new HashSet<Component>();
            var orderedCalls = calls
                .OrderBy(call => call.GetFirstCaseOrder(currentCase))
                .ToList();

            foreach (var call in orderedCalls)
            {
                foreach (var toWrite in components.Where(call.ContainsFrom))
                {
                    if (written.Add(toWrite))
                    {
                        toWrite.WriteToSequence(builder);
                    }
                }
            }

            foreach (var call in orderedCalls)
            {
                foreach (var toWrite in components.Where(call.ContainsTo))
                {
                    if (written.Add(toWrite))
                    {
                        toWrite.WriteToSequence(builder);
                    }
                }
            }

            foreach (var comment in comments)
            {
                foreach (var toWrite in components.Where(comment.ContainsOver))
                {
                    if (written.Add(toWrite))
                    {
                        toWrite.WriteToSequence(builder);
                    }
                }
            }
        }

        private void WriteMessages(StringBuilder builder)
        {
            var messages = new List<(float? Order, Action<StringBuilder> Write)>();

            foreach (var call in calls)
            {
                foreach (var order in call.GetCaseOrders(currentCase))
                {
                    messages.Add((order, call.WriteToSequence));
                }
            }

            foreach (var comment in comments)
            {
                foreach (var order in comment.GetCaseOrders(currentCase))
                {
                    messages.Add((order, comment.WriteToSequence));
                }
            }

            foreach (var message in messages.OrderBy(message => message.Order))
            {
                message.Write(builder);
            }
        }

        public override string ToString()
        {
            if (currentCase == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            builder.AppendLine("@startuml");

            currentCase.WriteSequenceTitle(builder);

            WriteComponents(builder);
            FilterCallsAndCommentsByComponents();
            WriteMessages(builder);

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        public static string Create(string caseName, IEnumerable<Element> elements)
        {
            var creator = new SequenceCreator(caseName);

            foreach (var element in elements)
            {
                element.Accept(creator);
            }

            return creator.ToString();
        }
    }

    private class ComponentsCreator : IVisitor
    {
        private readonly string caseName;
        private readonly bool isWholeMap;

        private readonly List<Case> cases = new();
        private readonly List<Component> components = new();
        private readonly List<Call> calls = new();
        private readonly List<Comment> comments = new();

        private ComponentsCreator(string caseName)
        {
            this.caseName = caseName;
        }

        private ComponentsCreator()
        {
            isWholeMap = true;
        }

        void IVisitor.Visit(Case value)
        {
            if (!isWholeMap && !value.HasName(caseName))
            {
                return;
            }

            cases.Add(value);
        }

        void IVisitor.Visit(Component value)
        {
            components.Add(value);
        }

        void IVisitor.Visit(Call value)
        {
            if (!string.IsNullOrWhiteSpace(caseName) && !value.HasCase(caseName))
            {
                return;
            }

            calls.Add(value);
        }

        void IVisitor.Visit(Comment value)
        {
            if (!string.IsNullOrWhiteSpace(caseName) && !value.HasCase(caseName))
            {
                return;
            }

            comments.Add(value);
        }

        private void WriteCaseComponents(StringBuilder builder)
        {
            var written = new HashSet<Component>();

            foreach (var call in calls)
            {
                var toWrite = components.Find(call.ContainsFrom);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder, comments);
                }
            }

            foreach (var call in calls)
            {
                var toWrite = components.Find(call.ContainsTo);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder, comments);
                }
            }

            foreach (var comment in comments)
            {
                var toWrite = components.Find(comment.ContainsOver);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder, comments);
                }
            }
        }

        private void FilterCallsAndCommentsByComponents()
        {
            calls.RemoveAll(call => !components.Any(component => call.ContainsTo(component)));
            calls.RemoveAll(call => !components.Any(component => call.ContainsFrom(component)));
            comments.RemoveAll(comment => !components.Any(component => comment.ContainsOver(component)));
        }

        private void WriteCalls(StringBuilder builder)
        {
            foreach (var call in calls)
            {
                call.WriteToComponents(builder);
            }
        }

        private void WriteAllComponents(StringBuilder builder)
        {
            foreach (var component in components)
            {
                component.WriteToComponents(builder, comments);
            }
        }

        public override string ToString()
        {
            if (cases.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            builder.AppendLine("@startuml");

            if (isWholeMap)
            {
                WriteAllComponents(builder);
            }
            else
            {
                WriteCaseComponents(builder);
            }

            FilterCallsAndCommentsByComponents();
            WriteCalls(builder);

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        public static string Create(string caseName, IEnumerable<Element> elements)
        {
            var creator = new ComponentsCreator(caseName);

            foreach (var element in elements)
            {
                element.Accept(creator);
            }

            return creator.ToString();
        }

        public static string CreateAll(IEnumerable<Element> elements)
        {
            var creator = new ComponentsCreator();

            foreach (var element in elements)
            {
                element.Accept(creator);
            }

            return creator.ToString();
        }
    }
}