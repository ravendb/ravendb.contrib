using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Raven.Contrib.AspNet.Extensions
{
    public static class HttpContextExtensions
    {
        public static Uri PublicUri(this HttpContext context)
        {
            return new HttpContextWrapper(context).PublicUri();
        }

        public static Uri PublicUri(this HttpContextBase context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var url  = context.Request.Url;
            var vars = context.Request.ServerVariables;

            if (url == null || vars == null)
                throw new InvalidOperationException("Invalid request");

            // Due to URL rewriting, cloud computing (i.e. Azure)
            // and web farms, etc., we have to be VERY careful about what
            // we consider the incoming URL.  We want to see the URL as it would
            // appear on the public-facing side of the hosting web site.
            //
            // HttpRequest.Url gives us the internal URL in a cloud environment,
            // So we use a variable that (at least from what I can tell) gives us
            // the public URL:
            if (vars["HTTP_HOST"] != null)
            {
                var scheme  = vars["HTTP_X_FORWARDED_PROTO"] ?? url.Scheme;
                var address = new Uri(scheme + Uri.SchemeDelimiter + vars["HTTP_HOST"]);

                var publicUri = new UriBuilder(url)
                {
                    Scheme = scheme,
                    Host   = address.Host,
                    Port   = address.Port
                };

                return publicUri.Uri;
            }
            else
            {
                // Failover to the method that works for non-web farm enviroments.
                //
                // We use Request.Url for the full path to the server, and modify it
                // with Request.RawUrl to capture both the cookieless session "directory" if it exists
                // and the original path in case URL rewriting is going on.  We don't want to be
                // fooled by URL rewriting because we're comparing the actual URL with what's in
                // the return_to parameter in some cases.
                //
                // Response.ApplyAppPathModifier(builder.Path) would have worked for the cookieless
                // session, but not the URL rewriting problem.
                return new Uri(url, context.Request.RawUrl);
            }
        }
    }
}
