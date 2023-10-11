using System.Collections.Generic;

namespace RaceElement.Core.Jobs
{
    public static class JobScheduler
    {
        public static List<AbstractJob> ActiveJobs { get; private set; }

        public static void CreateJob(AbstractJob job)
        {
            ActiveJobs.Add(job);
        }
    }
}
