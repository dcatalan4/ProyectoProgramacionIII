// Pestañas
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
        btn.classList.add('active');
        document.getElementById(`tab-${btn.dataset.tab}`).classList.add('active');
        
        // Cargar datos según la pestaña
        switch(btn.dataset.tab) {
            case 'productos': cargarProductos(); break;
            case 'proveedores': cargarProveedores(); break;
            case 'clientes': cargarClientes(); break;
            case 'compras': cargarCompras(); break;
            case 'ventas': cargarVentas(); break;
        }
    });
});

// Productos
const modalProducto = document.getElementById('modal-producto');

function obtenerIdUsuario(item) {
    const id = (item.IdOriginal || item.idOriginal || '').trim();
    return id.length > 0 ? id : null;
}

function formatearFecha(valor) {
    if (!valor) {
        return 'N/A';
    }

    const fecha = new Date(valor);
    return Number.isNaN(fecha.getTime()) ? 'N/A' : fecha.toLocaleDateString();
}

function formatearEstadoVenta(estado) {
    const mapa = {
        1: 'Recibida',
        2: 'Pagada',
        3: 'Preparando',
        4: 'Enviada',
        5: 'Entregada',
        6: 'Cancelada',
        Recibida: 'Recibida',
        Pagada: 'Pagada',
        Preparando: 'Preparando',
        Enviada: 'Enviada',
        Entregada: 'Entregada',
        Cancelada: 'Cancelada'
    };

    return mapa[estado] ?? estado ?? 'N/A';
}

function setCamposVentaModo(crear) {
    const camposRequeridos = [
        'venta-id-input',
        'venta-numero',
        'venta-cliente-id',
        'venta-fecha',
        'venta-producto-id',
        'venta-cantidad'
    ];

    camposRequeridos.forEach(id => {
        const campo = document.getElementById(id);
        if (!campo) {
            return;
        }

        if (crear) {
            campo.setAttribute('required', 'required');
        } else {
            campo.removeAttribute('required');
        }

        if (campo.parentElement) {
            campo.parentElement.style.display = crear ? 'block' : 'none';
        }
    });

    document.getElementById('grupo-detalles-venta').style.display = crear ? 'block' : 'none';
    document.getElementById('grupo-cantidad-venta').style.display = crear ? 'block' : 'none';
    document.getElementById('grupo-estado').style.display = crear ? 'none' : 'block';
}

async function cargarProductos() {
    try {
        const productos = await obtenerProductos();
        const lista = document.getElementById('lista-productos');
        lista.innerHTML = '';

        productos.forEach(p => {
            const item = document.createElement('div');
            item.className = 'item-card';
            item.innerHTML = `
                <strong>${p.Nombre || p.nombre}</strong>
                <p>ID: ${obtenerIdUsuario(p) || 'N/A'}</p>
                <p>Descripción: ${p.Descripcion || p.descripcion || 'Sin descripción'}</p>
                <p>Precio Venta: $${(p.PrecioVenta || p.precioVenta).toFixed(2)}</p>
                <p>Precio Compra: $${(p.PrecioCompra || p.precioCompra).toFixed(2)}</p>
                <p>Stock: ${p.Stock || p.stock}</p>
                <p>Proveedor: ${p.Proveedor || p.proveedor || 'N/A'}</p>
                <p>Activo: ${p.EstaActivo || p.estaActivo ? 'Sí' : 'No'}</p>
                <div class="item-actions">
                    <button onclick="editarProducto('${p.Id || p.id}')" class="btn btn-secondary">Editar</button>
                    <button onclick="eliminarProducto('${p.Id || p.id}')" class="btn btn-danger">Eliminar</button>
                </div>
            `;
            lista.appendChild(item);
        });
    } catch (error) {
        console.error('Error al cargar productos:', error);
    }
}

document.getElementById('btn-nuevo-producto').addEventListener('click', () => {
    document.getElementById('modal-producto-titulo').textContent = 'Nuevo Producto';
    document.getElementById('form-producto').reset();
    document.getElementById('producto-id').value = '';
    modalProducto.style.display = 'block';
});

document.getElementById('form-producto').addEventListener('submit', async (e) => {
    e.preventDefault();

    // Validar longitud mínima
    const productoId = document.getElementById('producto-id-input').value;
    const proveedorId = document.getElementById('producto-proveedor-id').value;

    if (!productoId || productoId.length < 4) {
        alert('Producto ID debe tener al menos 4 caracteres');
        return;
    }

    if (!proveedorId || proveedorId.length < 4) {
        alert('Proveedor ID debe tener al menos 4 caracteres');
        return;
    }

    const producto = {
        Id: productoId,
        Nombre: document.getElementById('producto-nombre').value,
        Descripcion: document.getElementById('producto-descripcion').value,
        PrecioVenta: parseFloat(document.getElementById('producto-precio-venta').value),
        PrecioCompra: parseFloat(document.getElementById('producto-precio-compra').value),
        Stock: parseInt(document.getElementById('producto-stock').value),
        ProveedorId: proveedorId
    };

    const id = document.getElementById('producto-id').value;
    
    try {
        if (id) {
            await actualizarProducto(id, producto);
            alert('Producto actualizado');
        } else {
            await crearProducto(producto);
            alert('Producto creado');
        }
        
        modalProducto.style.display = 'none';
        cargarProductos();
    } catch (error) {
        alert('Error al guardar producto: ' + error.message);
        console.error('Error detallado:', error);
    }
});

async function editarProducto(id) {
    try {
        const producto = await obtenerProducto(id);
        document.getElementById('modal-producto-titulo').textContent = 'Editar Producto';
        document.getElementById('producto-id').value = producto.Id || producto.id;
        document.getElementById('producto-id-input').value = producto.IdOriginal || producto.idOriginal || '';
        document.getElementById('producto-nombre').value = producto.Nombre || producto.nombre;
        document.getElementById('producto-descripcion').value = producto.Descripcion || producto.descripcion || '';
        document.getElementById('producto-precio-venta').value = producto.PrecioVenta || producto.precioVenta;
        document.getElementById('producto-precio-compra').value = producto.PrecioCompra || producto.precioCompra;
        document.getElementById('producto-stock').value = producto.Stock || producto.stock;
        document.getElementById('producto-proveedor-id').value = producto.ProveedorId || producto.proveedorId;
        modalProducto.style.display = 'block';
    } catch (error) {
        alert('Error al cargar producto: ' + error.message);
    }
}

async function eliminarProducto(id) {
    if (confirm('¿Estás seguro de eliminar este producto?')) {
        try {
            await eliminarProductoApi(id);
            alert('Producto eliminado');
            cargarProductos();
        } catch (error) {
            alert('Error al eliminar producto');
        }
    }
}

async function eliminarProductoApi(id) {
    return fetchAPI(`${API_ADMIN}/productos/${id}`, {
        method: 'DELETE'
    });
}

// Proveedores
const modalProveedor = document.getElementById('modal-proveedor');

async function cargarProveedores() {
    try {
        const proveedores = await obtenerProveedores();
        const lista = document.getElementById('lista-proveedores');
        lista.innerHTML = '';

        proveedores.forEach(p => {
            const item = document.createElement('div');
            item.className = 'item-card';
            item.innerHTML = `
                <strong>${p.Nombre || p.nombre}</strong>
                <p>Dirección: ${p.Direccion || p.direccion || 'N/A'}</p>
                <p>Teléfono: ${p.Telefono || p.telefono || 'N/A'}</p>
                <p>Email: ${p.Email || p.email || 'N/A'}</p>
                <p>ID: ${obtenerIdUsuario(p) || 'N/A'}</p>
                <div class="item-actions">
                    <button onclick="editarProveedor('${p.Id || p.id}')" class="btn btn-secondary">Editar</button>
                    <button onclick="eliminarProveedor('${p.Id || p.id}')" class="btn btn-danger">Eliminar</button>
                </div>
            `;
            lista.appendChild(item);
        });
    } catch (error) {
        console.error('Error al cargar proveedores:', error);
    }
}

document.getElementById('btn-nuevo-proveedor').addEventListener('click', () => {
    document.getElementById('modal-proveedor-titulo').textContent = 'Nuevo Proveedor';
    document.getElementById('form-proveedor').reset();
    document.getElementById('proveedor-id').value = '';
    modalProveedor.style.display = 'block';
});

document.getElementById('form-proveedor').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const proveedor = {
        Id: document.getElementById('proveedor-id-input').value,
        Nombre: document.getElementById('proveedor-nombre').value,
        Direccion: document.getElementById('proveedor-direccion').value || '',
        Telefono: document.getElementById('proveedor-telefono').value || '',
        Email: document.getElementById('proveedor-email').value || ''
    };
    
    const id = document.getElementById('proveedor-id').value;
    
    try {
        if (id) {
            await actualizarProveedor(id, proveedor);
            alert('Proveedor actualizado');
        } else {
            await crearProveedor(proveedor);
            alert('Proveedor creado');
        }
        
        modalProveedor.style.display = 'none';
        cargarProveedores();
    } catch (error) {
        alert('Error al guardar proveedor: ' + error.message);
        console.error('Error detallado:', error);
    }
});

async function editarProveedor(id) {
    try {
        const proveedor = await obtenerProveedor(id);
        document.getElementById('modal-proveedor-titulo').textContent = 'Editar Proveedor';
        document.getElementById('proveedor-id').value = proveedor.Id || proveedor.id;
        document.getElementById('proveedor-id-input').value = proveedor.idOriginal || proveedor.Id || proveedor.id;
        document.getElementById('proveedor-nombre').value = proveedor.Nombre || proveedor.nombre;
        document.getElementById('proveedor-direccion').value = proveedor.Direccion || proveedor.direccion || '';
        document.getElementById('proveedor-telefono').value = proveedor.Telefono || proveedor.telefono || '';
        document.getElementById('proveedor-email').value = proveedor.Email || proveedor.email || '';
        modalProveedor.style.display = 'block';
    } catch (error) {
        alert('Error al cargar proveedor: ' + error.message);
    }
}

async function eliminarProveedor(id) {
    if (confirm('¿Estás seguro de eliminar este proveedor?')) {
        try {
            await eliminarProveedorApi(id);
            alert('Proveedor eliminado');
            cargarProveedores();
        } catch (error) {
            alert('Error al eliminar proveedor');
        }
    }
}

async function eliminarProveedorApi(id) {
    return fetchAPI(`${API_ADMIN}/proveedores/${id}`, {
        method: 'DELETE'
    });
}

// Clientes
const modalCliente = document.getElementById('modal-cliente');

async function cargarClientes() {
    try {
        const clientes = await obtenerClientes();
        const lista = document.getElementById('lista-clientes');
        lista.innerHTML = '';

        clientes.forEach(c => {
            const item = document.createElement('div');
            item.className = 'item-card';
            item.innerHTML = `
                <strong>${c.Nombre || c.nombre} ${c.Apellido || c.apellido || ''}</strong>
                <p>Dirección: ${c.DireccionEnvio || c.direccionEnvio || 'N/A'}</p>
                <p>Teléfono: ${c.Telefono || c.telefono || 'N/A'}</p>
                <p>Email: ${c.Email || c.email || 'N/A'}</p>
                <p>ID: ${obtenerIdUsuario(c) || 'N/A'}</p>
                <div class="item-actions">
                    <button onclick="editarCliente('${c.Id || c.id}')" class="btn btn-secondary">Editar</button>
                    <button onclick="eliminarCliente('${c.Id || c.id}')" class="btn btn-danger">Eliminar</button>
                </div>
            `;
            lista.appendChild(item);
        });
    } catch (error) {
        console.error('Error al cargar clientes:', error);
    }
}

document.getElementById('btn-nuevo-cliente').addEventListener('click', () => {
    document.getElementById('modal-cliente-titulo').textContent = 'Nuevo Cliente';
    document.getElementById('form-cliente').reset();
    document.getElementById('cliente-id').value = '';
    modalCliente.style.display = 'block';
});

document.getElementById('form-cliente').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const cliente = {
        Id: document.getElementById('cliente-id-input').value,
        Nombre: document.getElementById('cliente-nombre').value,
        Apellido: document.getElementById('cliente-apellido').value,
        DireccionEnvio: document.getElementById('cliente-direccion').value || '',
        Direccion: document.getElementById('cliente-direccion').value || '',
        Telefono: document.getElementById('cliente-telefono').value || '',
        Email: document.getElementById('cliente-email').value || ''
    };
    
    const id = document.getElementById('cliente-id').value;
    
    try {
        if (id) {
            await actualizarCliente(id, cliente);
            alert('Cliente actualizado');
        } else {
            await crearCliente(cliente);
            alert('Cliente creado');
        }
        
        modalCliente.style.display = 'none';
        cargarClientes();
    } catch (error) {
        alert('Error al guardar cliente: ' + error.message);
        console.error('Error detallado:', error);
    }
});

async function editarCliente(id) {
    try {
        const cliente = await obtenerCliente(id);
        document.getElementById('modal-cliente-titulo').textContent = 'Editar Cliente';
        document.getElementById('cliente-id').value = cliente.Id || cliente.id;
        document.getElementById('cliente-id-input').value = cliente.IdOriginal || cliente.idOriginal || '';
        document.getElementById('cliente-nombre').value = cliente.Nombre || cliente.nombre;
        document.getElementById('cliente-apellido').value = cliente.Apellido || cliente.apellido || '';
        document.getElementById('cliente-direccion').value = cliente.DireccionEnvio || cliente.direccionEnvio || '';
        document.getElementById('cliente-telefono').value = cliente.Telefono || cliente.telefono || '';
        document.getElementById('cliente-email').value = cliente.Email || cliente.email || '';
        modalCliente.style.display = 'block';
    } catch (error) {
        alert('Error al cargar cliente: ' + error.message);
    }
}

async function eliminarCliente(id) {
    if (confirm('¿Estás seguro de eliminar este cliente?')) {
        try {
            await eliminarClienteApi(id);
            alert('Cliente eliminado');
            cargarClientes();
        } catch (error) {
            alert('Error al eliminar cliente');
        }
    }
}

async function eliminarClienteApi(id) {
    return fetchAPI(`${API_ADMIN}/clientes/${id}`, {
        method: 'DELETE'
    });
}

// Compras
const modalCompra = document.getElementById('modal-compra');

async function cargarCompras() {
    try {
        const compras = await obtenerCompras();
        const lista = document.getElementById('lista-compras');
        lista.innerHTML = '';

        compras.forEach(c => {
            const item = document.createElement('div');
            item.className = 'item-card';
            const detalles = (c.Detalles || c.detalles || []).map(d => `Producto ID: ${d.ProductoId || d.productoId}, Cantidad: ${d.Cantidad || d.cantidad}`).join(', ');
            const fecha = c.FechaUtc || c.fechaUtc || c.Fecha || c.fecha;
            item.innerHTML = `
                <strong>Compra ID: ${obtenerIdUsuario(c) || c.Id || c.id}</strong>
                <p>Número: ${c.NumeroCompra || c.numeroCompra}</p>
                <p>Proveedor ID: ${c.ProveedorId || c.proveedorId}</p>
                <p>Fecha: ${formatearFecha(fecha)}</p>
                <p>Detalles: ${detalles}</p>
                <div class="item-actions">
                    <button onclick="eliminarCompra('${c.Id || c.id}')" class="btn btn-danger">Eliminar</button>
                </div>
            `;
            lista.appendChild(item);
        });
    } catch (error) {
        console.error('Error al cargar compras:', error);
    }
}

document.getElementById('btn-nueva-compra').addEventListener('click', () => {
    document.getElementById('modal-compra-titulo').textContent = 'Nueva Compra';
    document.getElementById('form-compra').reset();
    document.getElementById('compra-id').value = '';
    modalCompra.style.display = 'block';
});

document.getElementById('form-compra').addEventListener('submit', async (e) => {
    e.preventDefault();

    const compraId = document.getElementById('compra-id-input').value;
    if (!compraId || compraId.length < 4) {
        alert('El ID de la compra es obligatorio y debe tener al menos 4 caracteres.');
        return;
    }

    const fechaCompra = document.getElementById('compra-fecha').value;
    if (!fechaCompra) {
        alert('Por favor ingresa la fecha de la compra.');
        return;
    }

    const compra = {
        Id: compraId,
        NumeroCompra: document.getElementById('compra-numero').value,
        ProveedorId: document.getElementById('compra-proveedor-id').value,
        Fecha: document.getElementById('compra-fecha').value,
        Detalles: [{
            ProductoId: document.getElementById('compra-producto-id').value,
            Cantidad: parseInt(document.getElementById('compra-cantidad').value),
            CostoUnitario: parseFloat(document.getElementById('compra-costo-unitario').value)
        }]
    };

    try {
        await crearCompra(compra);
        alert('Compra creada');
        modalCompra.style.display = 'none';
        cargarCompras();
    } catch (error) {
        alert('Error al guardar compra: ' + error.message);
        console.error('Error detallado:', error);
    }
});

async function eliminarCompra(id) {
    if (confirm('¿Estás seguro de eliminar esta compra?')) {
        try {
            await eliminarCompraApi(id);
            alert('Compra eliminada');
            cargarCompras();
        } catch (error) {
            alert('Error al eliminar compra');
        }
    }
}

async function eliminarCompraApi(id) {
    return fetchAPI(`${API_ADMIN}/compras/${id}`, {
        method: 'DELETE'
    });
}

// Ventas
const modalVenta = document.getElementById('modal-venta');

async function cargarVentas() {
    try {
        const ventas = await obtenerVentas();
        const lista = document.getElementById('lista-ventas');
        lista.innerHTML = '';

        ventas.forEach(v => {
            const item = document.createElement('div');
            item.className = 'item-card';
            item.innerHTML = `
                <strong>Venta ID: ${obtenerIdUsuario(v) || v.Id || v.id}</strong>
                <p>Cliente ID: ${v.ClienteId || v.clienteId}</p>
                <p>Fecha: ${formatearFecha(v.FechaUtc || v.fechaUtc || v.Fecha || v.fecha)}</p>
                <p>Estado: ${formatearEstadoVenta(v.Estado ?? v.estado)}</p>
                <div class="item-actions">
                    <button onclick="editarVenta('${v.Id || v.id}')" class="btn btn-secondary">Cambiar Estado</button>
                    <button onclick="eliminarVenta('${v.Id || v.id}')" class="btn btn-danger">Eliminar</button>
                </div>
            `;
            lista.appendChild(item);
        });
    } catch (error) {
        console.error('Error al cargar ventas:', error);
    }
}

document.getElementById('btn-nueva-venta').addEventListener('click', () => {
    document.getElementById('modal-venta-titulo').textContent = 'Nueva Venta';
    document.getElementById('form-venta').reset();
    document.getElementById('venta-id').value = '';
    setCamposVentaModo(true);
    modalVenta.style.display = 'block';
});

document.getElementById('form-venta').addEventListener('submit', async (e) => {
    e.preventDefault();

    const id = document.getElementById('venta-id').value;

    try {
        if (id) {
            const estado = parseInt(document.getElementById('venta-estado').value, 10);
            await cambiarEstadoVenta(id, estado);
            alert('Estado de venta actualizado');
            modalVenta.style.display = 'none';
            cargarVentas();
            return;
        }

        const ventaId = document.getElementById('venta-id-input').value;
        if (!ventaId || ventaId.length < 4) {
            alert('El ID de la venta es obligatorio y debe tener al menos 4 caracteres.');
            return;
        }

        const fechaVenta = document.getElementById('venta-fecha').value;
        if (!fechaVenta) {
            alert('Por favor ingresa la fecha de la venta.');
            return;
        }

        const productoId = document.getElementById('venta-producto-id').value;
        if (!productoId || productoId.length < 4) {
            alert('El ID del producto es obligatorio y debe tener al menos 4 caracteres.');
            return;
        }

        const venta = {
            Id: ventaId,
            NumeroVenta: document.getElementById('venta-numero').value,
            ClienteId: document.getElementById('venta-cliente-id').value,
            Fecha: document.getElementById('venta-fecha').value,
            Detalles: [{
                ProductoId: document.getElementById('venta-producto-id').value,
                Cantidad: parseInt(document.getElementById('venta-cantidad').value)
            }]
        };

        await crearVenta(venta);
        alert('Venta creada');
        modalVenta.style.display = 'none';
        cargarVentas();
    } catch (error) {
        alert('Error al guardar venta: ' + error.message);
        console.error('Error detallado:', error);
    }
});

async function editarVenta(id) {
    try {
        const venta = await obtenerVenta(id);
        document.getElementById('modal-venta-titulo').textContent = 'Cambiar Estado de Venta';
        document.getElementById('venta-id').value = venta.Id || venta.id;
        document.getElementById('venta-estado').value = String(venta.Estado ?? venta.estado ?? 1);
        setCamposVentaModo(false);
        modalVenta.style.display = 'block';
    } catch (error) {
        alert('Error al cargar venta: ' + error.message);
    }
}

async function eliminarVenta(id) {
    if (confirm('¿Estás seguro de eliminar esta venta?')) {
        try {
            await eliminarVentaApi(id);
            alert('Venta eliminada');
            cargarVentas();
        } catch (error) {
            alert('Error al eliminar venta');
        }
    }
}

async function eliminarVentaApi(id) {
    return fetchAPI(`${API_ADMIN}/ventas/${id}`, {
        method: 'DELETE'
    });
}

// Reportes
document.getElementById('tipo-reporte').addEventListener('change', (e) => {
    const idInput = document.getElementById('grupo-id');
    if (e.target.value === 'periodo') {
        idInput.style.display = 'none';
    } else {
        idInput.style.display = 'block';
    }
});

document.getElementById('btn-generar-reporte').addEventListener('click', async () => {
    const tipo = document.getElementById('tipo-reporte').value;
    const desde = document.getElementById('reporte-desde').value;
    const hasta = document.getElementById('reporte-hasta').value;
    const id = document.getElementById('reporte-id').value;
    
    if (!desde || !hasta) {
        alert('Por favor selecciona las fechas');
        return;
    }
    
    if (tipo !== 'periodo' && !id) {
        alert('Por favor ingresa el ID');
        return;
    }
    
    try {
        let reporte;
        switch(tipo) {
            case 'periodo':
                reporte = await obtenerReportePeriodo(desde, hasta);
                break;
            case 'producto':
                reporte = await obtenerReporteProducto(id, desde, hasta);
                break;
            case 'cliente':
                reporte = await obtenerReporteCliente(id, desde, hasta);
                break;
            case 'proveedor':
                reporte = await obtenerReporteProveedor(id, desde, hasta);
                break;
        }
        
        mostrarReporte(reporte, tipo);
    } catch (error) {
        alert('Error al generar reporte: ' + error.message);
        console.error('Error detallado:', error);
    }
});

function mostrarReporte(reporte, tipo) {
    const container = document.getElementById('resultado-reporte');

    if (!Array.isArray(reporte)) {
        container.innerHTML = '<p>No se encontraron resultados para el reporte.</p>';
        return;
    }

    if (tipo === 'periodo') {
        container.innerHTML = `
            <div class="item-row item-row-header">
                <div>ID</div>
                <div>Fecha</div>
                <div>Total</div>
                <div>Estado</div>
            </div>
            ${reporte.map(r => `
                <div class="item-row">
                    <div>${r.id}</div>
                    <div>${new Date(r.fecha).toLocaleDateString()}</div>
                    <div>$${r.total?.toFixed(2) || '0.00'}</div>
                    <div>${r.estado || '-'}</div>
                </div>
            `).join('')}
        `;
    } else {
        container.innerHTML = `
            <div class="item-row item-row-header">
                <div>ID</div>
                <div>Nombre</div>
                <div>Cantidad</div>
                <div>Total</div>
            </div>
            ${reporte.map(r => `
                <div class="item-row">
                    <div>${r.id}</div>
                    <div>${r.nombre || '-'}</div>
                    <div>${r.cantidad || 0}</div>
                    <div>$${r.total?.toFixed(2) || '0.00'}</div>
                </div>
            `).join('')}
        `;
    }
}

// Cerrar modales
document.querySelectorAll('.close').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.modal').forEach(m => m.style.display = 'none');
    });
});

window.addEventListener('click', (e) => {
    document.querySelectorAll('.modal').forEach(m => {
        if (e.target === m) m.style.display = 'none';
    });
});

// Cargar productos al inicio
document.addEventListener('DOMContentLoaded', () => {
    cargarProductos();
});
