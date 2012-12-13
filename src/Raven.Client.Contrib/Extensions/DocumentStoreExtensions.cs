using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Raven.Abstractions.Data;
using Raven.Client.Indexes;

namespace Raven.Client.Contrib.Extensions
{
    public static class DocumentStoreExtensions
    {
        public static void InitializeWithDefaults(this IDocumentStore documentStore)
        {
            InitializeWithDefaults(documentStore, null, null, typeof (DocumentStoreExtensions));
        }

        public static void InitializeWithDefaults(this IDocumentStore documentStore, IEnumerable<IEnumerable> seedData)
        {
            InitializeWithDefaults(documentStore, seedData, null, typeof (DocumentStoreExtensions));
        }

        public static void InitializeWithDefaults(this IDocumentStore documentStore, IEnumerable<IEnumerable> seedData,
                                                  IEnumerable<Type> indexesToExecute)
        {
            InitializeWithDefaults(documentStore, seedData, indexesToExecute, null);
        }

        public static void InitializeWithDefaults(this IDocumentStore documentStore, IEnumerable<IEnumerable> seedData,
                                                  Type assemblyToScanForIndexes)
        {
            InitializeWithDefaults(documentStore, seedData, null, assemblyToScanForIndexes);
        }

        public static void AssertDocumentStoreErrors(this IDocumentStore documentStore)
        {
            if (documentStore == null)
            {
                throw new ArgumentNullException("documentStore");
            }

            ServerError[] errors = documentStore.DatabaseCommands.GetStatistics().Errors;
            if (errors == null || errors.Length <= 0)
            {
                return;
            }

            // We have some Errors. NOT. GOOD. :(
            var errorMessages = new StringBuilder();
            foreach (ServerError serverError in errors)
            {
                string errorMessage = string.Format("Document: {0}; Index: {1}; Error: {2}",
                                                    string.IsNullOrEmpty(serverError.Document)
                                                        ? "No Document Id"
                                                        : serverError.Document,
                                                    string.IsNullOrEmpty(serverError.Index)
                                                        ? "No Index"
                                                        : serverError.Index,
                                                    string.IsNullOrEmpty(serverError.Error)
                                                        ? "No Error message .. err??"
                                                        : serverError.Error);

                Debug.WriteLine(errorMessage);
                errorMessages.AppendLine(errorMessage);
            }

            throw new InvalidOperationException("DocumentStore has some errors. Dast is nict gut.");
        }

        public static void WaitForStaleIndexesToComplete(this IDocumentStore documentStore)
        {
            while (documentStore.DatabaseCommands.GetStatistics().StaleIndexes.Length != 0)
            {
                Thread.Sleep(50);
                Debug.WriteLine("Waiting for indexes to stop being stale ...");
            }
        }

        private static void InitializeWithDefaults(IDocumentStore documentStore,
                                                   IEnumerable<IEnumerable> seedData,
                                                   IEnumerable<Type> indexesToExecute,
                                                   Type assemblyToScanForIndexes)
        {
            if (documentStore == null)
            {
                throw new ArgumentNullException("documentStore");
            }

            // Default initializtion;
            documentStore.Initialize();

            // Index initialisation.
            if (indexesToExecute != null)
            {
                Type[] indexes = (from type in indexesToExecute
                                  where type.IsSubclassOf(typeof (AbstractIndexCreationTask))
                                  select type).ToArray();

                IndexCreation.CreateIndexes(new CompositionContainer(new TypeCatalog(indexes)), documentStore);
            }
            else if (assemblyToScanForIndexes != null)
            {
                IndexCreation.CreateIndexes(assemblyToScanForIndexes.Assembly, documentStore);
            }

            // Create our Seed Data (if provided).
            if (seedData != null)
            {
                CreateSeedData(seedData, documentStore);
            }

            // Now lets check to make sure there are now errors.
            documentStore.AssertDocumentStoreErrors();
        }

        private static void CreateSeedData(IEnumerable<IEnumerable> seedData, IDocumentStore documentStore)
        {
            if (documentStore == null)
            {
                throw new ArgumentNullException("documentStore");
            }

            if (seedData == null)
            {
                throw new ArgumentNullException("seedData");
            }

            using (IDocumentSession documentSession = documentStore.OpenSession())
            {
                // First, check to make sure we don't have any data.
                if (documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfDocuments > 0)
                {
                    // We have documents, so nothing to worry about :)
                    return;
                }

                // Store each collection of fake seeded data.
                foreach (var collection in seedData)
                {
                    foreach (var entity in collection)
                    {
                        documentSession.Store(entity);
                    }
                }

                // Commit this transaction.
                documentSession.SaveChanges();

                // Make sure all our indexes are not stale.
                documentStore.WaitForStaleIndexesToComplete();
            }
        }
    }
}