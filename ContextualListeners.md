RavenDB.Client.ContextualListeners
==================================

A RavenDB client extension that passing of contextual information into client listeners. You can do this manually by doing something [similar to this](http://dhickey.ie/post/2012/02/12/Getting-contextual-state-into-a-RavenDBs-document-store-listener.aspx), or you can use this library via [NuGet](https://www.nuget.org/packages/RavenDB.Client.ContextualListeners).

There is a small piece about listeners [here](http://ravendb.net/docs/client-api/advanced/document-metadata), under the topic 'Manipulating metadata' but not much else at this moment in time. Basically, listeners allow you to hook into all document operations that occur in each session. The current design of the listeners is that their lifecyle is singleton with respect to the document database, making it a little tricky to pass in contextual information.

There are 4 listeners that allow you to intercept the data sent to, or recieved from a Raven database.

1. IDocumentStoreListener
2. IDocumentDeleteListener
3. IDocumentConversionListener
4. IDocumentQueryListener

Correspondingly, there are 4 context aware listeners that you register:

1. ContextualDocumentStoreListener<>
2. ContextualDocumentDeleteListener<>
3. ContextualDocumentConversionListener<>
4. ContextualDocumentQueryListener<>

... and there are 4 base context type that you inherit from that perform the contextual work:

1. AbstractDocumentStoreListenerContext
2. AbstractDocumentDeleteListenerContext
3. AbstractDocumentConversionListenerContext
4. AbstractDocumentQueryListenerContext

An example is the best way to demonstrate how to use these.

### Example

Say you want to add the current username to the metadata of each document that is stored. Therefore we want an IDocumentStoreListener to intercept each document before storing and add our username metadata.

First define your username document store context type:

<pre>
public class UserNameContext : AbstractDocumentStoreListenerContext
{
	private readonly string _userName;

	public UserNameContext(string userName)
	{
		_userName = userName;
	}

	protected override void AfterStore(string key, object entityInstance, RavenJObject metadata)
	{}

	protected override bool BeforeStore(string key, object entityInstance, RavenJObject metadata)
	{
		metadata.Add("UserName", RavenJToken.FromObject(_userName));
		return false; //return true if you modify the entityInstance
	}
}
</pre>

Register your the listener that will be aware of your context:

<pre>
var documentStore = new DocumentStore()
{
	Url = "http://server"
};

// can be called after .Initialize(), doesn't matter
documentStore.RegisterListener(new ContextualDocumentStoreListener&lt;UserNameContext&gt;()); 
documentStore.Initialize();
</pre>

And the sweet bit.. just open a new context before you open a session:

<pre>
using(new UserNameContext("Damian Hickey"))
using(var session = documentStore.OpenSession())
{
	session.Store(new Doc());
	session.SaveChanges();
}
</pre>