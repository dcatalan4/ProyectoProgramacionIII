# SuperBodega

Solucion .NET para administrar y vender productos de supermercado.

## Proyectos

- `SuperBodega.Admin.Api`: API principal de administracion.
- `SuperBodega.Ecommerce.Api`: API para catalogo y pedidos ecommerce.
- `SuperBodega.Domain`: entidades del dominio.
- `SuperBodega.Infrastructure`: repositorios y registro de dependencias.
- `SuperBodega.Worker`: proceso en segundo plano para tareas operativas.
- `SuperBodega.Tests`: pruebas unitarias.

## Ejecutar

```powershell
docker compose up -d postgres
dotnet restore SuperBodega.sln --configfile NuGet.Config
dotnet run --project SuperBodega.Admin.Api
dotnet run --project SuperBodega.Ecommerce.Api
```

Swagger queda disponible en `/swagger` cuando la aplicacion corre en ambiente `Development`.

## Endpoints administrativos

- `GET|POST /api/productos`
- `GET|PUT|DELETE /api/productos/{id}`
- `GET|POST /api/proveedores`
- `GET|PUT|DELETE /api/proveedores/{id}`
- `GET|POST /api/clientes`
- `GET|PUT|DELETE /api/clientes/{id}`
- `GET|POST /api/compras`
- `GET|PUT|DELETE /api/compras/{id}`
- `GET|POST /api/ventas`
- `GET|PUT|DELETE /api/ventas/{id}`
- `PATCH /api/ventas/{id}/estado`
- `GET /api/reportes/periodo?desde=2026-01-01&hasta=2026-12-31`
- `GET /api/reportes/producto/{productoId}?desde=2026-01-01&hasta=2026-12-31`
- `GET /api/reportes/cliente/{clienteId}?desde=2026-01-01&hasta=2026-12-31`
- `GET /api/reportes/proveedor/{proveedorId}?desde=2026-01-01&hasta=2026-12-31`

## Endpoints E-Commerce

- `GET /api/catalogo?page=1&pageSize=10`
- `GET /api/catalogo?categoriaId={categoriaId}`
- `POST /api/carrito`
- `POST /api/carrito/items`
- `POST /api/pedidos/sincrono`
- `POST /api/pedidos/asincrono`

`POST /api/pedidos/sincrono` procesa el carrito inmediatamente, valida stock, crea la venta y reduce inventario en una transaccion.
`POST /api/pedidos/asincrono` encola la solicitud y un worker interno la procesa en segundo plano.

La cadena de conexion local apunta a PostgreSQL en Docker:

```text
Host=localhost;Port=5432;Database=superbodega;Username=superbodega;Password=superbodega
```

## Migraciones EF Core

```powershell
dotnet ef migrations add InitialCreate --project SuperBodega.Infrastructure --startup-project SuperBodega.Admin.Api
dotnet ef database update --project SuperBodega.Infrastructure --startup-project SuperBodega.Admin.Api
```

## Docker

```powershell
docker build -t superbodega-admin .
docker build -t superbodega-ecommerce --build-arg PROJECT=SuperBodega.Ecommerce.Api .
docker run -p 8080:8080 superbodega-admin
```
