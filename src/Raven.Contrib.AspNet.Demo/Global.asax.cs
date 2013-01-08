using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client;
using Raven.Client.Contrib.Profiling;
using Raven.Client.Embedded;
using Raven.Contrib.AspNet.Auth;
using Raven.Contrib.AspNet.Demo.App_Start;
using Raven.Contrib.AspNet.Session;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;

namespace Raven.Contrib.AspNet.Demo
{
    public class Application : HttpApplication
    {
        private static IDocumentStore _documentStore;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            InitializeDatabase();
            InitializeProfiler();
            InitializeSession();
            InitializeAuthentication();
        }

        protected virtual void Application_BeginRequest()
        {
            MiniProfiler.Start();
        }

        protected virtual void Application_AuthenticateRequest()
        {
            if (!Request.IsLocal)
                MiniProfiler.Stop(discardResults: true);
        }

        protected virtual void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }

        private void InitializeDatabase()
        {
            _documentStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };

            _documentStore.Initialize();
        }

        private void InitializeProfiler()
        {
            MiniProfiler.Settings.IgnoredPaths = new[]
            {
                "/content/",
                "/scripts/",
            };

            // Hook into RavenDB.
            RavenProfiler.InitializeFor(_documentStore);

            // Setup profiler for controllers via a global ActionFilter.
            GlobalFilters.Filters.Add(new ProfilingActionFilter());

            // Intercept ViewEngines to profile all partial views and regular views.
            // If you prefer to insert your profiling blocks manually you can comment this out.
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();

            foreach (var item in copy)
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
        }

        private void InitializeSession()
        {
            SessionStateStoreProvider.Store = _documentStore;
        }

        private void InitializeAuthentication()
        {
            AuthProvider.Configuration.DocumentStore = _documentStore;
        }
    }
}