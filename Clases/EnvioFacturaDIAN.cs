using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;
using System.Threading.Tasks;
using DianResponse; // Importar el namespace generado
using UploadDocumentResponse;
using XmlParamsResponseTrackId;

namespace MysqlTienda.Class.FacturacionElectronica
{
    public class EnvioFacturaDIAN
    {
        public static async Task EnviarFacturaAsync(string xmlFirmadoPath, string softwareID, string passwordSoftware)
        {
            try
            {
                Console.WriteLine("📌 [1/6] Comenzando compresión de la factura...");

                byte[] zipBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        var entry = zip.CreateEntry("factura.xml");
                        using (var entryStream = entry.Open())
                        using (FileStream signedXmlFile = new FileStream(xmlFirmadoPath, FileMode.Open))
                        {
                            signedXmlFile.CopyTo(entryStream);
                        }
                    }
                    zipBytes = ms.ToArray();
                }

                Console.WriteLine($"✅ [2/6] XML firmado comprimido correctamente en memoria. Tamaño ZIP: {zipBytes.Length} bytes");

                string zipFilePath = "factura_prueba.zip";
                File.WriteAllBytes(zipFilePath, zipBytes);
                Console.WriteLine($"✅ [3/6] Archivo ZIP guardado en: {zipFilePath}");

                Console.WriteLine("📌 [4/6] Configurando conexión con el servicio web de la DIAN...");

                // Configurar la conexión con el servicio de la DIAN
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    Security = { Transport = { ClientCredentialType = HttpClientCredentialType.None } },
                    MaxReceivedMessageSize = 20000000, // Ajuste del tamaño de mensaje
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                    {
                        MaxStringContentLength = 20000000
                    },
                    MessageEncoding = WSMessageEncoding.Text // ❗❗ Cambiar a TEXT para evitar MTOM
                };


                EndpointAddress endpoint = new EndpointAddress("https://vpfe-hab.dian.gov.co/WcfDianCustomerServices.svc");

                using (var dianClient = new WcfDianCustomerServicesClient(binding, endpoint))
                {
                    Console.WriteLine("📌 [5/6] Configurando credenciales de autenticación con la DIAN...");
                    dianClient.ClientCredentials.UserName.UserName = softwareID;
                    dianClient.ClientCredentials.UserName.Password = passwordSoftware;

                    Console.WriteLine("📌 [6/6] Enviando factura electrónica a la DIAN...");

                    // Llamar al método generado en Reference.cs
                    DianResponse.DianResponse respuesta = await dianClient.SendBillSyncAsync("factura_prueba.zip", zipBytes);

                    Console.WriteLine("✅ Respuesta de la DIAN recibida:");
                    Console.WriteLine($"📌 Código de estado: {respuesta.StatusCode}");
                    Console.WriteLine($"📌 Descripción: {respuesta.StatusDescription}");

                    if (respuesta.IsValid)
                    {
                        Console.WriteLine("✅ La factura fue validada correctamente por la DIAN.");
                    }
                    else
                    {
                        Console.WriteLine("❌ La DIAN devolvió errores:");
                        foreach (string error in respuesta.ErrorMessage)
                        {
                            Console.WriteLine($"   - {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                Console.WriteLine($"📌 StackTrace: {ex.StackTrace}");
            }
        }
    }
}
