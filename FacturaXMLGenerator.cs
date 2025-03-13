using MysqlTienda.Class.FacturacionElectronica.Clases;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MysqlTienda.Class.FacturacionElectronica
{
    public class FacturaXMLGenerator
    {
        // Namespaces UBL requeridos
        private static readonly XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";

        public static XDocument GenerarXML(Factura factura)
        {
            // 📌 Datos proporcionados por la DIAN
            string prefijo = "SETP";
            string numeroFactura = factura.Numero;
            string claveTecnicaDIAN = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c";
            string softwareID = "9348759f-a0e7-4ee3-a89a-ae85a2dec7b0";
            string numeroResolucion = "18760000001"; // 📌 Número de resolución DIAN

            // 📌 Generar CUFE usando la clave técnica de la DIAN
            string cufe = CufeGenerator.GenerarCUFE(
                factura.Emisor.Nit,
                prefijo + numeroFactura,
                factura.FechaEmision,
                factura.FechaEmision.ToString("HH:mm:ss"),
                factura.BaseAmount,
                factura.TaxAmount,
                factura.TotalAmount,
                numeroResolucion,
                prefijo,
                claveTecnicaDIAN
            );

            XDocument facturaXml = new XDocument(
                new XElement(ns + "Invoice",
                    new XAttribute(XNamespace.Xmlns + "cac", cac),
                    new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                    new XAttribute(XNamespace.Xmlns + "ext", ext),
                    new XAttribute(XNamespace.Xmlns + "ds", ds),
                    new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),

                    // 📌 Extensiones UBL (Se agregan dos UBLExtensions)
                    new XElement(ext + "UBLExtensions",
                        // 📌 Primera extensión vacía (Requerida por la DIAN)
                        new XElement(ext + "UBLExtension",
                            new XElement(ext + "ExtensionContent")
                        ),
                        // 📌 Segunda extensión para la firma electrónica
                        new XElement(ext + "UBLExtension",
                            new XElement(ext + "ExtensionContent")
                        )
                    ),

                    // 📌 Identificación de la factura
                    new XElement(cbc + "ID", prefijo + numeroFactura),
                    new XElement(cbc + "IssueDate", factura.FechaEmision.ToString("yyyy-MM-dd")),
                    new XElement(cbc + "IssueTime", factura.FechaEmision.ToString("HH:mm:ss")),
                    new XElement(cbc + "InvoiceTypeCode", "01"), // 01 = Factura de venta
                    new XElement(cbc + "DocumentCurrencyCode", factura.Moneda),

                    // 📌 UUID (CUFE)
                    new XElement(cbc + "UUID",
                        new XAttribute("schemeName", "CUFE-SHA512"), // SHA-512
                        new XAttribute("schemeID", "2"), // 2 = pruebas, 1 = producción
                        cufe
                    ),
                    new XElement(cbc + "CustomizationID", "10"), // Indica el tipo de factura electrónica (DIAN usa "10")
                    new XElement(cbc + "ProfileExecutionID", "2"), // 2 = Pruebas, 1 = Producción

                    // 📌 Información del software registrado en la DIAN
                    new XElement(cac + "SoftwareProvider",
                        new XElement(cac + "SoftwareID", softwareID)
                    ),

                    // 📌 Datos del Emisor
                    new XElement(cac + "AccountingSupplierParty",
                        new XElement(cac + "Party",
                            new XElement(cac + "PartyIdentification",
                                new XElement(cbc + "ID", new XAttribute("schemeID", "31"), factura.Emisor.Nit)
                            ),
                            new XElement(cac + "PartyName",
                                new XElement(cbc + "Name", factura.Emisor.Nombre)
                            ),
                            new XElement(cac + "PhysicalLocation",
                                new XElement(cac + "Address",
                                    new XElement(cbc + "CityName", factura.Emisor.Ciudad),
                                    new XElement(cbc + "CountrySubentity", factura.Emisor.Departamento),
                                    new XElement(cac + "Country",
                                        new XElement(cbc + "IdentificationCode", "CO")
                                    )
                                )
                            ),
                            new XElement(cac + "PartyLegalEntity",
                                new XElement(cbc + "RegistrationName", factura.Emisor.Nombre),
                                new XElement(cbc + "CompanyID", factura.Emisor.Nit)
                            )
                        )
                    ),

                    // 📌 Datos del Adquirente
                    new XElement(cac + "AccountingCustomerParty",
                        new XElement(cac + "Party",
                            new XElement(cac + "PartyIdentification",
                                new XElement(cbc + "ID", new XAttribute("schemeID", "31"), factura.Adquirente.Nit)
                            ),
                            new XElement(cac + "PartyLegalEntity",
                                new XElement(cbc + "RegistrationName", factura.Adquirente.Nombre)
                            )
                        )
                    ),

                    // 📌 Totales de impuestos
                    new XElement(cac + "TaxTotal",
                        new XElement(cbc + "TaxAmount", new XAttribute("currencyID", factura.Moneda),
                            factura.TaxAmount.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount", new XAttribute("currencyID", factura.Moneda),
                                factura.BaseAmount.ToString("F2", CultureInfo.InvariantCulture)),
                            new XElement(cbc + "TaxAmount", new XAttribute("currencyID", factura.Moneda),
                                factura.TaxAmount.ToString("F2", CultureInfo.InvariantCulture))
                        )
                    ),

                    // 📌 Totales monetarios
                    new XElement(cac + "LegalMonetaryTotal",
                        new XElement(cbc + "LineExtensionAmount", new XAttribute("currencyID", factura.Moneda),
                            factura.BaseAmount.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "TaxExclusiveAmount", new XAttribute("currencyID", factura.Moneda),
                            factura.BaseAmount.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "TaxInclusiveAmount", new XAttribute("currencyID", factura.Moneda),
                            factura.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "PayableAmount", new XAttribute("currencyID", factura.Moneda),
                            factura.TotalAmount.ToString("F2", CultureInfo.InvariantCulture))
                    )
                )
            );

            return facturaXml;
        }
    }
}
