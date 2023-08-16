using System.Collections.Generic;

namespace RaceElement.Core.Jobs
{
    public static class JobScheduler
    {
        public static List<AbstractJob> ActiveJobs { get; private set; }

    }
}
