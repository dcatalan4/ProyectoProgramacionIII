const API_ADMIN = 'http://localhost:5088/api';
const API_ECOMMERCE = 'http://localhost:5180/api';

async function fetchAPI(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers
        }
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Error HTTP ${response.status}: ${errorText}`);
    }

    return response.json();
}

// Catálogo
async function obtenerCatalogo(page = 1, pageSize = 10, categoriaId = null) {
    let url = `${API_ECOMMERCE}/catalogo?page=${page}&pageSize=${pageSize}`;
    if (categoriaId) {
        url += `&categoriaId=${categoriaId}`;
    }
    return fetchAPI(url);
}

// Carrito
async function crearCarrito(clienteId) {
    return fetchAPI(`${API_ECOMMERCE}/carrito`, {
        method: 'POST',
        body: JSON.stringify({ clienteId })
    });
}

async function obtenerCarritoDetalles(carritoId) {
    return fetchAPI(`${API_ECOMMERCE}/carrito/${carritoId}`);
}

async function agregarItemCarrito(carritoId, productoId, cantidad) {
    return fetchAPI(`${API_ECOMMERCE}/carrito/items`, {
        method: 'POST',
        body: JSON.stringify({
            carritoId: carritoId,
            productoId: productoId,
            cantidad: cantidad
        })
    });
}

async function actualizarItemCarrito(carritoId, productoId, cantidad) {
    return fetchAPI(`${API_ECOMMERCE}/carrito/items`, {
        method: 'PUT',
        body: JSON.stringify({
            carritoId: carritoId,
            productoId: productoId,
            cantidad: cantidad
        })
    });
}

async function eliminarItemCarrito(carritoId, productoId) {
    return actualizarItemCarrito(carritoId, productoId, 0);
}

// Pedidos
async function crearPedidoSincrono(carritoId) {
    return fetchAPI(`${API_ECOMMERCE}/pedidos/sincrono`, {
        method: 'POST',
        body: JSON.stringify({ carritoId })
    });
}

async function crearPedidoAsincrono(carritoId) {
    return fetchAPI(`${API_ECOMMERCE}/pedidos/asincrono`, {
        method: 'POST',
        body: JSON.stringify({ carritoId })
    });
}

// Admin - Productos
async function obtenerProductos(incluirInactivos = true) {
    return fetchAPI(`${API_ADMIN}/productos?incluirInactivos=${incluirInactivos}`);
}

async function obtenerProducto(id) {
    return fetchAPI(`${API_ADMIN}/productos/${id}`);
}

async function crearProducto(producto) {
    return fetchAPI(`${API_ADMIN}/productos`, {
        method: 'POST',
        body: JSON.stringify(producto)
    });
}

async function actualizarProducto(id, producto) {
    return fetchAPI(`${API_ADMIN}/productos/${id}`, {
        method: 'PUT',
        body: JSON.stringify(producto)
    });
}

async function eliminarProducto(id) {
    return fetchAPI(`${API_ADMIN}/productos/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Categorías
async function obtenerCategorias() {
    return fetchAPI(`${API_ADMIN}/categorias`);
}

async function obtenerCategoria(id) {
    return fetchAPI(`${API_ADMIN}/categorias/${id}`);
}

async function crearCategoria(categoria) {
    return fetchAPI(`${API_ADMIN}/categorias`, {
        method: 'POST',
        body: JSON.stringify(categoria)
    });
}

async function actualizarCategoria(id, categoria) {
    return fetchAPI(`${API_ADMIN}/categorias/${id}`, {
        method: 'PUT',
        body: JSON.stringify(categoria)
    });
}

async function eliminarCategoria(id) {
    return fetchAPI(`${API_ADMIN}/categorias/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Proveedores
async function obtenerProveedores(incluirInactivos = true) {
    return fetchAPI(`${API_ADMIN}/proveedores?incluirInactivos=${incluirInactivos}`);
}

async function obtenerProveedor(id) {
    return fetchAPI(`${API_ADMIN}/proveedores/${id}`);
}

async function crearProveedor(proveedor) {
    return fetchAPI(`${API_ADMIN}/proveedores`, {
        method: 'POST',
        body: JSON.stringify(proveedor)
    });
}

async function actualizarProveedor(id, proveedor) {
    return fetchAPI(`${API_ADMIN}/proveedores/${id}`, {
        method: 'PUT',
        body: JSON.stringify(proveedor)
    });
}

async function eliminarProveedor(id) {
    return fetchAPI(`${API_ADMIN}/proveedores/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Clientes
async function obtenerClientes() {
    return fetchAPI(`${API_ADMIN}/clientes`);
}

async function obtenerCliente(id) {
    return fetchAPI(`${API_ADMIN}/clientes/${id}`);
}

async function crearCliente(cliente) {
    return fetchAPI(`${API_ADMIN}/clientes`, {
        method: 'POST',
        body: JSON.stringify(cliente)
    });
}

async function actualizarCliente(id, cliente) {
    return fetchAPI(`${API_ADMIN}/clientes/${id}`, {
        method: 'PUT',
        body: JSON.stringify(cliente)
    });
}

async function eliminarCliente(id) {
    return fetchAPI(`${API_ADMIN}/clientes/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Compras
async function obtenerCompras() {
    return fetchAPI(`${API_ADMIN}/compras`);
}

async function obtenerCompra(id) {
    return fetchAPI(`${API_ADMIN}/compras/${id}`);
}

async function crearCompra(compra) {
    return fetchAPI(`${API_ADMIN}/compras`, {
        method: 'POST',
        body: JSON.stringify(compra)
    });
}

async function actualizarCompra(id, compra) {
    return fetchAPI(`${API_ADMIN}/compras/${id}`, {
        method: 'PUT',
        body: JSON.stringify(compra)
    });
}

async function eliminarCompra(id) {
    return fetchAPI(`${API_ADMIN}/compras/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Ventas
async function obtenerVentas() {
    return fetchAPI(`${API_ADMIN}/ventas`);
}

async function obtenerVenta(id) {
    return fetchAPI(`${API_ADMIN}/ventas/${id}`);
}

async function crearVenta(venta) {
    return fetchAPI(`${API_ADMIN}/ventas`, {
        method: 'POST',
        body: JSON.stringify(venta)
    });
}

async function actualizarVenta(id, venta) {
    return fetchAPI(`${API_ADMIN}/ventas/${id}`, {
        method: 'PUT',
        body: JSON.stringify(venta)
    });
}

async function cambiarEstadoVenta(id, estado) {
    return fetchAPI(`${API_ADMIN}/ventas/${id}/estado`, {
        method: 'PATCH',
        body: JSON.stringify({ Estado: estado })
    });
}

async function eliminarVenta(id) {
    return fetchAPI(`${API_ADMIN}/ventas/${id}`, {
        method: 'DELETE'
    });
}

// Admin - Reportes
async function obtenerReportePeriodo(desde, hasta) {
    return fetchAPI(`${API_ADMIN}/reportes/periodo?desde=${desde}&hasta=${hasta}`);
}

async function obtenerReporteProducto(productoId, desde, hasta) {
    return fetchAPI(`${API_ADMIN}/reportes/producto/${productoId}?desde=${desde}&hasta=${hasta}`);
}

async function obtenerReporteCliente(clienteId, desde, hasta) {
    return fetchAPI(`${API_ADMIN}/reportes/cliente/${clienteId}?desde=${desde}&hasta=${hasta}`);
}

async function obtenerReporteProveedor(proveedorId, desde, hasta) {
    return fetchAPI(`${API_ADMIN}/reportes/proveedor/${proveedorId}?desde=${desde}&hasta=${hasta}`);
}

// Funciones de utilidad para el carrito local
function obtenerCarritoLocal() {
    const carrito = localStorage.getItem('carrito');
    return carrito ? JSON.parse(carrito) : { id: null, clienteId: null, items: [] };
}

function guardarCarritoLocal(carrito) {
    localStorage.setItem('carrito', JSON.stringify(carrito));
}
