using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Jobs
{
    public class BatchProcessJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //Outputs a message to the debug console
            var message = $"Simple executed at ${DateTime.Now.ToString()}";
            Debug.WriteLine(message);
        }
    }
}
