using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ChatApp.Startup))]
namespace ChatApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // You can configure middleware here, e.g.:
            // app.UseCookieAuthentication(...);
            app.MapSignalR(); // This enables the /signalr/hubs route
        }
    }
}