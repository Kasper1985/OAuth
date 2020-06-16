using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace OAuthServer.Providers
{
    public class OAuthIDTokenProvider
    {
        private readonly RSAParameters privateKey;
        public RSAParameters PublicKey => new RSAParameters() { Modulus = this.privateKey.Modulus, Exponent = this.privateKey.Exponent };

        public OAuthIDTokenProvider()
        {
            try
            {
                using (var RSAalg = new RSACryptoServiceProvider())
                {
                    RSAalg.FromXmlString(RSAkey);
                    this.privateKey = RSAalg.ExportParameters(includePrivateParameters: true);
                }
            }
            catch (Exception ex)
            {
                using (var eventLog = new System.Diagnostics.EventLog("Application"))
                {
                    eventLog.Source = "OAuth2";
                    eventLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }

        public string CreateIDToken(object payload)
        {
            string base64Header = this.CreateBase64Header();
            string base64Payload = this.CreateBase64Payload(payload);
            string base64Signature = this.CreateBase64Signature(".", base64Header, base64Payload);

            return string.Join(".", base64Header, base64Payload, base64Signature);
        }

        private string CreateBase64Header()
        {
            Dictionary<string, string> JSONWebKey = new Dictionary<string, string>
            {
                { "kty", "RSA" },
                { "use", "enc" },
                { "n", Convert.ToBase64String(this.privateKey.Modulus) },
                { "e", Convert.ToBase64String(this.privateKey.Exponent) }
            };
            Dictionary<string, object> header = new Dictionary<string, object>
            {
                { "alg", "RS256" },
                { "jwk", JSONWebKey }
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(header)));
        }

        private string CreateBase64Payload(object payload)
        {
            byte[] result = new byte[0];
            string base64result;

            try
            {
                result = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(payload));
            }
            catch (Exception ex)
            {
                result = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(ex));
            }
            finally
            {
                base64result = Convert.ToBase64String(result);
            }

            return base64result;
        }

        private string CreateBase64Signature(string separator, params string[] data)
        {
            string base64signature = "";

            using (var RSAalg = new RSACryptoServiceProvider())
            {
                RSAalg.PersistKeyInCsp = false;
                RSAalg.ImportParameters(this.privateKey);

                byte[] dataToSign = Encoding.UTF8.GetBytes(string.Join(separator, data));

                base64signature = Convert.ToBase64String(RSAalg.SignData(dataToSign, new SHA256CryptoServiceProvider()));
            }

            return base64signature;
        }


        private static string RSAkey =>
            "<RSAKeyValue>" +
                "<Modulus>la3C+lOwyXn/lG+1WNuZoE0Ot1LpXeroob3tXkPor1dEZWs6HwlT5jQ39EK5W3BvmOI0BKm5pmIl0aGPolU2AWWb6J3mMrGiEnZRPsgNGJh6UJgaIqgAkUq11VxjNBoDPFxHpl9YSS2vo8IGqHA8UOBt4JT6Vpwk1LW8YCgD9WbS0OfQZXe0ovsTV9Gdb1tE+by3U46mYhJo6yAjs30+e9gbz+O7VELA2qXntxFu4+9P40NLKTV5qf3Phm+aLGfYZmwww8NgZUKQHx7Os2jimPaZTvCOtAeaccrFUruxnJd9djx4jjOIz1jqZyRqh1RLHiAv0SMsQGzKKDv2cmNgXSttYM5t8n88eG1odqz4mxjyMYt34ncI7uQLxx5Cw2aib4LpDcn27cLzxNR2DYvBV/h1OxBf//SX7eiqDYyA0wE3Hr91xi9Cm39bI4yEk1aY3RXR6Yb/e1FcwOVVps77WmhrzmsznzZQqTeVjgxIVENFh3VoFbFRAEH2M6cxaGaK0DpAvftcXGA1ugv+5rxDjysRcU26XxVX9KtPKC4rM+6HU6BITmMmq1nwpkx1ODnCXpMKgE56S8F1DEQlCnwaa1WYtMWeEZExk2vLUjmSf/h0DmgC/Fh8qDaOejI2nwM4PWZ1EYKh+AGhn5EMZ8dpnCeVGlV9vGZES4xUKNmx5Zc=</Modulus>" +
                "<Exponent>AQAB</Exponent>" +
                "<P>xuWvdvybXOeqVGQPulHIQMegS6KNHKSeZoFZwA3rKVoSVvYFwDHNre8u39Y5K7sSvZeH23mwxu9R9MD2LdRCPHkW+WrjITT8A779BzWuMWhxjF6ru9QPs+LdnOopGOcN0tfwkVzh6hAPoJP2YsV/0JyooCdFWuzmJTwsNCVZE83gHrd/MGj62FLyg6NJm7lm5KYAB+GMHarOJbQKUhC6byBjGQYmeLNXLsYXA+suL3kUCEHTQt7L8BLRdGG+/MySHQaikB3G+GClLZ74oEWBgDG7ZUrCxGyDZLf9pn5RG7bN5WuaPm8UP9TrSSH7QN4g6BQgNWl3Mxe+cvhmHVE7YQ==</P>" +
                "<Q>wKatCmo6C5g1JXU+RhCb9NnEX+cU7sHdKOwP7cxFS7LhjZw2aGJ+lyNnP4S7MyIfBWzWyqm8Ir+z1dlleqZA0WTj/Yta0jND3KM3Ld/6zWaf6kDpbCe62HSnzoK+VtrxzrQ1ZnMy5f243GRyQ0L62GJXndtpM59cQ9XO3T3WoD5ioGQItbbBFUOBlArt0/d0sjto5N1Ps9yi9YMOEWz1jXicZd8vmc3AfW9kOK0amfAZxYt+PZKkPmejhU82EMDtxbqEkj/buGjtZKCIhkvXHlJ0+HLU0A/BvVrS+OVzqbSD9QfRUd2IZg8xQA6s3HUjmSadPZKoWPOU0xKalwp79w==</Q>" +
                "<DP>KM0NRZdkN5a+djlgkJQeREuvMlTSJcIPTG/UfXVdfEoh4PzvCRIuYzPhpcFIURQ7L/FZyUgZX3XQlijPV29ndwhQEjI/DZzU5d/UABqe9Tu7r1PsXseNwRUU6Q+QBq+/QjVSAD/RiDZrPkGcFX72TVEVtmTX8WMPgyvo1Xmj4FAnXaW+zat7acz6Os7QVsWdueHjt7opCBz11P4t78/Gru0x5vxAABqqE8pHBmLEQrDPzzHRqLITm9gAZ6N1jl5YyuOFjZLOMj2BJO6rihU4D5gkHQ9NDQZFne+wdOsMsGa7rgh4x735pW6EJah25kOq9HDMeqh771s7klIqgHEdgQ==</DP>" +
                "<DQ>m5R/Xk/XPJHZKbz7Y536NMzA2CpIL2EEeA3b91DmDXIS0Uf8cGRKk0gWE6fffjXqE3mIJPbaZr2mP+dnGpu08OWncZTe7a5U+ZQd9cBSTRq5vnZZi+yx6iibdB0GFiKO8T17epdB3Zb/eYzs8gDfj2wsEeGlnfjujCHZA7by40o+YAUpO4axe7iepq4Ezw+igGWWGm3X9gsS127VJ6F01KE4vNR3HqCY+TPCHjs9pl/OkEhM1PDeDz+nQPttUC5eVzZ/5ZQIn26teHxkhQAuY+B3ckVaNATlk0QL6NF0jPKpNnLvyks74iAWyAKR+x/mdcJHbfhIWD2uTJ9TlPL0lQ==</DQ>" +
                "<InverseQ>vmbnhHbck0ujuJzc7T1jsr4C+G1M0zbVjkGuCmlT92gar1guzcNQRzJz9j18Ge7xCFA0AQsDturSUAO/ik4On9AuxUQI0VqX/nespaw6lBXRyZ8Z72NaPGRLpG+HHdO95OLpJst362RVu6MlEZtC/cIwdP2mdCS5a1XgD5oxN57vUMA/WEx6WMN20yn9plfa7Oi0wZ67sfNSltAf/9c1pD4TeXZVybIAzIMtYiE1uNyBA81z1fF45vh71FZt39cb8DtPEFxYuF2osyMWLe/nxaOk1UiqEItlFghL/JyCaXfPA+IKWyPgb5YX4pP/ikZ8MFkTJNbOfrQ7SI7tfLq+cg==</InverseQ>" +
                "<D>kUJrAOmLnlxJj1LCGhQ0iX9/EqbiWAUL8zwRSvR4swHsHMjtRQWWaMiwrNq8bGM3ypVsikH1K+YAcetrK82qdbyYjC4BI52EfopY2qbnHsGLov+aDZwhwN9SjFf25q+ACGo+FtZdJdeypyg7segKgSrtN5EdSkSK8evVfzQWc2hwm9D9TBvaMHmeEhIZyeI3lG5zNCIykubu75DnVN/tbd8TTrPiLyoBY7CyqdW/WnH7g8DhBoMhKbpqVjuk0geN2giRYssPl/X7TFhnnXHixzQ7IIMQ2uKuHlT9HweMnT3fwol5LfgRg7j081hCEkfaBsiR31NVNMjfgCCBUg3ji8wFKlle1/oi2gU1H/IubeI5njqMS/SpWWf7sQ7QThsqSSgndZY71CIIEX4taEdKu1INz7tCQinKUsopzShQf/LYxYNlUV9U0Kn9SLn/qGm+K2C6RWtYd4sdQtVS7baKQPDUSfSB2+TWbqZsr8kFmjbAz6URQgBaEyyUf93Snu+LJd8nA8+kYMocjJVIYHfdv23HF7qXHJMJn0SONXUq4+w8sWUdWYNazXFDPc1NSworZMtmTzAhJIcFryHQeqQpGq7/RjPy6ifah0iZCjR5YClGQqSRJ/OXtJBCWxVkj+x+HPJo/xWphNoJl0pfFykIOdP1ykMAKqsVsszzXoStmIE=</D>" +
            "</RSAKeyValue>";
    }
}