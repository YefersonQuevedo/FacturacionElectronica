using System;
using System.Security.Cryptography;
using System.Text;

namespace MysqlTienda.Class.FacturacionElectronica
{
    public static class CufeGenerator
    {
        /// <summary>
        /// Genera el CUFE (Código Único de Facturación Electrónica) según la normativa de la DIAN.
        /// </summary>
        /// <param name="nitEmisor">NIT del emisor</param>
        /// <param name="numeroFactura">Número de la factura (incluyendo prefijo)</param>
        /// <param name="fechaEmision">Fecha de emisión en formato yyyy-MM-dd</param>
        /// <param name="horaEmision">Hora de emisión en formato HH:mm:ss</param>
        /// <param name="baseImponible">Valor total bruto de la factura</param>
        /// <param name="iva">Valor del IVA</param>
        /// <param name="valorTotal">Valor total de la factura</param>
        /// <param name="numeroResolucion">Número de resolución DIAN</param>
        /// <param name="prefijo">Prefijo de la numeración DIAN</param>
        /// <param name="claveTecnica">Clave técnica proporcionada por la DIAN</param>
        /// <returns>CUFE en formato SHA-384</returns>
        public static string GenerarCUFE(string nitEmisor, string numeroFactura, DateTime fechaEmision, string horaEmision,
                                         decimal baseImponible, decimal iva, decimal valorTotal,
                                         string numeroResolucion, string prefijo, string claveTecnica)
        {
            // 📌 Construcción de la cadena según la DIAN
            string cufeCadena = $"{nitEmisor}" +
                                $"{numeroFactura}" +
                                $"{fechaEmision:yyyy-MM-dd}" +
                                $"{horaEmision}" +
                                $"{baseImponible:0.00}" +
                                $"{iva:0.00}" +
                                $"{valorTotal:0.00}" +
                                $"{numeroResolucion}" +
                                $"{prefijo}" +
                                $"{claveTecnica}";

            // 📌 Convertir la cadena a SHA-384
            using (SHA384 sha384 = SHA384.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(cufeCadena);
                byte[] hashBytes = sha384.ComputeHash(bytes);

                // 📌 Convertir el hash en cadena hexadecimal (en mayúsculas)
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().ToUpper(); // La DIAN exige que el CUFE esté en mayúsculas
            }
        }
    }
}
