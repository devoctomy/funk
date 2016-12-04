#r "devoctomy.funk.core.dll"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using Newtonsoft.Json.Linq;
using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    JObject pJOtInfo = new JObject();
    pJOtInfo.Add("InfoRequestedAt", new JValue(DateTime.UtcNow.ToString(EnvironmentHelpers.GetEnvironmentVariable("DateTimeFormat"))));
    pJOtInfo.Add("PublicKey", new JValue(EnvironmentHelpers.GetEnvironmentVariable("PublicRSAKey")));

    RSAParametersSerialisable pRSAPrivate = RSAParametersSerialisable.FromJSON(EnvironmentHelpers.GetEnvironmentVariable("PrivateRSAKey"),
        true);
    pRSAPrivate.Sign(pJOtInfo);

    return(req.CreateResponse(HttpStatusCode.OK, pJOtInfo.ToString()));
}