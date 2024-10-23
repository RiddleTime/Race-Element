using RaceElement.Core.Jobs.Loop;
using System;

namespace Race_Element.Core.Jobs.Loop;
public abstract class AbstractCollectionJob<T> : AbstractLoopJob where T : struct
{
    public event EventHandler<T> OnCollected;
    public abstract T Collect { get; }

    public sealed override void RunAction() => OnCollected?.Invoke(null, Collect);
}
