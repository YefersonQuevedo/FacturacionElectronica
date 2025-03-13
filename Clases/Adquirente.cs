using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysqlTienda.Class.FacturacionElectronica.Clases
{
    public class Adquirente
    {
        public string Nit { get; set; }
        public string Nombre { get; set; }

        public Adquirente(string nit, string nombre)
        {
            Nit = nit;
            Nombre = nombre;
        }
    }
}