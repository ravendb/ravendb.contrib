using System;
using System.Linq;
using Raven.Client.Connection.Profiling;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.Profiling
{
    internal static class JsonFormatter
    {
        public static ProfilingInformation Format(ProfilingInformation information)
        {
            var profilingInformation                  = ProfilingInformation.CreateProfilingInformation(information.Id);
            profilingInformation.At                   = information.At;
            profilingInformation.Context              = information.Context;
            profilingInformation.DurationMilliseconds = information.DurationMilliseconds;
            profilingInformation.Requests             = information.Requests.Select(FormatRequest).ToList();

            return profilingInformation;
        }

        public static RequestResultArgs FormatRequest(RequestResultArgs input)
        {
            return new RequestResultArgs
            {
                DurationMilliseconds = input.DurationMilliseconds,
                At                   = input.At,
                HttpResult           = input.HttpResult,
                Method               = input.Method,
                Status               = input.Status,
                Url                  = input.Url,
                PostedData           = FilterData(input.PostedData),
                Result               = FilterData(input.Result)

            };
        }

        private static string FilterData(string result)
        {
            RavenJToken token;

            try
            {
                token = RavenJToken.Parse(result);
            }
            catch (Exception)
            {
                return result;
            }

            Visit(token);

            return token.ToString(Formatting.Indented);
        }

        private static void Visit(RavenJToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var item in (RavenJObject)token)
                        Visit(item.Value);

                    break;

                case JTokenType.Array:
                    foreach (var items in (RavenJArray)token)
                        Visit(items);

                    break;

                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.None:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(token.Type.ToString());
            }
        }
    }
}
