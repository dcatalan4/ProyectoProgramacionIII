# SuperBodega

Solución .NET para administración y ventas de supermercado utilizando PostgreSQL, Docker y Kafka.

---

# Proyectos

- `SuperBodega.Admin.Api` → API principal de administración.
- `SuperBodega.Ecommerce.Api` → API para catálogo, carrito y pedidos ecommerce.
- `SuperBodega.Domain` → Entidades del dominio.
- `SuperBodega.Infrastructure` → Persistencia, DbContext y configuración.
- `SuperBodega.Worker` → Procesos en segundo plano.
- `SuperBodega.Tests` → Pruebas unitarias.

---

# Requisitos

Antes de ejecutar el proyecto debes tener instalado:

- .NET 9 SDK
- EF Core CLI

Instalar EF Core CLI globalmente:

```bash
dotnet tool install --global dotnet-ef
```

Verificar instalación:

```bash
dotnet ef
```


# Levantar infraestructura Docker

Desde la raíz del proyecto ejecutar:

```bash
docker compose up -d
```

Esto levantará:

- PostgreSQL
- Kafka
- Zookeeper

Verificar contenedores:

```bash
docker ps
```

Deberías ver algo similar:

- `superbodega-postgres`
- `superbodega-kafka`
- `superbodega-zookeeper`

---

# Restaurar paquetes

```bash
dotnet restore SuperBodega.sln --configfile NuGet.Config
```

---

# Paquete requerido para migraciones EF Core

Si EF Core muestra este error:

```text
Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design
```

Instalar la versión correcta:

```bash
dotnet add SuperBodega.Ecommerce.Api package Microsoft.EntityFrameworkCore.Design --version 9.0.0
```

---

# Migraciones EF Core

## Crear migración

```bash
dotnet ef migrations add InitialCreate --project SuperBodega.Infrastructure --startup-project SuperBodega.Ecommerce.Api
```

## Aplicar migraciones

```bash
dotnet ef database update --project SuperBodega.Infrastructure --startup-project SuperBodega.Ecommerce.Api
```

---

# Configuración PostgreSQL

Cadena de conexión local:

```text
Host=localhost;Port=5432;Database=superbodega;Username=superbodega;Password=superbodega
```

# Ejecutar APIs

## Admin API

```bash
dotnet run --project SuperBodega.Admin.Api
```

## Ecommerce API

```bash
dotnet run --project SuperBodega.Ecommerce.Api
```

Swagger disponible en:

```text
http://localhost:5180/swagger
```

o el puerto que indique la consola.

---

# Kafka

La API Ecommerce utiliza Kafka para procesar pedidos asincrónicos.

Topic utilizado:

```text
pedidos-topic
```

Crear el topic a utilizar:

```bash
docker exec -it superbodega-kafka kafka-topics --create --topic pedidos-topic --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1
```

Consumer Group:

```text
pedidos-group
```

Al iniciar correctamente debe aparecer:

```text
Kafka Consumer iniciado.
```

---

# Flujo Ecommerce

## Crear carrito

```http
POST /api/carrito
```

## Agregar productos

```http
POST /api/carrito/items
```

## Procesar pedido síncrono

```http
POST /api/pedidos/sincrono
```

Procesa inmediatamente:

- valida stock
- crea venta
- reduce inventario
- genera notificación

todo dentro de una transacción.

## Procesar pedido asíncrono

```http
POST /api/pedidos/asincrono
```

Envía el carrito a Kafka para procesamiento en segundo plano.

---

# Verificar datos en PostgreSQL

Entrar al contenedor:

```bash
docker exec -it superbodega-postgres psql -U superbodega -d superbodega
```

Consultar ventas:

```sql
SELECT * FROM ventas;
```

Consultar carritos:

```sql
SELECT * FROM carritos;
```

Salir:

```sql
\q
```

---

# Docker Build Manual (Opcional)

## Admin API

```bash
docker build -t superbodega-admin .
```

Ejecutar:

```bash
docker run -p 8080:8080 superbodega-admin
```

## Ecommerce API

```bash
docker build -t superbodega-ecommerce --build-arg PROJECT=SuperBodega.Ecommerce.Api .
```


# Endpoints Administrativos

```http
GET|POST /api/productos
GET|PUT|DELETE /api/productos/{id}

GET|POST /api/proveedores
GET|PUT|DELETE /api/proveedores/{id}

GET|POST /api/clientes
GET|PUT|DELETE /api/clientes/{id}

GET|POST /api/compras
GET|PUT|DELETE /api/compras/{id}

GET|POST /api/ventas
GET|PUT|DELETE /api/ventas/{id}

PATCH /api/ventas/{id}/estado

GET /api/reportes/periodo?desde=2026-01-01&hasta=2026-12-31
GET /api/reportes/producto/{productoId}?desde=2026-01-01&hasta=2026-12-31
GET /api/reportes/cliente/{clienteId}?desde=2026-01-01&hasta=2026-12-31
GET /api/reportes/proveedor/{proveedorId}?desde=2026-01-01&hasta=2026-12-31
```

---

# Endpoints Ecommerce

```http
GET /api/catalogo?page=1&pageSize=10

GET /api/catalogo?categoriaId={categoriaId}

POST /api/carrito

POST /api/carrito/items

POST /api/pedidos/sincrono

POST /api/pedidos/asincrono
```

---

# Notas

- Kafka debe estar levantado antes de ejecutar Ecommerce API.
- Los pedidos asincrónicos utilizan Kafka.
- El stock se actualiza automáticamente al confirmar ventas.
