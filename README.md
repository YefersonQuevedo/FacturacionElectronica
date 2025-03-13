# Descripción del Proyecto Facturación Electrónica DIAN

Este proyecto proporciona una solución para la generación, firma y envío de facturas electrónicas a la DIAN (Dirección de Impuestos y Aduanas Nacionales) de Colombia, cumpliendo con las especificaciones técnicas y normativas vigentes.

## Estructura del Proyecto

El proyecto se estructura en las siguientes clases y archivos principales:

- **`CufeGenerator.cs`**: Genera el Código Único de Facturación Electrónica (CUFE) a partir de los datos de la factura y la clave técnica proporcionada por la DIAN. Utiliza el algoritmo SHA-384 para garantizar la integridad y unicidad del código.

- **`EnvioFacturaDIAN.cs`**: Contiene la lógica para comprimir el XML firmado en formato ZIP y enviarlo a la DIAN a través de un servicio web (WCF). Utiliza credenciales de autenticación (Software ID y contraseña) proporcionadas por la DIAN.

- **`FacturacionDian.cs`**: Implementa la lógica principal para la generación, firma y envío de facturas electrónicas a la DIAN. Utiliza un certificado digital para firmar el XML de la factura y se comunica con los servicios web de la DIAN para el envío.

- **`FacturaXMLGenerator.cs`**: Genera el XML de la factura electrónica en formato UBL 2.1, cumpliendo con las especificaciones de la DIAN. Utiliza los datos de la factura (emisor, adquirente, ítems, totales, etc.) para construir el XML.

- **`Clases/`**: Contiene las clases que representan las entidades del modelo de facturación:
    - **`Adquirente.cs`**: Representa al adquirente (cliente) de la factura, con propiedades como NIT y nombre.
    - **`Emisor.cs`**: Representa al emisor (vendedor) de la factura, con propiedades como NIT, nombre, dirección, ciudad y departamento.
    - **`EnvioFacturaDIAN.cs`**: (Duplicado, parece haber un error aquí)
    - **`Factura.cs`**: Representa la factura electrónica, con propiedades como número, fecha de emisión, moneda, emisor, adquirente, ítems, totales, etc.
    - **`ItemFactura.cs`**: Representa un ítem de la factura, con propiedades como ID, descripción, cantidad, precio y subtotal.
    - **`Reference.cs`**: Define las clases de respuesta de los servicios de la DIAN, como `DianResponse`, `ExchangeEmailResponse`, `UploadDocumentResponse`, etc.
    - **`XmlSigner.cs`**: Implementa la lógica para firmar digitalmente el XML de la factura utilizando un certificado digital (PFX) y la librería FirmaXadesNet.

## Flujo de Facturación

El proceso de facturación electrónica sigue los siguientes pasos:

1.  **Generación del XML**: Se utiliza la clase `FacturaXMLGenerator` para generar el XML de la factura en formato UBL 2.1 a partir de los datos de la factura.
2.  **Firma del XML**: Se utiliza la clase `XmlSigner` para firmar digitalmente el XML de la factura utilizando un certificado digital (PFX) y la librería FirmaXadesNet.
3.  **Generación del CUFE**: Se utiliza la clase `CufeGenerator` para generar el CUFE a partir de los datos de la factura y la clave técnica proporcionada por la DIAN.
4.  **Envío a la DIAN**: Se utiliza la clase `EnvioFacturaDIAN` para comprimir el XML firmado en formato ZIP y enviarlo a la DIAN a través de un servicio web (WCF).

## Consideraciones Técnicas

-   El proyecto utiliza la librería FirmaXadesNet para la firma digital del XML.
-   El proyecto utiliza servicios web (WCF) para la comunicación con la DIAN.
-   El proyecto requiere un certificado digital (PFX) válido para la firma del XML.
-   El proyecto requiere credenciales de autenticación (Software ID y contraseña) proporcionadas por la DIAN.

## Dependencias

-   FirmaXadesNet
-   System.ServiceModel
-   System.IO.Compression
-   System.Security.Cryptography.X509Certificates

## Notas

Este proyecto fue desarrollado con C# .NET Framework 4.8.

Este README proporciona una descripción general del proyecto. Para obtener información más detallada, consulte el código fuente y la documentación de la DIAN.
