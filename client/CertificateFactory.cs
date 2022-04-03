using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public static class CertificateFactory
    {
        internal static IEnumerable<X509Certificate2> GetStoredCertificates(StoreLocation storeLocation = StoreLocation.CurrentUser, StoreName storeName = StoreName.My)
        {
            using (var store = new X509Store(storeName, storeLocation)) {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.OfType<X509Certificate2>();
                store.Close();
                return certs;
            }
        }

        internal static X509Certificate2 BuildCertificate(string name, X509Certificate2 issuerCert)
        {
            using RSA rsa = RSA.Create();
            byte[] serialNumber = Guid.NewGuid().ToByteArray();

            var certReq = new CertificateRequest($"CN={name}m OU=App, O=Ltd", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = (issuerCert!=null)
                ? (X509Certificate2)certReq.Create(issuerCert, DateTime.UtcNow, DateTime.UtcNow.AddDays(365), serialNumber)
                : (X509Certificate2)certReq.CreateSelfSigned(DateTime.UtcNow, DateTime.UtcNow.AddDays(365));
            return cert;
        }

        internal static X509Certificate2 LoadPfx(string filepath, string pwd)
        {
            using var bf = new FileStream(filepath, FileMode.Open);
            byte[] bytes = new byte[bf.Length];
            if (bf.Read(bytes, 0, (int)bf.Length) != bf.Length)
                throw new Exception("couldn't read certificate");
            
            var cert = new X509Certificate2(bytes, pwd);
            return cert;
        }

        internal static void SavePfx(X509Certificate2 cert, string filepath)
        {
            byte[] bytes = cert.Export(X509ContentType.Pfx);
            
            using var bf = new FileStream(filepath, FileMode.Create);
            bf.Write(bytes, 0, bytes.Length);
        }
    }
}