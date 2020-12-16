using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Scheduler.Dtos;
using Scheduler.Factorys;
using Scheduler.Jobs;
using Scheduler.Listener;
using Scheduler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();


            //�VDI�e�����UQuartz�A��
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobListener, JobListener>();
            services.AddSingleton<ISchedulerListener, SchedulerListener>();

            //�VDI�e�����UJob
            services.AddSingleton<ReportJob>();

            //�VDI�e�����UJobSchedule
            services.AddSingleton(new JobSchedule(jobName: "111", jobType: typeof(ReportJob), cronExpression: "0/30 * * * * ?"));
            services.AddSingleton(new JobSchedule(jobName: "222", jobType: typeof(ReportJob), cronExpression: "0/52 * * * * ?"));

            //�VDI�e�����UHost�A��
            services.AddSingleton<QuartzHostedService>();
            services.AddHostedService(provider => provider.GetService<QuartzHostedService>());

            // �Q�� ASP.NET ����w���Ƶ{�A���@�ӫe�D�O�u�ݽT�O�����û��B����檬�A�v
            // �n�`�N�bIIS�|�۰ʦ��^���ε{��������ӳy�� scheduler ���_
            // �o�ɭԭn�]�w Preload Enable = true, Start Mode = AlwaysRunning
            // https://docs.microsoft.com/zh-tw/archive/blogs/vijaysk/iis-8-whats-new-website-settings
            // https://docs.microsoft.com/zh-tw/archive/blogs/vijaysk/iis-8-whats-new-application-pool-settings
            // https://blog.darkthread.net/blog/hangfire-recurringjob-notes/


            // ���UDB�e��schedulerHub����
            services.AddSingleton<SchedulerHub>();

            // �]�w SignalR �A��
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // �]�w signalR �� router
                endpoints.MapHub<SchedulerHub>("/schedulerHub");
            });


         
        }
    }
}
