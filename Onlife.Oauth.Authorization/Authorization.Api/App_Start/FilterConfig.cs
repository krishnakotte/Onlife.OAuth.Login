using System.Linq;
using System.Web;
using System.Web.Mvc;
using Authorization.Api.Attributes;
using Authorization.Api.DependencyResolution;

namespace Authorization.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new ApiHandleErrorAttribute());

            var container = IoC.Initialize();
            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);
            FilterProviders.Providers.Add(new InjectableFilterProvider(container));
        }
    }
}