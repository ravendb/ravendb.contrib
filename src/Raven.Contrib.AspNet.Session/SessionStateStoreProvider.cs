using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Raven.Contrib.AspNet.Session
{
    /// <summary>
    /// Session state store provider for RavenDB.
    /// </summary>
    /// 
    /// <remarks>
    /// The following configuration options can be specified:
    /// 
    ///   disableLock          = Disables locking on the session data.
    ///                          Defaults to false. Recommended setting to true.
    /// 
    ///   conflictRetries      = The number of retries on conflicts.
    ///                          Defaults to 3.
    /// 
    ///   connectionStringName = The name of the connection string to use.
    ///                          Optional if a document store is provided manually.
    /// 
    /// Due to the usage of RavenDB's Expiration bundle, this provider does not
    /// support the item expiration callback.
    /// </remarks>
    public class SessionStateStoreProvider : SessionStateStoreProviderBase, IDisposable
    {
        private IDocumentSession    _db;

        private int _retries  = 3;
        private bool _locking = true;

        private static int _timeout;
        private static IDocumentStore _store;
        private static bool _disposeStore = true;

        /// <summary>
        /// The document store to use. If null, creates a new document store, using the
        /// <c>connectionStringName</c> configuration option.
        /// </summary>
        public static IDocumentStore Store
        {
            get
            {
                return _store;
            }
            set
            {
                _store        = value;
                _disposeStore = false;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="SessionStateStoreProvider"/>.
        /// </summary>
        public SessionStateStoreProvider()
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="SessionStateStoreProvider"/>,
        /// using the specified <see cref="Raven.Client.IDocumentStore"/>.
        /// </summary>
        public SessionStateStoreProvider(IDocumentStore documentStore)
        {
            Store = documentStore;
        }

        /// <summary>
        /// Initializes the session state store provider.
        /// </summary>
        /// 
        /// <param name="name">
        /// The friendly name of the provider.
        /// </param>
        /// 
        /// <param name="config">
        /// A collection of the name/value pairs representing the provider-specific attributes
        /// specified in the configuration for this provider.
        /// </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["disableLock"] != null)
            {
                bool disableLock;

                if (Boolean.TryParse(config["disableLock"], out disableLock))
                    _locking = !disableLock;
            }

            if (config["conflictRetries"] != null)
            {
                int conflictRetries;

                if (Int32.TryParse(config["conflictRetries"], out conflictRetries))
                    _retries = conflictRetries;
            }

            if (_store == null)
            {
                if (String.IsNullOrEmpty(config["connectionStringName"]))
                    throw new ConfigurationErrorsException("Must supply a connectionStringName.");

                _store = new DocumentStore
                {
                    ConnectionStringName = config["connectionStringName"],
                };

                _store.Initialize();
            }

            var sessionConfig = (SessionStateSection) WebConfigurationManager.GetSection("system.web/sessionState");
            _timeout          = (int) sessionConfig.Timeout.TotalMinutes;
        }

        /// <summary>
        /// Called by the <see cref="System.Web.SessionState.SessionStateModule"/> object for
        /// per-request initialization.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        public override void InitializeRequest(HttpContext context)
        {
            _db = _store.OpenSession();
        }

        /// <summary>
        /// Called by the <see cref="System.Web.SessionState.SessionStateModule"/> object at the end
        /// of a request.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        public override void EndRequest(HttpContext context)
        {
            _db.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_store != null && _disposeStore)
                _store.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="System.Web.SessionState.SessionStateStoreData"/> object to be used
        /// for the current request.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="timeout">
        /// The timeout value for the new <see cref="System.Web.SessionState.SessionStateStoreData"/>.
        /// </param>
        /// 
        /// <returns>
        /// A new <see cref="System.Web.SessionState.SessionStateStoreData"/> for the current request.
        /// </returns>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            var items   = new SessionStateItemCollection();
            var objects = SessionStateUtility.GetSessionStaticObjects(context);

            return new SessionStateStoreData(items, objects, timeout);
        }

        /// <summary>
        /// Adds a new session-state item to the data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="timeout">
        /// The timeout for the current request.
        /// </param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var session = new Session
            {
                Id      = id,
                Expires = DateTime.UtcNow.AddMinutes(timeout)
            };

            _db.Store(session);
            _db.Advanced.GetMetadataFor(session)["Raven-Expiration-Date"] = new RavenJValue(session.Expires);
            _db.SaveChanges();
        }

        /// <summary>
        /// Returns read-only session-state data from the session data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="locked">
        /// When this method returns, contains a Boolean value that is set to true if the requested
        /// session item is locked at the session data store; otherwise, false.
        /// </param>
        /// 
        /// <param name="lockAge">
        /// When this method returns, contains a System.TimeSpan object that is set to the amount of time
        /// that an item in the session data store has been locked.
        /// </param>
        /// 
        /// <param name="lockId">
        /// When this method returns, contains an object that is set to the lock identifier for the current
        /// request. For details on the lock identifier, see "Locking Session-Store Data" in the
        /// <see cref="System.Web.SessionState.SessionStateStoreProviderBase"/> class summary.
        /// </param>
        /// 
        /// <param name="actions">
        /// When this method returns, contains one of the <see cref="System.Web.SessionState.SessionStateActions"/>
        /// values, indicating whether the current session is an uninitialized, cookieless session.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="System.Web.SessionState.SessionStateStoreData"/> populated with session values
        /// and information from the session data store.
        /// </returns>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked,
                                                      out TimeSpan lockAge, out object lockId,
                                                      out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, 0, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Returns read-only session-state data from the session data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="locked">
        /// When this method returns, contains a Boolean value that is set to true if a lock is
        /// successfully obtained; otherwise, false.
        /// </param>
        /// 
        /// <param name="lockAge">
        /// When this method returns, contains a System.TimeSpan object that is set to the amount of time
        /// that an item in the session data store has been locked.
        /// </param>
        /// 
        /// <param name="lockId">
        /// When this method returns, contains an object that is set to the lock identifier for the current
        /// request. For details on the lock identifier, see "Locking Session-Store Data" in the
        /// <see cref="System.Web.SessionState.SessionStateStoreProviderBase"/> class summary.
        /// </param>
        /// 
        /// <param name="actions">
        /// When this method returns, contains one of the <see cref="System.Web.SessionState.SessionStateActions"/>
        /// values, indicating whether the current session is an uninitialized, cookieless session.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="System.Web.SessionState.SessionStateStoreData"/> populated with session values
        /// and information from the session data store.
        /// </returns>
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
        /// Updates the session-item information in the session-state data store with values from the current
        /// request, and clears the lock on the data.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="item">
        /// The <see cref="System.Web.SessionState.SessionStateStoreData"/> object that contains the current
        /// session values to be stored.
        /// </param>
        /// 
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        /// 
        /// <param name="newItem">
        /// true to identify the session item as a new item; false to identify the session item as an
        /// existing item.
        /// </param>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item,
                                                        object lockId, bool newItem)
        {
            var session = newItem ? new Session { Id = id } : _db.Load<Session>(id);

            if (session == null)
                return;

            if (_locking && session.Locked && session.LockId != (Guid) lockId)
                return;

            session.Locked       = false;
            session.Expires      = DateTime.UtcNow.AddMinutes(_timeout);
            session.SessionItems = Serialize((SessionStateItemCollection) item.Items);

            _db.Store(session);
            _db.Advanced.GetMetadataFor(session)["Raven-Expiration-Date"] = new RavenJValue(session.Expires);
            _db.SaveChanges();
        }

        /// <summary>
        /// Releases a lock on an item in the session data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            var session = _db.Load<Session>(id);

            if (session == null)
                return;

            if (_locking && session.Locked && session.LockId != (Guid) lockId)
                return;

            session.Locked = false;

            _db.SaveChanges();
        }

        /// <summary>
        /// Updates the expiration date and time of an item in the session data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var session = _db.Load<Session>(id);

            if (session == null)
                return;

            session.Expires = DateTime.UtcNow.AddMinutes(_timeout);

            _db.Advanced.GetMetadataFor(session)["Raven-Expiration-Date"] = new RavenJValue(session.Expires);
            _db.SaveChanges();
        }

        /// <summary>
        /// Deletes item data from the session data store.
        /// </summary>
        /// 
        /// <param name="context">
        /// The <see cref="System.Web.HttpContext"/> for the current request.
        /// </param>
        /// 
        /// <param name="id">
        /// The session identifier for the current request.
        /// </param>
        /// 
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        /// 
        /// <param name="item">
        /// The <see cref="System.Web.SessionState.SessionStateStoreData"/> that represents the item to
        /// delete from the data store.
        /// </param>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            var session = _db.Load<Session>(id);

            if (session == null)
                return;

            if (_locking && session.Locked && session.LockId != (Guid) lockId)
                return;

            _db.Delete(session);
            _db.SaveChanges();
        }

        /// <summary>
        /// Sets a reference to the <see cref="System.Web.SessionState.SessionStateItemExpireCallback"/>
        /// delegate for the Session_OnEnd event defined in the Global.asax file.
        /// </summary>
        /// 
        /// <param name="expireCallback">
        /// The callback delegate for the Session_OnEnd event defined in the Global.asax file.
        /// </param>
        /// 
        /// <returns>
        /// true if the session-state store provider supports calling the Session_OnEnd
        /// event; otherwise, false.
        /// </returns>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        /* GetSessionStoreItem is called by both the GetItem and GetItemExclusive methods.
         * 
         * GetSessionStoreItem retrieves the session data from the data source. If the lockRecord
         * parameter is true (in the case of GetItemExclusive), then GetSessionStoreItem locks
         * the record and sets a new LockId and LockDate, unless locking is disabled.
         */
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

            using (Disposable.Create(() => _db.Advanced.UseOptimisticConcurrency         = false))
            using (Disposable.Create(() => _db.Advanced.AllowNonAuthoritativeInformation = true))
            {
                _db.Advanced.UseOptimisticConcurrency         = true;
                _db.Advanced.AllowNonAuthoritativeInformation = false;

                var session = _db.Load<Session>(id);

                if (session == null)
                    return null;

                if (_locking && session.Locked)
                {
                    locked  = true;
                    lockAge = DateTime.UtcNow.Subtract(session.LockDate);
                    lockId  = session.LockId;

                    return null;
                }

                // We shouldn't get expired items if the expiration bundle is installed.
                // If it isn't installed, or we made the window, we'll delete expired items here.
                if (session.Expires < DateTime.UtcNow)
                {
                    try
                    {
                        _db.Delete(session);
                        _db.SaveChanges();
                    }
                    catch (ConcurrencyException)
                    {
                        // Don't care.
                    }

                    return null;
                }

                if (_locking && lockRecord)
                {
                    session.Locked   = true;
                    session.LockId   = Guid.NewGuid();
                    session.LockDate = DateTime.UtcNow;

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (ConcurrencyException)
                    {
                        // The session was locked by another request. Retry.
                        if (retriesRemaining > 0)
                        {
                            return GetSessionStoreItem(true, context, id, retriesRemaining - 1, out locked,
                                                        out lockAge, out lockId, out actionFlags);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    lockId = session.LockId;
                }

                if (session.Flags == SessionStateActions.InitializeItem)
                {
                    return CreateNewStoreData(context, _timeout);
                }
                else
                {
                    return Deserialize(context, session.SessionItems, _timeout);
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
                var items   = new SessionStateItemCollection();
                var objects = SessionStateUtility.GetSessionStaticObjects(context);

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