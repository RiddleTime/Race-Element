using System;

namespace RaceElement.Core.Jobs
{
    public abstract class AbstractJob
    {
        public abstract JobStatus Initiate();
        public abstract JobStatus Execute();
        public abstract void Complete();

        public Guid Guid { get; } = Guid.NewGuid();
        public JobStatus Status { get; private set; } = JobStatus.None;

        public void InitiateJob() => Status = Initiate();
        public void ExecuteJob() => Status = Execute();
        public void CompleteJob()
        {
            Status = JobStatus.Completing;
            Complete();
            Status = JobStatus.None;
        }
    }
}