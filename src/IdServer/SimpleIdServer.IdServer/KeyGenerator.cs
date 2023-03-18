﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.IdentityModel.Tokens;
using SimpleIdServer.IdServer.Domains;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace SimpleIdServer.IdServer
{
    public static class KeyGenerator
    {
        public static SerializedFileKey GenerateSigningCredentials(Realm realm)
        {
            var rsa = RSA.Create();
            var key = new RsaSecurityKey(rsa)
            {
                KeyId = "keyid"
            };
            var pem = PemConverter.ConvertFromSecurityKey(key);
            var result = new SerializedFileKey
            {
                Id = Guid.NewGuid().ToString(),
                Alg = SecurityAlgorithms.RsaSha256,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow,
                KeyId = key.KeyId,
                PrivateKeyPem = pem.PrivateKey,
                PublicKeyPem = pem.PublicKey,
                Usage = Constants.JWKUsages.Sig,
                IsSymmetric = false
            };
            result.Realms.Add(realm);
            return result;
        }

        public static X509Certificate2 GenerateSelfSignedCertificate()
        {
            var subjectName = "Self-Signed-Cert-Example";
            var rsa = RSA.Create();
            var certRequest = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            var generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10));
            return generatedCert;
        }

        public static X509Certificate2 GenerateCertificateAuthority(string subjectName, string password, int nbValidDays = 365)
        {
            using (RSA parent = RSA.Create(2048))
            {
                CertificateRequest parentReq = new CertificateRequest(
                    subjectName,
                    parent,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
                parentReq.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                parentReq.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));
                using (X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(nbValidDays)))
                {
                    return new X509Certificate2(parentCert.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                }
            }
        }

        public static PemResult GenerateClientCertificate(X509Certificate2 ca, string subjectName, int nbValidDays, CancellationToken cancellationToken)
        {
            using (var rsa = RSA.Create(2048))
            {
                CertificateRequest req = new CertificateRequest(
                    subjectName,
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                req.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));

                req.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment,
                        false));

                req.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                using (X509Certificate2 cert = req.Create(
                    ca,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(nbValidDays),
                    new byte[] { 1, 2, 3, 4 }))
                {
                    var privatePem = new string(PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));
                    return new PemResult(cert.ExportCertificatePem(), privatePem);
                }
            }
        }
    }
}
