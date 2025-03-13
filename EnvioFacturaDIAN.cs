using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;

namespace MysqlTienda.Class.FacturacionElectronica
{
    public class EnvioFacturaDIAN
    {
        public static void EnviarFactura(string xmlFirmadoPath, string softwareID, string passwordSoftware)
        {
            try
            {
                Console.WriteLine("📌 Comenzando compresión de la factura...");

                // 📌 Comprimir el XML firmado en un archivo ZIP en memoria
                byte[] zipBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        var entry = zip.CreateEntry("factura.xml"); // Nombre del archivo dentro del ZIP
                        using (var entryStream = entry.Open())
                        using (FileStream signedXmlFile = new FileStream(xmlFirmadoPath, FileMode.Open))
                        {
                            signedXmlFile.CopyTo(entryStream);
                        }
                    }
                    zipBytes = ms.ToArray();
                }

                Console.WriteLine("✅ XML firmado comprimido correctamente en memoria.");

                // 📌 Configurar el cliente WCF para el servicio DIAN
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
                binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                EndpointAddress endpoint = new EndpointAddress("https://vpfe-hab.dian.gov.co/WcfDianCustomerServices.svc");

                // 📌 Crear el cliente WCF generado desde el WSDL de la DIAN
                var dianClient = new WcfDianCustomerServicesClient(binding, endpoint);

                // 📌 Configurar credenciales WS-Security
                dianClient.ClientCredentials.UserName.UserName = softwareID;      // Software ID provisto por DIAN
                dianClient.ClientCredentials.UserName.Password = passwordSoftware; // Contraseña del software en DIAN

                Console.WriteLine("📌 Enviando factura electrónica a la DIAN...");

                // 📌 Llamar al método SendBillSync del servicio SOAP
                var respuesta = dianClient.SendBillSync("factura.zip", zipBytes);

                // 📌 Mostrar la respuesta de la DIAN
                Console.WriteLine($"✅ Respuesta DIAN: {respuesta.StatusCode} - {respuesta.StatusDescription}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar la factura a la DIAN: {ex.Message}");
            }
        }
    }
}
