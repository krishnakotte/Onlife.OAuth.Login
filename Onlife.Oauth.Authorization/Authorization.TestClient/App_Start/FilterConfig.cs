using System.Web;
using System.Web.Mvc;
using Authorization.TestClient.Attributes;

namespace Authorization.TestClient
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyHandleErrorAttribute());
            filters.Add(new OAuthAuthorizeAttribute());
        }
    }

    public class MyHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var test = "";
        }
    }
}