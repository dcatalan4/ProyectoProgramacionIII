let productos = [];
let categoriaSeleccionada = '';

async function cargarCatalogo() {
    try {
        const response = await obtenerCatalogo(1, 100, categoriaSeleccionada || null);
        productos = response.items || [];
        renderizarCatalogo();
    } catch (error) {
        console.error('Error al cargar catálogo:', error);
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
            <div class="precio">$${p.precioVenta.toFixed(2)}</div>
            <button onclick="agregarAlCarrito('${p.id}')" class="btn btn-primary">Agregar al Carrito</button>
        `;
        grid.appendChild(card);
    });
}

async function agregarAlCarrito(productoId) {
    try {
        let carrito = obtenerCarritoLocal();
        
        if (!carrito.id) {
            const nuevoCarrito = await crearCarrito();
            carrito.id = nuevoCarrito.id;
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
        alert('Error al agregar al carrito');
    }
}

document.getElementById('filtro-categoria').addEventListener('change', (e) => {
    categoriaSeleccionada = e.target.value;
    cargarCatalogo();
});

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
            <div class="precio">$${p.precioVenta.toFixed(2)}</div>
            <button onclick="agregarAlCarrito('${p.id}')" class="btn btn-primary">Agregar al Carrito</button>
        `;
        grid.appendChild(card);
    });
});

document.addEventListener('DOMContentLoaded', cargarCatalogo);
