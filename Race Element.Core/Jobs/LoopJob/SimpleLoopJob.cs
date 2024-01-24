namespace RaceElement.Core.Jobs.LoopJob;

public class SimpleLoopJob : AbstractLoopJob
{
    public delegate void RunnableAction();
    public RunnableAction Action { get; set; } = () => { };

    public override void RunAction() => Action();
}
