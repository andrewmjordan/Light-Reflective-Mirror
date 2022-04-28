using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Mirror.SimpleWeb
{
    internal class SslConfigLoader
    {
        internal struct Cert
        {
            public string path;
            public string password;
        }
        internal static SslConfig Load(SimpleWebTransport transport)
        {
            // don't need to load anything if ssl is not enabled
            if (!transport.sslEnabled)
                return default;

            string certJsonPath = transport.sslCertJson;

            Cert cert = LoadCertJson(certJsonPath);

            return new SslConfig(
                enabled: transport.sslEnabled,
                sslProtocols: transport.sslProtocols,
                certPath: cert.path,
                certPassword: cert.password
            );
        }

        internal static Cert LoadCertJson(string certJsonPath)
        {
            Cert cert;
            if (File.Exists(certJsonPath))
            {
                string json = File.ReadAllText(certJsonPath);
                cert = JsonConvert.DeserializeObject<Cert>(json);
            }
            else
            {
				cert = new Cert
				{
					path = Environment.GetEnvironmentVariable("CERT_PATH") ?? "cert.pfx",
					password = Environment.GetEnvironmentVariable("CERT_PASSWORD") ?? string.Empty,
				};
			}

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CERT_CONTENT")))
			{
                cert.path = Path.GetTempFileName();
                Console.WriteLine();
                Console.WriteLine(Environment.GetEnvironmentVariable("CERT_CONTENT").Length);
                var bytes = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("CERT_CONTENT"));
                File.WriteAllBytes(cert.path, bytes);
                Console.WriteLine(cert.path);
			}

            if (string.IsNullOrEmpty(cert.path))
            {
                throw new InvalidDataException("Cert Json didn't not contain \"path\"");
            }
            if (string.IsNullOrEmpty(cert.password))
            {
                // password can be empty
                cert.password = string.Empty;
            }

            return cert;
        }
    }
}
