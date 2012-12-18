using System;
using Raven.Client.Contrib.Tests.Listeners.HaackHttpSimulator;

namespace Raven.Client.Contrib.Tests.Listeners
{
	internal static class HttpContextHelper
	{
		internal static IDisposable SimulateRequest()
		{
			var httpSimulator = new HttpSimulator("/webapp", "c:\\inetpub\\wwwroot\\webapp\\");
			httpSimulator.SimulateRequest(new Uri("http://localhost/default.aspx"));
			return httpSimulator;
		}
	}
}