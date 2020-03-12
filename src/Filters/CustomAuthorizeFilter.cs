using Hangfire.Dashboard;

namespace WebApi.Filters
{
    /// <summary>
    /// This authorization is specially needed for HangFire to work in production environments
    /// </summary>
    public class CustomAuthorizeFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            //var httpcontext = context.GetHttpContext();
            //return httpcontext.User.Identity.IsAuthenticated;//Use this for actual authorization for HangFire
            return true; //Bypassing the authorizzation as its all internal as of now.
        }
    }
}