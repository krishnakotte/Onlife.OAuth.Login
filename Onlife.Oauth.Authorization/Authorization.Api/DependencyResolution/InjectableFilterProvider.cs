using System.Collections.Generic;
using System.Web.Mvc;
using StructureMap;

namespace Authorization.Api.DependencyResolution
{
    public class InjectableFilterProvider : FilterAttributeFilterProvider
    {
        private readonly IContainer _container;
 
        public InjectableFilterProvider(IContainer container)
        {
            _container = container;
        }

        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor);
            foreach (var filter in filters)
            {
                _container.BuildUp(filter.Instance);
            }
            return filters;
        }
    }
}