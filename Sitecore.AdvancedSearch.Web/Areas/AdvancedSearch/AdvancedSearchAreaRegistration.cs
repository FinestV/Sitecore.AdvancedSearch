using System.Web.Mvc;

namespace Sitecore.AdvancedSearch.Web.Areas.AdvancedSearch
{
    public class AdvancedSearchAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AdvancedSearch";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AdvancedSearch_default",
                "AdvancedSearch/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}