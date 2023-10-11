using System;

namespace RaceElement.Core.Jobs
{
    internal interface IJob
    {
        internal string Name { get; }
        internal Guid Guid { get;}
        internal JobStatus Status { get; }

        public void Start();
        public void Stop();
    }

    public abstract class AbstractJob : IJob
    {
        public abstract JobStatus Initiate();
        public abstract JobStatus Execute();
        public abstract void Complete();

        private Guid Guid = Guid.NewGuid();
        public JobStatus Status = JobStatus.None;
        public readonly bool Repeat = false;

        string IJob.Name { get => throw new NotImplementedException();  }
        Guid IJob.Guid { get => throw new NotImplementedException();  }
        JobStatus IJob.Status { get => throw new NotImplementedException();  }

        public void InitiateJob() => Status = Initiate();
        public void ExecuteJob() => Status = Execute();
        public void CompleteJob()
        {
            Status = JobStatus.Completing;
            Complete();

            Status = Repeat ? JobStatus.Initiating : JobStatus.None;
        }

        public void NextJob()
        {
            switch (Status)
            {
                case JobStatus.None: break;
                case JobStatus.Initiating:
                    {
                        InitiateJob();
                        break;
                    }
                case JobStatus.Executing:
                    {
                        ExecuteJob();
                        break;
                    }
                case JobStatus.Completing:
                    {
                        CompleteJob();
                        break;
                    }
            }
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}