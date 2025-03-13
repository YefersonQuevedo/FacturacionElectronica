using FirmaXadesNet;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using FirmaXadesNet.Crypto;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography;

namespace MysqlTienda.Class.FacturacionElectronica
{
    public class XmlSigner
    {
        public static void FirmarXML(string xmlPath, string signedXmlPath, string pfxPath, string password)
        {
            try
            {
                Console.WriteLine("📌 Verificando existencia del certificado digital...");
                if (!File.Exists(pfxPath))
                {
                    Console.WriteLine($"❌ Error: No se encontró el certificado en {pfxPath}");
                    return;
                }

                Console.WriteLine("📌 Cargando certificado digital...");
                X509Certificate2 cert = new X509Certificate2(pfxPath, password);

                Console.WriteLine("📌 Cargando XML a firmar...");
                XmlDocument xmlDoc = new XmlDocument
                {
                    PreserveWhitespace = true
                };

                try
                {
                    xmlDoc.Load(xmlPath);
                }
                catch (XmlException ex)
                {
                    Console.WriteLine($"❌ Error al cargar el XML: {ex.Message}");
                    return;
                }

                // 📌 Convertir XmlDocument a MemoryStream para FirmaXadesNet
                MemoryStream xmlStream = new MemoryStream();
                xmlDoc.Save(xmlStream);
                xmlStream.Position = 0;

                Console.WriteLine("📌 Configurando la firma XAdES-EPES...");
                SignaturePolicyInfo policyInfo = new SignaturePolicyInfo
                {
                    PolicyIdentifier = "https://facturaelectronica.dian.gov.co/politicadefirma/v2/politicadefirmav2.pdf",
                    PolicyHash = "ERhQox49sUPH3hYDWu5ieFnb53E=", // Hash correcto de la DIAN
                    PolicyDigestAlgorithm = DigestMethod.SHA512 // ✅ Usando SHA-512
                };

                SignatureParameters parametros = new SignatureParameters
                {
                    Signer = new Signer(cert),
                    SignaturePackaging = SignaturePackaging.ENVELOPED,
                    DigestMethod = DigestMethod.SHA512, // ✅ SHA-512 para cumplir con la DIAN
                    SignaturePolicyInfo = policyInfo
                };

                Console.WriteLine("📌 Creando la firma...");
                XadesService xadesService = new XadesService();
                var signedXml = xadesService.Sign(xmlStream, parametros);

                // 📌 Convertir el XML firmado a un XmlDocument
                XmlDocument signedXmlDoc = new XmlDocument();
                signedXmlDoc.PreserveWhitespace = true;

                // 📌 Guardar la firma en un MemoryStream y cargarla en XmlDocument
                MemoryStream signedStream = new MemoryStream();
                signedXml.Save(signedStream);
                signedStream.Position = 0;
                signedXmlDoc.Load(signedStream);

                Console.WriteLine("📌 Buscando nodo UBLExtensions para insertar la firma...");

                // 📌 NamespaceManager para encontrar UBLExtensions
                XmlNamespaceManager ns = new XmlNamespaceManager(xmlDoc.NameTable);
                ns.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                XmlNode extensionContentNode = xmlDoc.SelectSingleNode("//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent", ns);

                if (extensionContentNode != null)
                {
                    // 📌 Importar la firma en el XML original
                    XmlNode importedNode = xmlDoc.ImportNode(signedXmlDoc.DocumentElement, true);
                    extensionContentNode.AppendChild(importedNode);
                    xmlDoc.Save(signedXmlPath);

                    Console.WriteLine($"✅ Firma insertada correctamente en UBLExtensions y guardada en: {signedXmlPath}");
                }
                else
                {
                    Console.WriteLine("⚠️ No se encontró el nodo correcto para insertar la firma en UBLExtensions.");
                }

            }
            catch (CryptographicException ce)
            {
                Console.WriteLine($"❌ Error de certificado: {ce.Message}");
            }
            catch (XmlException xe)
            {
                Console.WriteLine($"❌ Error en el XML: {xe.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
            }
        }
    }
}
