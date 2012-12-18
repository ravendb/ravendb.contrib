using Raven.Client.Listeners;

namespace Raven.Client.Contrib.Listeners
{
	/// <summary>
	/// Hook for users to modify all queries globally with context.
	/// </summary>
	public abstract class AbstractDocumentQueryListenerContext : AbstractDocumentListenerContext, IDocumentQueryListener
	{
		void IDocumentQueryListener.BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
		{
			BeforeQueryExecuted(queryCustomization);
		}

		/// <summary>
		/// Allow to customize a query globally
		/// </summary>
		/// <param name="queryCustomization">The query customization.</param>
		protected abstract void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization);
	}
}