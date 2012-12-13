using System;
using System.Collections;
using System.Collections.Generic;
using Raven.Client.Contrib.Extensions;
using Raven.Client.Embedded;
using Raven.Client.Listeners;

namespace Raven.Client.Contrib.Tests.Helpers
{
    public abstract class RavenDbTestBase : IDisposable
    {
        private IDocumentSession _documentSession;
        private IDocumentStore _documentStore;

        protected IList<Type> IndexesToExecute { get; set; }

        protected IEnumerable<IEnumerable> DataToBeSeeded { get; set; }

        private IDocumentStore DocumentStore
        {
            get
            {
                if (_documentStore != null)
                {
                    return _documentStore;
                }

                var documentStore = new EmbeddableDocumentStore
                                    {
                                        RunInMemory = true
                                    };
                //documentStore.OpenSession();
                documentStore.InitializeWithDefaults(DataToBeSeeded, IndexesToExecute);

                // Force query's to wait for index's to catch up. Unit Testing only :P
                documentStore.RegisterListener(new NoStaleQueriesListener());

                _documentStore = documentStore;

                return _documentStore;
            }
        }

        protected IDocumentSession DocumentSession
        {
            get { return _documentSession ?? (_documentSession = DocumentStore.OpenSession()); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (DocumentStore.WasDisposed)
            {
                return;
            }

            // Assert for any errors.
            DocumentStore.AssertDocumentStoreErrors();

            // Clean up.
            DocumentSession.Dispose();
            DocumentStore.Dispose();
        }

        #endregion

        #region Nested type: NoStaleQueriesListener

        public class NoStaleQueriesListener : IDocumentQueryListener
        {
            #region IDocumentQueryListener Members

            public void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
            {
                queryCustomization.WaitForNonStaleResults();
            }

            #endregion
        }

        #endregion
    }
}