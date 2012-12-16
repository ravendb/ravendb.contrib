using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using Raven.Abstractions.Exceptions;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.MVC.Session
{
    /// <summary>
    /// Session state store provider for RavenDB. Based on: https://github.com/mjrichardson/RavenDbSessionStateStoreProvider
    /// </summary>
    public class RavenSessionStateStoreProvider : SessionStateStoreProviderBase, IDisposable
    {
        private const int RetriesOnConcurrentConfictsDefault = 3;

        private IDocumentStore _store;
        private SessionStateSection _config;
        private int _retries = RetriesOnConcurrentConfictsDefault;

        /// <summary>
        /// Public parameterless constructor
        /// </summary>
        public RavenSessionStateStoreProvider()
        {
            
        }

        /// <summary>
        /// Constructor accepting a document store instance, used for testing.
        /// </summary>
        public RavenSessionStateStoreProvider(IDocumentStore documentStore)
        {
            _store = documentStore;
        }

        /// <summary>
        /// The name of the application.
        /// Session-data items will be stored against this name.
        /// If not set, defaults to System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
        /// </summary>
        public string ApplicationName
        {
            get;
            set;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name))
                name = "RavenSessionStateStore";

            base.Initialize(name, config);

            if (config["retriesOnConcurrentConflicts"] != null)
            {
                int retriesOnConcurrentConflicts;

                if (Int32.TryParse(config["retriesOnConcurrentConflicts"], out retriesOnConcurrentConflicts))
                    _retries = retriesOnConcurrentConflicts;
            }

            if (String.IsNullOrEmpty(ApplicationName))
                ApplicationName = HostingEnvironment.ApplicationVirtualPath;

            _config = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");

            if (_store == null)
            {
                if (String.IsNullOrEmpty(config["connectionStringName"]))
                    throw new ConfigurationErrorsException("Must supply a connectionStringName.");

                _store = new DocumentStore
                {
                    ConnectionStringName = config["connectionStringName"],
                    Conventions =
                    {
                        FindIdentityProperty = q => q.Name == "SessionId"
                    }
                };

                _store.Initialize();
            }
        }

        /// <summary>
        /// Retrieves session values and information from the session data store and locks the session-item data 
        /// at the data store for the duration of the request. 
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="locked">An output parameter indicating whether the item is currently exclusively locked.</param>
        /// <param name="lockAge">The age of the exclusive lock (if present)</param>
        /// <param name="lockId">The identifier of the exclusive lock (if present)</param>
        /// <param name="actions">Used with sessions whose Cookieless property is true, 
        /// when the regenerateExpiredSessionId attribute is set to true. 
        /// An actionFlags value set to InitializeItem (1) indicates that the entry in the session data store is a 
        /// new session that requires initialization.</param>
        /// <returns>The session data</returns>
        public override SessionStateStoreData GetItemExclusive(HttpContext context,
                                                               string id,
                                                               out bool locked,
                                                               out TimeSpan lockAge,
                                                               out object lockId,
                                                               out SessionStateActions actions)
        {
            return GetSessionStoreItem(true, context, id, _retries, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// This method performs the same work as the GetItemExclusive method, except that it does not attempt to lock the session item in the data store.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="locked">An output parameter indicating whether the item is currently exclusively locked.</param>
        /// <param name="lockAge">The age of the exclusive lock (if present)</param>
        /// <param name="lockId">The identifier of the exclusive lock (if present)</param>
        /// <param name="actions">Used with sessions whose Cookieless property is true, 
        /// when the regenerateExpiredSessionId attribute is set to true. 
        /// An actionFlags value set to InitializeItem (1) indicates that the entry in the session data store is a 
        /// new session that requires initialization.</param>
        /// <returns>The session data</returns>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked,
                                                      out TimeSpan lockAge, out object lockId,
                                                      out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, 0, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// If the newItem parameter is true, the SetAndReleaseItemExclusive method inserts a new item into the data store with the supplied values. 
        /// Otherwise, the existing item in the data store is updated with the supplied values, and any lock on the data is released. 
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="item">The current session values to be stored</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        /// <param name="newItem">If true, a new item is inserted into the store.  Otherwise, the existing item in 
        /// the data store is updated with the supplied values, and any lock on the data is released. </param>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item,
                                                        object lockId, bool newItem)
        {
            var serializedItems = Serialize((SessionStateItemCollection)item.Items);

            using (var db = _store.OpenSession())
            {
                db.Advanced.UseOptimisticConcurrency = true;

                Session sessionState;

                if (newItem)
                {
                    sessionState = db.Query<Session>()
                                     .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                     .SingleOrDefault(x => x.SessionId == id && x.ApplicationName == ApplicationName && x.Expires < DateTime.UtcNow);

                    if (sessionState != null)
                        throw new InvalidOperationException(String.Format("Item aleady exist with SessionId=\"{0}\" and ApplicationName=\"{1}\"", id, lockId));

                    sessionState = new Session(id, ApplicationName);

                    db.Store(sessionState);
                }
                else
                {
                    sessionState = db.Query<Session>()
                                     .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                     .Single(x => x.SessionId == id && x.ApplicationName == ApplicationName && x.LockId == (int) lockId);
                }

                var expiry = DateTime.UtcNow.AddMinutes(_config.Timeout.TotalMinutes);

                sessionState.Expires      = expiry;
                sessionState.SessionItems = serializedItems;
                sessionState.Locked       = false;

                db.Advanced.GetMetadataFor(sessionState)["Raven-Expiration-Date"] = new RavenJValue(expiry);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Releases the lock on an item in the session data store.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            using (var db = _store.OpenSession())
            {
                db.Advanced.UseOptimisticConcurrency = true;

                var sessionState = db.Query<Session>()
                                     .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                     .Single(x => x.SessionId == id && x.ApplicationName == ApplicationName && x.LockId == (int) lockId);

                var expiry = DateTime.UtcNow.AddMinutes(_config.Timeout.TotalMinutes);

                sessionState.Expires = expiry;
                sessionState.Locked  = false;

                db.Advanced.GetMetadataFor(sessionState)["Raven-Expiration-Date"] = new RavenJValue(expiry);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// deletes the session information from the data store where the data store item matches the supplied SessionID value, 
        /// the current application, and the supplied lock identifier.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="lockId">The exclusive-lock identifier.</param>
        /// <param name="item"></param>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            using (var db = _store.OpenSession())
            {
                db.Advanced.UseOptimisticConcurrency = true;

                var sessionState = db.Query<Session>()
                                     .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                     .SingleOrDefault(x => x.SessionId == id && x.ApplicationName == ApplicationName && x.LockId == (int) lockId);

                if (sessionState != null)
                {
                    db.Delete(sessionState);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Resets the expiry timeout for a session item.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
            try
            {
                using (var db = _store.OpenSession())
                {
                    db.Advanced.UseOptimisticConcurrency = true;

                    var sessionState = db.Query<Session>()
                                         .SingleOrDefault(x => x.SessionId == id && x.ApplicationName == ApplicationName);

                    if (sessionState != null)
                    {
                        var expiry = DateTime.UtcNow.AddMinutes(_config.Timeout.TotalMinutes);

                        sessionState.Expires = expiry;

                        db.Advanced.GetMetadataFor(sessionState)["Raven-Expiration-Date"] = new RavenJValue(expiry);
                        db.SaveChanges();
                    }
                }
            }
            catch (ConcurrencyException)
            {
                // Not fatal.
            }
        }

        /// <summary>
        /// Adds an uninitialized item to the session data store.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="id">The session identifier.</param>
        /// <param name="timeout">The expiry timeout in minutes.</param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            using (var db = _store.OpenSession())
            {
                var expiry = DateTime.UtcNow.AddMinutes(timeout);

                var sessionState = new Session(id, ApplicationName)
                {
                    Expires = expiry
                };

                db.Store(sessionState);
                db.Advanced.GetMetadataFor(sessionState)["Raven-Expiration-Date"] = new RavenJValue(expiry);
                db.SaveChanges();
            }
        }


        /// <summary>
        ///  returns a new SessionStateStoreData object with an empty ISessionStateItemCollection object, 
        ///  an HttpStaticObjectsCollection collection, and the specified Timeout value.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        /// <param name="timeout">The expiry timeout in minutes.</param>
        /// <returns>A newly created SessionStateStoreData object.</returns>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        /// <summary>
        /// Takes as input a delegate that references the Session_OnEnd event defined in the Global.asax file. 
        /// If the session-state store provider supports the Session_OnEnd event, a local reference to the 
        /// SessionStateItemExpireCallback parameter is set and the method returns true; otherwise, the method returns false.
        /// </summary>
        /// <param name="expireCallback">A callback.</param>
        /// <returns>False.</returns>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        /// <summary>
        /// Performs any initialization required by your session-state store provider.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        public override void InitializeRequest(HttpContext context)
        {
        }

        /// <summary>
        /// Performs any cleanup required by your session-state store provider.
        /// </summary>
        /// <param name="context">The HttpContext instance for the current request</param>
        public override void EndRequest(HttpContext context)
        {
        }

        public override void Dispose()
        {
            if (_store != null)
                _store.Dispose();
        }

        //
        // GetSessionStoreItem is called by both the GetItem and 
        // GetItemExclusive methods. GetSessionStoreItem retrieves the 
        // session data from the data source. If the lockRecord parameter
        // is true (in the case of GetItemExclusive), then GetSessionStoreItem
        // locks the record and sets a new LockId and LockDate.
        //
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,
                                                          HttpContext context,
                                                          string id,
                                                          int retriesRemaining,
                                                          out bool locked,
                                                          out TimeSpan lockAge,
                                                          out object lockId,
                                                          out SessionStateActions actionFlags)
        {
            // Initial values for return value and out parameters.
            lockAge     = TimeSpan.Zero;
            lockId      = null;
            locked      = false;
            actionFlags = 0;

            using (var db = _store.OpenSession())
            {
                db.Advanced.UseOptimisticConcurrency = true;
                db.Advanced.AllowNonAuthoritativeInformation = false;

                var sessionState = db.Query<Session>()
                                     .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                     .SingleOrDefault(x => x.SessionId == id && x.ApplicationName == ApplicationName);

                if (sessionState == null)
                    return null;

                // Abort if locked.
                if (sessionState.Locked)
                {
                    locked  = true;
                    lockAge = DateTime.UtcNow.Subtract(sessionState.LockDate);
                    lockId  = sessionState.LockId;

                    return null;
                }

                // We shouldn't get expired items if the expiration bundle is installed.
                // If it isn't installed, or we made the window, we'll delete expired items here.
                if (sessionState.Expires < DateTime.UtcNow)
                {
                    try
                    {
                        db.Delete(sessionState);
                        db.SaveChanges();
                    }
                    catch (ConcurrencyException)
                    {
                        // Don't care.
                    }

                    return null;
                }

                if (lockRecord)
                {
                    sessionState.Locked   = true;
                    sessionState.LockId += 1;
                    sessionState.LockDate = DateTime.UtcNow;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (ConcurrencyException)
                    {
                        if (retriesRemaining > 0)
                        {
                            return GetSessionStoreItem(true, context, id, retriesRemaining - 1, out locked, out lockAge, out lockId, out actionFlags);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                lockId = sessionState.LockId;

                var timeout = (int)_config.Timeout.TotalMinutes;

                if (sessionState.Flags == SessionStateActions.InitializeItem)
                {
                    var collection = new SessionStateItemCollection();
                    var objects    = SessionStateUtility.GetSessionStaticObjects(context);

                    return new SessionStateStoreData(collection, objects, timeout);
                }
                else
                {
                    return Deserialize(context, sessionState.SessionItems, timeout);
                }
            }
        }

        private static string Serialize(SessionStateItemCollection items)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (items != null)
                    items.Serialize(writer);

                writer.Flush();
                writer.Close();

                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private static SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(serializedItems)))
            {
                var items    = new SessionStateItemCollection();
                var objects  = SessionStateUtility.GetSessionStaticObjects(context);

                if (stream.Length > 0)
                {
                    using (var reader = new BinaryReader(stream))
                        items = SessionStateItemCollection.Deserialize(reader);
                }

                return new SessionStateStoreData(items, objects, timeout);
            }
        }
    }
}