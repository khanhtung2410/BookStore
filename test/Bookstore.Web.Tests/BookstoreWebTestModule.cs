using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Bookstore.EntityFrameworkCore;
using Bookstore.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Bookstore.Web.Tests
{
    [DependsOn(
        typeof(BookstoreWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class BookstoreWebTestModule : AbpModule
    {
        public BookstoreWebTestModule(BookstoreEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(BookstoreWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(BookstoreWebMvcModule).Assembly);
        }
    }
}