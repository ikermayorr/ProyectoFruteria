from pydantic import BaseModel, Field
from datetime import datetime


class Producto(BaseModel):
    id: int
    nombre: str
    categoria: str
    precioKg: float
    stockKg: float


class ProductoCrear(BaseModel):
    nombre: str = Field(min_length=1)
    categoria: str = Field(min_length=1)
    precioKg: float = Field(gt=0)
    stockKg: float = Field(ge=0)


class ProductoActualizar(BaseModel):
    nombre: str = Field(min_length=1)
    categoria: str = Field(min_length=1)
    precioKg: float = Field(gt=0)
    stockKg: float = Field(ge=0)


class DetalleVentaCrear(BaseModel):
    productoId: int
    kilos: float = Field(gt=0)


class VentaCrear(BaseModel):
    productos: list[DetalleVentaCrear] = Field(min_length=1)


class DetalleVenta(BaseModel):
    productoId: int
    nombreProducto: str
    kilos: float
    precioKg: float
    subtotal: float


class Venta(BaseModel):
    id: int
    fecha: datetime
    productos: list[DetalleVenta]
    total: float
