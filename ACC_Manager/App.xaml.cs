using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using RaceElement.Data.ACC.Core.Game.Jobs;
using System.Globalization;
using System.Windows;

namespace RaceElement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Instance { get; private set; }
        public bool StartMinimized = false;

        public App()
        {
            this.Startup += App_Startup;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Instance = this;

            var builder = Host.CreateDefaultBuilder()
             .ConfigureServices((cxt, services) =>
             {
                 services.AddQuartz(q =>
                 {
                     q.UseMicrosoftDependencyInjectionJobFactory();

                     IJobDetail job = JobBuilder.Create<ReplaySaver>().WithIdentity(ReplaySaver.Key).Build();
                     q.ScheduleJob<ReplaySaver>(trigger => trigger.WithIdentity("aaaa").WithSimpleSchedule(x => x.WithIntervalInSeconds(5).WithRepeatCount(5))
                                                           .ForJob(ReplaySaver.Key)
                            );
                 });
                 services.AddQuartzHostedService(opt =>
                 {
                     opt.WaitForJobsToComplete = false;
                     opt.AwaitApplicationStarted = true;
                 });
             }).Build();

            builder.RunAsync();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/StartMinimized")
                    StartMinimized = true;
            }
        }

    }
}
