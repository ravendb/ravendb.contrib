using System;
using System.Collections.Generic;
using System.Web;

namespace Raven.Client.ContextualListeners
{
	internal class HttpContextLocalStorage : AbstractLocalStorage
	{
		protected override Dictionary<Type, Stack<object>> GetContexts()
		{
			const string key = "ContextualListenersContexts";
			if (HttpContext.Current.Items[key] == null)
			{
				HttpContext.Current.Items.Add(key, new Dictionary<Type, Stack<object>>());
			}
			return (Dictionary<Type, Stack<object>>) HttpContext.Current.Items[key];
		}
	}
}