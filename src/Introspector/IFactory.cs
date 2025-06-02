namespace Introspector;

public interface IFactory
{
    string CreateUseCases();

    string CreateSequence(string useCase);

    string CreateComponents(string useCase);

    string CreateAllComponents();
}