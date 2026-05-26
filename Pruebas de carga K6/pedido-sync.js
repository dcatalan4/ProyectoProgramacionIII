import http from 'k6/http';
import { check } from 'k6';

const clientes = open('./clientes.txt')
  .split('\n')
  .map(x => x.replace(/\r/g, '').replace(/\uFEFF/g, '').trim())
  .filter(x => x && x.length === 36);

function esGuid(val) {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(val);
}

export let options = {
  vus: 20,
  duration: '180s',
};

export default function () {

  let index = Math.floor(Math.random() * clientes.length);
  let clienteId = clientes[index];

  if (!clienteId) {
    console.error('clienteId undefined en índice:', index);
    return;
  }

  if (!esGuid(clienteId)) {
    console.error('clienteId inválido:', clienteId);
    return;
  }

  let carritoRes = http.post(
    'http://localhost:5180/api/carrito',
    JSON.stringify({ clienteId }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  if (carritoRes.status !== 200 || !carritoRes.body) {
    console.error('Error creando carrito:', carritoRes.status, carritoRes.body);
    return;
  }

  let carrito;
  try {
    carrito = JSON.parse(carritoRes.body);
  } catch (e) {
    console.error('Error parseando carrito:', carritoRes.body);
    return;
  }

  if (!carrito.id || carrito.id === "00000000-0000-0000-0000-000000000000") {
    console.error('ID de carrito inválido:', carrito);
    return;
  }

  let productoId = "55555555-5555-5555-5555-555555555555";

  let itemRes = http.post(
    'http://localhost:5180/api/carrito/items',
    JSON.stringify({
      carritoId: carrito.id,
      productoId,
      cantidad: 1
    }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  if (itemRes.status !== 200) {
    console.error('Error agregando item:', itemRes.status, itemRes.body);
    return;
  }

  let pedidoRes = http.post(
    'http://localhost:5180/api/pedidos/sincrono',
    JSON.stringify({ carritoId: carrito.id }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  let ok = check(pedidoRes, {
    'status 200': (r) => r.status === 200,

    'respuesta no vacía': (r) => r.body && r.body.length > 0,

    'tiene ventaId': (r) => {
      try {
        let body = r.json();
        return body.id !== undefined || body.ventaId !== undefined;
      } catch (e) {
        return false;
      }
    }
  });

  if (!ok) {
    console.error('Error en pedido síncrono:', pedidoRes.status, pedidoRes.body);
  }

}