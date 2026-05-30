let productos = [];
let categoriaSeleccionada = '';
let clientesCompra = [];

function formatearQuetzales(valor) {
    return 'Q' + Number(valor || 0).toFixed(2);
}

function obtenerNombreCliente(cliente) {
    return `${cliente.Nombre || cliente.nombre || ''} ${cliente.Apellido || cliente.apellido || ''}`.trim();
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

function guardarClienteSeleccionado(clienteId) {
    const cliente = clientesCompra.find(item => (item.Id || item.id) === clienteId);
    const carrito = obtenerCarritoLocal();

    if (!cliente) {
        carrito.clienteId = clienteId || null;
        carrito.cliente = null;
        guardarCarritoLocal(carrito);
        return;
    }

    carrito.clienteId = clienteId;
    carrito.cliente = toClienteLocal(cliente);
    guardarCarritoLocal(carrito);
}

async function cargarCatalogo() {
    try {
        const response = await obtenerCatalogo(1, 100, categoriaSeleccionada || null);
        productos = response.items || [];
        renderizarCatalogo();
    } catch (error) {
        console.error('Error al cargar catálogo:', error);
    }
}

async function cargarClientesCompra() {
    try {
        clientesCompra = await obtenerClientes();
        const select = document.getElementById('cliente-compra');
        select.innerHTML = '<option value="">Selecciona un cliente</option>';

        clientesCompra.forEach(cliente => {
            const option = document.createElement('option');
            option.value = cliente.Id || cliente.id;
            option.textContent = obtenerNombreCliente(cliente);
            select.appendChild(option);
        });

        const carrito = obtenerCarritoLocal();
        if (carrito.clienteId) {
            select.value = carrito.clienteId;
            if (!carrito.cliente) {
                guardarClienteSeleccionado(carrito.clienteId);
            }
        }
    } catch (error) {
        console.error('Error al cargar clientes:', error);
    }
}

async function crearClienteDesdeCatalogo() {
    const cliente = {
        Nombre: document.getElementById('cliente-nombre').value.trim(),
        Apellido: document.getElementById('cliente-apellido').value.trim(),
        Email: document.getElementById('cliente-email').value.trim(),
        Telefono: document.getElementById('cliente-telefono').value.trim(),
        DireccionEnvio: document.getElementById('cliente-direccion').value.trim()
    };

    if (!cliente.Nombre || !cliente.Apellido || !cliente.Email) {
        alert('Ingresa nombre, apellido y email del cliente');
        return;
    }

    try {
        const nuevoCliente = await crearCliente(cliente);
        await cargarClientesCompra();
        document.getElementById('cliente-compra').value = nuevoCliente.Id || nuevoCliente.id;
        guardarCarritoLocal({
            id: null,
            clienteId: nuevoCliente.Id || nuevoCliente.id,
            cliente: toClienteLocal(nuevoCliente),
            items: []
        });
        document.querySelectorAll('.cliente-rapido input').forEach(input => input.value = '');
        alert('Cliente creado y seleccionado');
    } catch (error) {
        console.error('Error al crear cliente:', error);
        alert(error.message || 'Error al crear cliente');
    }
}

function renderizarCatalogo() {
    const grid = document.getElementById('catalogo-grid');
    grid.innerHTML = '';
    
    productos.forEach(p => {
        const card = document.createElement('div');
        card.className = 'producto-card';
        card.innerHTML = `
            <h3>${p.nombre}</h3>
            <p>${p.descripcion || 'Sin descripción'}</p>
            <p>Stock: ${p.stock}</p>
            <div class="precio">${formatearQuetzales(p.precioVenta)}</div>
            <button onclick="agregarAlCarrito('${p.id}')" class="btn btn-primary">Agregar al Carrito</button>
        `;
        grid.appendChild(card);
    });
}

async function agregarAlCarrito(productoId) {
    try {
        const clienteId = document.getElementById('cliente-compra').value;
        if (!clienteId) {
            alert('Selecciona un cliente antes de agregar productos al carrito');
            return;
        }

        let carrito = obtenerCarritoLocal();

        if (carrito.id && carrito.clienteId && carrito.clienteId !== clienteId) {
            carrito = { id: null, clienteId: clienteId, cliente: null, items: [] };
        }

        const clienteSeleccionado = clientesCompra.find(cliente => (cliente.Id || cliente.id) === clienteId);
        if (clienteSeleccionado) {
            carrito.cliente = toClienteLocal(clienteSeleccionado);
        }
        
        if (!carrito.id) {
            const nuevoCarrito = await crearCarrito(clienteId);
            carrito.id = nuevoCarrito.id;
            carrito.clienteId = nuevoCarrito.clienteId;
            carrito.items = [];
        }
        
        const itemExistente = carrito.items.find(i => i.productoId === productoId);
        if (itemExistente) {
            itemExistente.cantidad++;
        } else {
            carrito.items.push({ productoId, cantidad: 1 });
        }

        await agregarItemCarrito(carrito.id, productoId, 1);
        guardarCarritoLocal(carrito);
        alert('Producto agregado al carrito');
    } catch (error) {
        console.error('Error al agregar al carrito:', error);
        const mensaje = error.message?.includes('Error HTTP')
            ? error.message.replace(/^Error HTTP \d+: /, '')
            : 'Error al agregar al carrito';
        alert(mensaje);
    }
}

document.getElementById('filtro-categoria').addEventListener('change', (e) => {
    categoriaSeleccionada = e.target.value;
    cargarCatalogo();
});

document.getElementById('cliente-compra').addEventListener('change', (e) => {
    const carrito = obtenerCarritoLocal();
    if (carrito.id && carrito.clienteId && carrito.clienteId !== e.target.value) {
        guardarCarritoLocal({ id: null, clienteId: null, cliente: null, items: [] });
    }

    guardarClienteSeleccionado(e.target.value);
});

document.getElementById('btn-crear-cliente-compra').addEventListener('click', crearClienteDesdeCatalogo);

document.getElementById('busqueda').addEventListener('input', (e) => {
    const busqueda = e.target.value.toLowerCase();
    const grid = document.getElementById('catalogo-grid');
    grid.innerHTML = '';
    
    const productosFiltrados = productos.filter(p => 
        p.nombre.toLowerCase().includes(busqueda) || 
        (p.descripcion && p.descripcion.toLowerCase().includes(busqueda))
    );
    
    productosFiltrados.forEach(p => {
        const card = document.createElement('div');
        card.className = 'producto-card';
        card.innerHTML = `
            <h3>${p.nombre}</h3>
            <p>${p.descripcion || 'Sin descripción'}</p>
            <p>Stock: ${p.stock}</p>
            <div class="precio">${formatearQuetzales(p.precioVenta)}</div>
            <button onclick="agregarAlCarrito('${p.id}')" class="btn btn-primary">Agregar al Carrito</button>
        `;
        grid.appendChild(card);
    });
});

document.addEventListener('DOMContentLoaded', async () => {
    await cargarClientesCompra();
    await cargarCatalogo();
});
