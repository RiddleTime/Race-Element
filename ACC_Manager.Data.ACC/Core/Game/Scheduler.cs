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
using System.Diagnostics;

namespace ACCManager.Data.ACC.Core.Game
{
    public class Scheduler
    {
        public static IScheduler AccScheduler;

        public static void RegisterJobs(IServiceCollectionQuartzConfigurator q)
        {

            StdSchedulerFactory factory = new StdSchedulerFactory();
            AccScheduler = factory.GetScheduler().Result;
            AccScheduler.Start();


            IJobDetail job = JobBuilder.Create<ReplaySaver>().WithIdentity(ReplaySaver.Key).Build();
            AccScheduler.ScheduleJob(job, TriggerBuilder.Create()
                        .WithIdentity("triggerIdentityTest")
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5)
                            .WithRepeatCount(5))
                            .ForJob(ReplaySaver.Key).Build()
                        );
        }
    }
}
