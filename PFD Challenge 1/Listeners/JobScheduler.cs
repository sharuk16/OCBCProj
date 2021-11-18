using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Listeners
{
    public class JobScheduler
    {
        public static void Start()
        {
            Task<IScheduler> schedulerjob = StdSchedulerFactory.GetDefaultScheduler();
            IScheduler scheduler = schedulerjob.Result;
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<Incompletescanjob>().Build();
            //trigger the job
            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(10) //run the method every 10 second
             .RepeatForever())
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
        public static void FutureScanJob()
        {
            Task<IScheduler> schedulerjob = StdSchedulerFactory.GetDefaultScheduler();
            IScheduler scheduler = schedulerjob.Result;
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<Futurescanjob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
           .WithIdentity("trigger2", "group1")
           .StartNow()
           .WithSimpleSchedule(x => x
           .WithIntervalInSeconds(10) //run the method every 10 second
           .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger);
        }
        public static void ResetDailySpentJob()
        {
            Task<IScheduler> schedulerjob = StdSchedulerFactory.GetDefaultScheduler();
            IScheduler scheduler = schedulerjob.Result;
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<ResetDailySpentJob>().Build();
            //trigger the job
               ITrigger trigger = TriggerBuilder.Create()
               .WithDailyTimeIntervalSchedule
                 (s =>
                   s.OnEveryDay()
                   .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0)) //will run at 12 AM every day
                   )
                 .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}