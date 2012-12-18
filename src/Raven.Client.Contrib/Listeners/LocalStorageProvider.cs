using System.Web;

namespace Raven.Client.Listeners
{
	internal static class LocalStorageProvider
	{
		public static AbstractLocalStorage Get()
		{
			if (HttpContext.Current != null)
			{
				return new HttpContextLocalStorage();
			}
			return new ThreadLocalStorage();
		}
	}
}