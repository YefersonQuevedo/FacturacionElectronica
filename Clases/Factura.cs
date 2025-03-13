using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysqlTienda.Class.FacturacionElectronica.Clases
{
    public class Factura
    {
        public string id { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Moneda { get; set; }
        public Emisor Emisor { get; set; }
        public Adquirente Adquirente { get; set; }
        public List<ItemFactura> Items { get; set; }
        public decimal BaseAmount { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal TotalAmount { get; private set; }

        public Factura(string id,string numero, Emisor emisor, Adquirente adquirente, string moneda)
        {

            Numero = numero;
            FechaEmision = DateTime.Now;
            Moneda = moneda;
            Emisor = emisor;
            Adquirente = adquirente;
            Items = new List<ItemFactura>();
        }

        public void AgregarItem(ItemFactura item)
        {
            Items.Add(item);
            CalcularTotales();
        }

        private void CalcularTotales()
        {
            BaseAmount = 0;
            foreach (var item in Items)
            {
                BaseAmount += item.Subtotal;
            }
            TaxAmount = BaseAmount * 0.19m;  // Asumiendo 19% de IVA
            TotalAmount = BaseAmount + TaxAmount;
        }
    }
}