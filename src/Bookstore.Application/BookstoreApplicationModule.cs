using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Bookstore.Authorization;

namespace Bookstore
{
    [DependsOn(
        typeof(BookstoreCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class BookstoreApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<BookstoreAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(BookstoreApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
