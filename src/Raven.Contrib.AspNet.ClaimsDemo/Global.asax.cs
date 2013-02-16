using System.Web.Http;
using System.Web.Mvc;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Contrib.AspNet.ClaimsDemo.Controllers;

namespace Raven.Contrib.AspNet.ClaimsDemo
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private DocumentStore _documentStore;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            InitializeDatabase();
            InitializeAuthentication();
            InsertDummyProfile();
        }

        private void InsertDummyProfile()
        {
            using(var session = _documentStore.OpenSession())
            {
                session.Store(new Profile
                {
                    Username = "root",
                    Name = "Root account",
                    Password = "toor",
                    Roles = new string[] { "Admin" }
                }, "profiles/root");
                session.SaveChanges();
            }
        }

        private void InitializeDatabase()
        {
            _documentStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };

            _documentStore.Initialize();

            DependencyResolver.SetResolver(type =>
            {
                if (type == typeof(DocumentStore))
                    return _documentStore;
                return null;
            },
            type => null);
        }

        private void InitializeAuthentication()
        {
            AuthenticationManager.Store = _documentStore;
        }
    }
}