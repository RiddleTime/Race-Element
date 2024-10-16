namespace RaceElement.Core.Jobs.Loop;

public sealed class SimpleLoopJob : AbstractLoopJob
{
    public delegate void RunnableAction();
    public RunnableAction Action { get; set; } = () => { };

    public sealed override void RunAction() => Action();
}
