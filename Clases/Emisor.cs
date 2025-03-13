using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysqlTienda.Class.FacturacionElectronica.Clases
{
    public class Emisor
    {
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Departamento { get; set; }

        public Emisor(string nit, string nombre, string direccion, string ciudad, string departamento)
        {
            Nit = nit;
            Nombre = nombre;
            Direccion = direccion;
            Ciudad = ciudad;
            Departamento = departamento;
        }
    }
}