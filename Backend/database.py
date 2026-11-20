import json
from pathlib import Path

DATA_DIR = Path(__file__).resolve().parent / "data"
PRODUCTOS_FILE = DATA_DIR / "productos.json"
VENTAS_FILE = DATA_DIR / "ventas.json"


def inicializar_datos():
    DATA_DIR.mkdir(parents=True, exist_ok=True)

    if not PRODUCTOS_FILE.exists():
        productos_iniciales = [
            {"id": 1, "nombre": "Manzanas", "categoria": "Fruta", "precioKg": 2.50, "stockKg": 30.0},
            {"id": 2, "nombre": "Plátanos", "categoria": "Fruta", "precioKg": 1.80, "stockKg": 25.0},
            {"id": 3, "nombre": "Naranjas", "categoria": "Fruta", "precioKg": 2.00, "stockKg": 40.0},
            {"id": 4, "nombre": "Zanahorias", "categoria": "Verdura", "precioKg": 1.40, "stockKg": 18.0},
            {"id": 5, "nombre": "Tomates", "categoria": "Verdura", "precioKg": 2.80, "stockKg": 20.0}
        ]
        guardar_json(PRODUCTOS_FILE, productos_iniciales)

    if not VENTAS_FILE.exists():
        guardar_json(VENTAS_FILE, [])


def leer_json(ruta):
    inicializar_directorio()
    with ruta.open("r", encoding="utf-8-sig") as archivo:
        contenido = archivo.read().strip()
        # Los ficheros vacíos no provocan un HTTP 500 al consultar la API.
        return json.loads(contenido) if contenido else []


def inicializar_directorio():
    DATA_DIR.mkdir(parents=True, exist_ok=True)


def guardar_json(ruta, datos):
    inicializar_directorio()
    with ruta.open("w", encoding="utf-8") as archivo:
        json.dump(datos, archivo, ensure_ascii=False, indent=4)


def obtener_productos():
    inicializar_datos()
    return leer_json(PRODUCTOS_FILE)


def guardar_productos(productos):
    guardar_json(PRODUCTOS_FILE, productos)


def obtener_ventas():
    inicializar_datos()
    return leer_json(VENTAS_FILE)


def guardar_ventas(ventas):
    guardar_json(VENTAS_FILE, ventas)
