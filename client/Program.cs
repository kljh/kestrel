using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Client
{
    public static class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            var storedCerts = CertificateFactory.GetStoredCertificates();
            var foundCerts = storedCerts.Where(x => string.Equals(x.Thumbprint, "B$CD78AF", System.StringComparison.OrdinalIgnoreCase));

            var cert = CertificateFactory.BuildCertificate("Kljh", null);
            bool verified = cert.Verify();
            CertificateFactory.SavePfx(cert, "selfsigned.pfx");


            var urls = new string[] {
                "http://localhost:8086/",
                "https://localhost:8064/" };
            
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; // !!
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler);
            foreach (var url in urls) {
                var uri = new Uri(url);
                for (var i=0; i<3; i++) {
                    var res = await client.GetAsync(uri);
                    Console.WriteLine($"{res.StatusCode} {await res.Content.ReadAsStringAsync()}");
                }
            }

            return 0;
        }
    }
}