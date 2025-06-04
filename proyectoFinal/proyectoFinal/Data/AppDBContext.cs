using proyectoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace proyectoFinal.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ComprasProveedor> comprasProveedores { get; set;}
        public DbSet<DetalleCompra> detalleCompras { get; set; }
        public DbSet<DetalleVenta> detalleVenta { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<MedioPago> MedioPagos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Rol> Rol {  get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<ComprasProveedor>(tb =>
            {
                tb.HasOne(tb => tb.proveedor).WithMany(tb => tb.comprasProveedors).HasForeignKey(tb => tb.idProveedor);
            });
            modelBuilder.Entity<ComprasProveedor>().ToTable("ComprasProveedores");
            modelBuilder.Entity<DetalleCompra>(tb =>
            {
                tb.HasOne(tb => tb.ComprasProveedor).WithMany(tb => tb.detalleCompras).HasForeignKey(tb => tb.idCompras);
                tb.HasOne(tb => tb.productos).WithMany(tb => tb.detalleCompras).HasForeignKey(tb =>tb.idProducto);
            });
            modelBuilder.Entity<DetalleCompra>().ToTable("DetalleCompra");
            modelBuilder.Entity<DetalleVenta>(tb =>
            {
                tb.HasOne(tb => tb.Producto).WithMany(tb => tb.detalleVentas).HasForeignKey(tb => tb.idProducto);
            });
            modelBuilder.Entity<DetalleVenta>().ToTable("DetalleVenta");

            modelBuilder.Entity<Inventario>(tb =>
            {
                tb.HasOne(tb => tb.producto).WithMany(tb => tb.inventario).HasForeignKey( tb => tb.idProducto);
            });
            modelBuilder.Entity<Inventario>().ToTable("Inventario");

            modelBuilder.Entity<MedioPago>().ToTable("MedioPago");

            modelBuilder.Entity<Producto>(tb =>
            {
                tb.HasOne( tb => tb.categoria).WithMany(tb => tb.Productos).HasForeignKey(tb => tb.idCategoria);
            });
            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedor");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Usuario>(tb =>
            {
                tb.HasOne(tb => tb.rol).WithMany(tb => tb.usuarios).HasForeignKey(tb => tb.idRol);
            });            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Venta>(tb =>
            {
                tb.HasOne(tb => tb.detalleVenta).WithMany(tb => tb.ventas).HasForeignKey(tb => tb.idDetalle);
                tb.HasOne(tb => tb.cliente).WithMany(tb => tb.ventas).HasForeignKey( tb => tb.idCliente);
                tb.HasOne(tb => tb.medioPago).WithMany(tb=>tb.ventas).HasForeignKey( tb=>tb.idMedioPago);
                tb.HasOne(tb => tb.usuario).WithMany(tb => tb.ventas).HasForeignKey( tb => tb.idUsuario);
            });
        }


    }
}
