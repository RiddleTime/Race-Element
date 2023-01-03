using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceElement.Data.ACC.Core.Game.Jobs;

namespace ACCManager.Data.ACC.Core.Game
{
    public class ACCJobs
    {
        public static void RegisterJobs(IServiceCollectionQuartzConfigurator q)
        {
            IJobDetail job = JobBuilder.Create<ReplaySaver>().WithIdentity(ReplaySaver.Key).Build();
            q.ScheduleJob<ReplaySaver>(trigger => trigger.WithIdentity("aaaa").WithSimpleSchedule(x => x.WithIntervalInSeconds(5).WithRepeatCount(5))
                                                  .ForJob(ReplaySaver.Key));
        }
    }
}
