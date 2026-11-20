from datetime import date, datetime

from fastapi import FastAPI, HTTPException

from database import (
    guardar_productos,
    guardar_ventas,
    inicializar_datos,
    obtener_productos,
    obtener_ventas,
)
from models import Producto, ProductoActualizar, ProductoCrear, Venta, VentaCrear

app = FastAPI(
    title="API - Gestión de Frutería",
    description="API para gestionar productos, stock y ventas de una frutería",
    version="1.0.0",
)


@app.on_event("startup")
def iniciar():
    inicializar_datos()


@app.get("/")
def inicio():
    return {"mensaje": "API de la frutería funcionando correctamente"}


@app.get("/productos", response_model=list[Producto])
def listar_productos():
    return obtener_productos()


@app.post("/productos", response_model=Producto, status_code=201)
def crear_producto(producto: ProductoCrear):
    productos = obtener_productos()
    nuevo = {"id": max((p["id"] for p in productos), default=0) + 1, **producto.model_dump()}
    productos.append(nuevo)
    guardar_productos(productos)
    return nuevo


@app.put("/productos/{producto_id}", response_model=Producto)
def actualizar_producto(producto_id: int, datos: ProductoActualizar):
    productos = obtener_productos()
    for indice, producto in enumerate(productos):
        if producto["id"] == producto_id:
            actualizado = {"id": producto_id, **datos.model_dump()}
            productos[indice] = actualizado
            guardar_productos(productos)
            return actualizado
    raise HTTPException(status_code=404, detail="Producto no encontrado")


@app.delete("/productos/{producto_id}")
def eliminar_producto(producto_id: int):
    productos = obtener_productos()
    if not any(p["id"] == producto_id for p in productos):
        raise HTTPException(status_code=404, detail="Producto no encontrado")
    guardar_productos([p for p in productos if p["id"] != producto_id])
    return {"mensaje": "Producto eliminado correctamente"}


@app.get("/ventas", response_model=list[Venta])
def listar_ventas():
    return obtener_ventas()


@app.post("/ventas", response_model=Venta, status_code=201)
def registrar_venta(datos: VentaCrear):
    productos = obtener_productos()
    ventas = obtener_ventas()

    # Agrupa productos repetidos para validar y descontar su cantidad total.
    kilos_por_id = {}
    for linea in datos.productos:
        kilos_por_id[linea.productoId] = kilos_por_id.get(linea.productoId, 0) + linea.kilos

    detalles = []
    for producto_id, kilos in kilos_por_id.items():
        producto = next((p for p in productos if p["id"] == producto_id), None)
        if producto is None:
            raise HTTPException(status_code=404, detail=f"No existe el producto con ID {producto_id}")
        if producto["stockKg"] < kilos:
            raise HTTPException(
                status_code=400,
                detail=f"Stock insuficiente para {producto['nombre']}. Disponible: {producto['stockKg']} kg",
            )
        detalles.append({
            "productoId": producto["id"],
            "nombreProducto": producto["nombre"],
            "kilos": kilos,
            "precioKg": producto["precioKg"],
            "subtotal": round(producto["precioKg"] * kilos, 2),
        })

    for detalle in detalles:
        producto = next(p for p in productos if p["id"] == detalle["productoId"])
        producto["stockKg"] = round(producto["stockKg"] - detalle["kilos"], 3)

    venta = {
        "id": max((v["id"] for v in ventas), default=0) + 1,
        "fecha": datetime.now().astimezone().isoformat(timespec="seconds"),
        "productos": detalles,
        "total": round(sum(d["subtotal"] for d in detalles), 2),
    }
    guardar_productos(productos)
    ventas.append(venta)
    guardar_ventas(ventas)
    return venta


@app.get("/informes/ventas-hoy")
def informe_ventas_hoy():
    hoy = date.today()
    ventas_hoy = [
        venta for venta in obtener_ventas()
        if datetime.fromisoformat(venta["fecha"]).astimezone().date() == hoy
    ]
    total_vendido = round(sum(v["total"] for v in ventas_hoy), 2)
    kilos_por_producto = {}
    for venta in ventas_hoy:
        for detalle in venta["productos"]:
            nombre = detalle["nombreProducto"]
            kilos_por_producto[nombre] = round(
                kilos_por_producto.get(nombre, 0) + detalle["kilos"], 3
            )

    mas_vendido = max(kilos_por_producto, key=kilos_por_producto.get) if kilos_por_producto else "Sin ventas"
    return {
        "totalVendido": total_vendido,
        "numeroVentas": len(ventas_hoy),
        "productoMasVendido": mas_vendido,
        "kilosVendidos": kilos_por_producto.get(mas_vendido, 0),
        "kilosPorProducto": kilos_por_producto,
    }
