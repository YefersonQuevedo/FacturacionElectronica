using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ServiceModel.Channels;
using MysqlTienda.DianService;
using System.Windows.Forms;
using DianResponse;


namespace MysqlTienda.Class.FacturacionElectronica
{
    public class FacturacionDian
    {
        private const string TEST_URL = "https://vpfe-hab.dian.gov.co/WcfDianCustomerServices.svc?wsdl";
        private const string PRODUCTION_URL = "https://vpfe.dian.gov.co/WcfDianCustomerServices.svc";

        private readonly string _certificatePath;
        private readonly string _certificatePassword;
        private readonly string _softwareId;
        private readonly string _softwarePin;
        private readonly string _nit;

        public FacturacionDian(string certificatePath, string certificatePassword, string softwareId, string softwarePin, string nit)
        {
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;
            _softwareId = softwareId;
            _softwarePin = softwarePin;
            _nit = nit;
        }

        public XDocument GenerarFacturaUBL21(string invoiceNumber, string issueDate, string invoiceType, string customerNit, string totalAmount, string payableAmount)
        {
            try
            {
                // Validaciones antes de generar el XML
                if (string.IsNullOrEmpty(invoiceNumber) || string.IsNullOrEmpty(issueDate) ||
                    string.IsNullOrEmpty(invoiceType) || string.IsNullOrEmpty(customerNit) ||
                    string.IsNullOrEmpty(totalAmount) || string.IsNullOrEmpty(payableAmount))
                {
                    throw new Exception("Uno o más parámetros están vacíos al generar la factura.");
                }

                XNamespace invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";

                var invoice = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    new XElement(invoiceNs + "Invoice",
                        new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                        new XAttribute(XNamespace.Xmlns + "cac", cac),
                        new XAttribute(XNamespace.Xmlns + "ds", ds),
                        new XElement(cbc + "CustomizationID", "DIAN 2.1"),
                        new XElement(cbc + "ProfileID", "DIAN 2.1"),
                        new XElement(cbc + "ID", invoiceNumber),
                        new XElement(cbc + "IssueDate", issueDate),
                        new XElement(cbc + "InvoiceTypeCode", invoiceType),
                        new XElement(cac + "AccountingSupplierParty",
                            new XElement(cbc + "AdditionalAccountID", "1"),
                            new XElement(cbc + "CompanyID", _nit)
                        ),
                        new XElement(cac + "AccountingCustomerParty",
                            new XElement(cbc + "AdditionalAccountID", "2"),
                            new XElement(cbc + "CompanyID", customerNit)
                        ),
                        new XElement(cac + "LegalMonetaryTotal",
                            new XElement(cbc + "LineExtensionAmount", totalAmount),
                            new XElement(cbc + "PayableAmount", payableAmount)
                        )
                    )
                );

                Log("Factura generada correctamente.");
                return invoice;
            }
            catch (Exception ex)
            {
                Log("Error generando factura: " + ex.Message);
                throw;
            }
        }

        public string FirmarFacturaUBL21(XDocument invoice)
        {
            try
            {
                if (!File.Exists(_certificatePath))
                {
                    throw new Exception("El certificado no existe en la ruta especificada.");
                }

                using (X509Certificate2 cert = new X509Certificate2(_certificatePath, _certificatePassword))
                {
                    var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                    using (var reader = invoice.CreateReader())
                    {
                        xmlDoc.Load(reader);
                    }

                    SignedXml signedXml = new SignedXml(xmlDoc) { SigningKey = cert.GetRSAPrivateKey() };
                    Reference reference = new Reference { Uri = "", DigestMethod = SignedXml.XmlDsigSHA256Url };
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                    signedXml.AddReference(reference);
                    signedXml.KeyInfo = new KeyInfo();
                    signedXml.KeyInfo.AddClause(new KeyInfoX509Data(cert));
                    signedXml.ComputeSignature();
                    xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(signedXml.GetXml(), true));

                    string facturaFirmada = xmlDoc.OuterXml;
                    File.WriteAllText("factura_firmada.xml", facturaFirmada);  // Guarda el XML firmado para revisión
                    Log("Factura firmada correctamente.");
                    MessageBox.Show("firme");
                    return facturaFirmada;
                }
            }
            catch (Exception ex)
            {
                Log("Error firmando factura: " + ex.Message);
                throw;
            }
        }

        public DianResponse.DianResponse EnviarFacturaUBL21(string facturaFirmada)

        {
            try
            {
                var binding = new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8)
                    { ReaderQuotas = new XmlDictionaryReaderQuotas { MaxStringContentLength = 2000000 } },
                    new HttpsTransportBindingElement
                    {
                        MaxReceivedMessageSize = 2000000,
                        MaxBufferSize = 2000000,
                        MaxBufferPoolSize = 2000000
                    }
                )
                {
                    SendTimeout = TimeSpan.FromMinutes(3),
                    ReceiveTimeout = TimeSpan.FromMinutes(3),
                    OpenTimeout = TimeSpan.FromMinutes(2),
                    CloseTimeout = TimeSpan.FromMinutes(2)
                };

                var endpoint = new EndpointAddress(TEST_URL);

                using (var factory = new ChannelFactory<IDianCustomerServices>(binding, endpoint))
                {
                    var cert = new X509Certificate2(_certificatePath, _certificatePassword);
                    factory.Credentials.ClientCertificate.Certificate = cert;

                    var client = factory.CreateChannel();

                    Log("Enviando factura a la DIAN...");
                    var response = client.SendBillSync($"Invoice_{DateTime.Now:yyyyMMddHHmmss}.xml", Encoding.UTF8.GetBytes(facturaFirmada));

                    Log("Respuesta de la DIAN recibida: " + response.StatusDescription);
                    return response;
                }
            }
            catch (TimeoutException ex)
            {
                Log("Error de Timeout: " + ex.Message);
                return new DianResponse.DianResponse { StatusDescription = "Tiempo de espera agotado", StatusCode = "TIMEOUT" };
            }
            catch (FaultException ex)
            {
                Log("Error SOAP de la DIAN: " + ex.Message);
                return new DianResponse.DianResponse { StatusDescription = "Error SOAP: " + ex.Message, StatusCode = "SOAP_ERROR" };
            }
            catch (Exception ex)
            {
                Log("Error general: " + ex.Message);
                return new DianResponse.DianResponse { StatusDescription = $"Error de envío: {ex.Message}", StatusCode = "ERROR" };
                

            }
        }

        private void Log(string message)
        {
            string logPath = "log_facturacion.txt";
            File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
        }
        [ServiceContract]
        public interface IDianCustomerServices
        {
            [OperationContract]
            DianResponse.DianResponse SendBillSync(string fileName, byte[] file);
        }

    }
}
