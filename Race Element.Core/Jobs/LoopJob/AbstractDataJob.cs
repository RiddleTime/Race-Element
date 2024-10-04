using RaceElement.Core.Jobs.LoopJob;
using System;

namespace Race_Element.Core.Jobs.LoopJob;
public abstract class AbstractDataJob<T> : AbstractLoopJob where T : struct
{
    public event EventHandler<T> OnCollected;
    public abstract T Collect();
    public sealed override void RunAction() => OnCollected.Invoke(this, Collect());
}
