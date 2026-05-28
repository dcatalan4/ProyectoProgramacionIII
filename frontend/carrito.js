let carrito = { id: null, items: [] };

async function cargarCarrito() {
    try {
        carrito = obtenerCarritoLocal();

        if (carrito.id) {
            const carritoDetalles = await obtenerCarritoDetalles(carrito.id);
            carrito.items = carritoDetalles.items || [];
            guardarCarritoLocal(carrito);
        }

        renderizarCarrito();
    } catch (error) {
        console.error('Error al cargar carrito:', error);
    }
}

function renderizarCarrito() {
    const container = document.getElementById('carrito-items');
    container.innerHTML = '';

    if (carrito.items.length === 0) {
        container.innerHTML = '<p>El carrito está vacío</p>';
        document.getElementById('total').textContent = '0.00';
        return;
    }

    let total = 0;

    carrito.items.forEach(item => {
        const subtotal = item.precioUnitario * item.cantidad;
        total += subtotal;

        const div = document.createElement('div');
        div.className = 'carrito-item';
        div.innerHTML = `
            <div class="carrito-item-info">
                <h3>${item.producto || item.productoId}</h3>
                <p>Precio: Q${item.precioUnitario?.toFixed(2) || '0.00'}</p>
                <p>Subtotal: Q${subtotal.toFixed(2)}</p>
            </div>
            <div class="carrito-item-actions">
                <div class="cantidad">
                    <button onclick="actualizarCantidad('${item.productoId}', -1)">-</button>
                    <span>${item.cantidad}</span>
                    <button onclick="actualizarCantidad('${item.productoId}', 1)">+</button>
                </div>
                <button onclick="quitarProducto('${item.productoId}')" class="btn btn-danger btn-quitar">Quitar</button>
            </div>
        `;
        container.appendChild(div);
    });

    document.getElementById('total').textContent = total.toFixed(2);
}

async function sincronizarCarrito(response) {
    carrito.items = response.items || [];
    guardarCarritoLocal(carrito);
    renderizarCarrito();
}

async function actualizarCantidad(productoId, delta) {
    if (!carrito.id) return;

    try {
        const item = carrito.items.find(i => i.productoId === productoId);
        if (!item) return;

        const nuevaCantidad = item.cantidad + delta;
        const response = await actualizarItemCarrito(carrito.id, productoId, nuevaCantidad);
        await sincronizarCarrito(response);
    } catch (error) {
        console.error('Error al actualizar cantidad:', error);
        alert(error.message || 'Error al actualizar cantidad');
        await cargarCarrito();
    }
}

async function quitarProducto(productoId) {
    if (!carrito.id) return;

    try {
        const response = await eliminarItemCarrito(carrito.id, productoId);
        await sincronizarCarrito(response);
    } catch (error) {
        console.error('Error al quitar producto:', error);
        alert(error.message || 'Error al quitar producto');
        await cargarCarrito();
    }
}

async function procesarPedido() {
    try {
        if (!carrito.id || carrito.items.length === 0) {
            alert('El carrito está vacío');
            return;
        }

        const response = await crearPedidoSincrono(carrito.id);
        alert(`Pedido procesado exitosamente. Venta ID: ${response.ventaId || response.id || 'N/A'}`);

        carrito = { id: null, clienteId: null, items: [] };
        guardarCarritoLocal(carrito);
        renderizarCarrito();
    } catch (error) {
        console.error('Error al procesar pedido:', error);
        const mensaje = error.message?.includes('Error HTTP')
            ? error.message.replace(/^Error HTTP \d+: /, '')
            : 'Error al procesar pedido';
        alert(mensaje);
    }
}

document.getElementById('btn-procesar-pedido').addEventListener('click', procesarPedido);

document.addEventListener('DOMContentLoaded', cargarCarrito);
