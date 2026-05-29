let carrito = { id: null, items: [] };
let solicitudActual = null;

function formatearQuetzales(valor) {
    return 'Q' + Number(valor || 0).toFixed(2);
}

function toClienteLocal(cliente) {
    return {
        id: cliente.Id || cliente.id,
        idOriginal: cliente.IdOriginal || cliente.idOriginal || '',
        nombre: cliente.Nombre || cliente.nombre || '',
        apellido: cliente.Apellido || cliente.apellido || '',
        email: cliente.Email || cliente.email || '',
        telefono: cliente.Telefono || cliente.telefono || '',
        direccion: cliente.DireccionEnvio || cliente.direccionEnvio || cliente.Direccion || cliente.direccion || ''
    };
}

async function asegurarClienteCarrito() {
    if (!carrito.clienteId || carrito.cliente) {
        return;
    }

    try {
        const cliente = await obtenerCliente(carrito.clienteId);
        carrito.cliente = toClienteLocal(cliente);
        guardarCarritoLocal(carrito);
    } catch (error) {
        console.error('Error al cargar cliente del carrito:', error);
    }
}

async function cargarCarrito() {
    try {
        carrito = obtenerCarritoLocal();
        await asegurarClienteCarrito();

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
    renderizarClienteCarrito();

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
                <p>Precio: ${formatearQuetzales(item.precioUnitario)}</p>
                <p>Subtotal: ${formatearQuetzales(subtotal)}</p>
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

function renderizarClienteCarrito() {
    const container = document.getElementById('cliente-carrito');
    const cliente = carrito.cliente;

    if (!carrito.clienteId) {
        container.innerHTML = `
            <strong>Cliente seleccionado</strong>
            <p>No hay cliente seleccionado para este carrito.</p>
            <a href="index.html" class="btn btn-secondary">Seleccionar cliente</a>
        `;
        return;
    }

    const nombreCompleto = cliente
        ? `${cliente.nombre || ''} ${cliente.apellido || ''}`.trim()
        : '';

    container.innerHTML = `
        <strong>Cliente seleccionado</strong>
        <p>${nombreCompleto || 'Cliente'}${cliente?.idOriginal ? ` - ID: ${cliente.idOriginal}` : ''}</p>
        <p>${cliente?.email || 'Email no disponible'}${cliente?.telefono ? ` - ${cliente.telefono}` : ''}</p>
        <a href="index.html" class="btn btn-secondary">Cambiar cliente</a>
    `;
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

        const modo = document.querySelector('input[name="modo-pedido"]:checked').value;
        let response;
        
        if (modo === 'sincrono') {
            response = await crearPedidoSincrono(carrito.id);
            alert(`Pedido procesado exitosamente (Síncrono). Venta ID: ${response.ventaId || response.id || 'N/A'}`);
            carrito = { id: null, clienteId: null, cliente: null, items: [] };
            guardarCarritoLocal(carrito);
            renderizarCarrito();
        } else {
            response = await crearPedidoAsincrono(carrito.id);
            solicitudActual = response.solicitudId || response.id;
            mostrarEstadoSolicitud(solicitudActual);
            alert(`Pedido encolado exitosamente (Asíncrono). Solicitud ID: ${solicitudActual}`);
            // No limpiar el carrito inmediatamente en modo asíncrono
            // El carrito se limpia cuando el pedido se completa o se finaliza manualmente
        }
    } catch (error) {
        console.error('Error al procesar pedido:', error);
        const mensaje = error.message?.includes('Error HTTP')
            ? error.message.replace(/^Error HTTP \d+: /, '')
            : 'Error al procesar pedido';
        alert(mensaje);
    }
}

function mostrarEstadoSolicitud(solicitudId) {
    document.getElementById('estado-solicitud').style.display = 'block';
    document.getElementById('solicitud-id').textContent = solicitudId;
    document.getElementById('solicitud-estado').textContent = 'Encolado';
    document.getElementById('solicitud-venta').style.display = 'none';
    document.getElementById('solicitud-error').style.display = 'none';
    document.getElementById('btn-finalizar-solicitud').style.display = 'inline-block';
    
    // Iniciar polling para verificar el estado automáticamente
    iniciarPollingEstado(solicitudId);
}

let estadoPollingInterval = null;

function iniciarPollingEstado(solicitudId) {
    console.log('Iniciando polling para solicitud:', solicitudId);
    // Detener cualquier polling anterior
    if (estadoPollingInterval) {
        clearInterval(estadoPollingInterval);
    }
    
    // Consultar el estado cada 2 segundos
    estadoPollingInterval = setInterval(async () => {
        try {
            console.log('Consultando estado de solicitud:', solicitudId);
            const estado = await obtenerEstadoSolicitud(solicitudId);
            console.log('Estado recibido:', estado);
            document.getElementById('solicitud-estado').textContent = estado.estado;
            
            if (estado.estado === 'Completado') {
                console.log('Pedido completado, VentaId:', estado.ventaId);
                document.getElementById('solicitud-venta').style.display = 'block';
                document.getElementById('venta-id').textContent = estado.ventaId;
                document.getElementById('btn-finalizar-solicitud').style.display = 'none';
                alert('Pedido completado exitosamente');
                carrito = { id: null, clienteId: null, cliente: null, items: [] };
                guardarCarritoLocal(carrito);
                renderizarCarrito();
                document.getElementById('estado-solicitud').style.display = 'none';
                clearInterval(estadoPollingInterval);
                estadoPollingInterval = null;
            } else if (estado.estado === 'Fallido') {
                console.log('Pedido fallido, Error:', estado.mensajeError);
                document.getElementById('solicitud-error').style.display = 'block';
                document.getElementById('error-mensaje').textContent = estado.mensajeError;
                document.getElementById('btn-finalizar-solicitud').style.display = 'inline-block';
                clearInterval(estadoPollingInterval);
                estadoPollingInterval = null;
            }
        } catch (error) {
            console.error('Error al consultar estado:', error);
            alert('Error al consultar estado: ' + error.message);
        }
    }, 2000);
}

async function consultarEstadoSolicitud() {
    if (!solicitudActual) return;
    
    try {
        const estado = await obtenerEstadoSolicitud(solicitudActual);
        document.getElementById('solicitud-estado').textContent = estado.estado;
        
        if (estado.estado === 'Completado') {
            document.getElementById('solicitud-venta').style.display = 'block';
            document.getElementById('venta-id').textContent = estado.ventaId;
            document.getElementById('btn-finalizar-solicitud').style.display = 'none';
            alert('Pedido completado exitosamente');
            carrito = { id: null, clienteId: null, cliente: null, items: [] };
            guardarCarritoLocal(carrito);
            renderizarCarrito();
            document.getElementById('estado-solicitud').style.display = 'none';
        } else if (estado.estado === 'Fallido') {
            document.getElementById('solicitud-error').style.display = 'block';
            document.getElementById('error-mensaje').textContent = estado.mensajeError;
            document.getElementById('btn-finalizar-solicitud').style.display = 'inline-block';
        }
    } catch (error) {
        console.error('Error al consultar estado:', error);
        alert('Error al consultar estado: ' + error.message);
    }
}

async function finalizarSolicitudManualmente() {
    if (!solicitudActual) return;
    
    try {
        const response = await finalizarSolicitudManualmenteApi(solicitudActual);
        alert(`Pedido finalizado manualmente. Venta ID: ${response.ventaId || response.id || 'N/A'}`);
        carrito = { id: null, clienteId: null, cliente: null, items: [] };
        guardarCarritoLocal(carrito);
        renderizarCarrito();
        document.getElementById('estado-solicitud').style.display = 'none';
    } catch (error) {
        console.error('Error al finalizar solicitud:', error);
        const mensaje = error.message?.includes('Error HTTP')
            ? error.message.replace(/^Error HTTP \d+: /, '')
            : 'Error al finalizar solicitud';
        alert(mensaje);
        
        // Si la solicitud ya fue completada, ocultar la sección de estado
        if (mensaje.includes('ya fue completada')) {
            document.getElementById('estado-solicitud').style.display = 'none';
        }
    }
}

document.getElementById('btn-procesar-pedido').addEventListener('click', procesarPedido);
document.getElementById('btn-finalizar-solicitud').addEventListener('click', finalizarSolicitudManualmente);

document.addEventListener('DOMContentLoaded', cargarCarrito);
