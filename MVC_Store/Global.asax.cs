using MVC_Store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVC_Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            if (User == null)
            {
                return;
            }

            var userName = User.Identity.Name;
            string[] roles;

            using (var db = new Db())
            {
                var dto = db.Users.FirstOrDefault(u => u.UserName == userName);

                if (dto == null)
                    return;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }

            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            Context.User = newUserObj;
        }
    }
}
