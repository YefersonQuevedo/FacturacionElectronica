namespace MysqlTienda.Class.FacturacionElectronica.Clases
{
    public class ItemFactura
    {
        public int Id { get; set; } // ID del producto o ítem
        public string Descripcion { get; set; } // Nombre del producto
        public int Cantidad { get; set; } // Cantidad comprada
        public decimal Precio { get; set; } // Precio unitario
        public decimal Subtotal => Cantidad * Precio; // Cálculo automático del subtotal

        public ItemFactura(int id, string descripcion, int cantidad, decimal precio)
        {
            Id = id;
            Descripcion = descripcion;
            Cantidad = cantidad;
            Precio = precio;
        }
    }
}
