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

            IJobDetail job = JobBuilder.Create<Futurejob>().Build();
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
    }
}