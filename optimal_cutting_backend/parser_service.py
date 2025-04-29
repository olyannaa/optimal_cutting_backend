from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import JSONResponse
import ezdxf
import tempfile
import logging
from typing import List, Dict
from math import cos, sin, radians

app = FastAPI()

# Логирование
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def format_coord(num: float) -> str:
    """Форматирует число с запятой вместо точки"""
    try:
        formatted = f"{float(num):.6f}".replace('.', ',').rstrip('0').rstrip(',')
        return formatted if formatted != '' else '0'
    except (TypeError, ValueError) as e:
        logger.warning(f"Ошибка форматирования координаты {num}: {str(e)}")
        return "0"

async def parse_dxf_file(file: UploadFile) -> List[Dict[str, object]]:
    try:
        logger.info(f"Начало обработки файла: {file.filename}")

        file_content = await file.read()
        if not file_content:
            raise ValueError("Получен пустой файл")

        with tempfile.NamedTemporaryFile(delete=False, suffix=".dxf") as tmp:
            tmp.write(file_content)
            tmp_path = tmp.name

        try:
            doc = ezdxf.readfile(tmp_path)
        except Exception as e:
            logger.error(f"Ошибка чтения DXF: {str(e)}")
            raise ValueError(f"Невозможно прочитать DXF файл: {str(e)}")
        finally:
            tmp.close()

        # --- Определяем единицы измерения ---
        units_code = doc.header.get('$INSUNITS', 0)
        units_map = {
            0: "Unspecified",
            1: "Inches",
            2: "Feet",
            4: "Millimeters",
            5: "Centimeters",
            6: "Meters",
        }
        unit_multipliers = {
            0: 1.0,
            1: 25.4,
            2: 304.8,
            4: 1.0,
            5: 10.0,
            6: 1000.0,
        }
        units = units_map.get(units_code, "Unknown")
        multiplier = unit_multipliers.get(units_code, 1.0)

        logger.info(f"Единицы DXF: {units} (код {units_code}), множитель: {multiplier}")

        figures = []
        all_x = []
        all_y = []

        sources = [
            list(doc.modelspace()),
            *(list(layout) for layout in doc.layouts),
            *(list(block) for block in doc.blocks if block.name not in ("*Model_Space", "*Paper_Space"))
        ]

        for source in sources:
            for entity in source:
                try:
                    figure, bounding_points = process_entity(entity, multiplier)
                    if figure:
                        figures.append(figure)

                        # Сбор координат для расчёта размеров
                        if figure['type'] in [1, 2]:
                            coords = figure['coordinates'].split(';')
                            for i in range(0, len(coords) - 1, 2):
                                all_x.append(float(coords[i].replace(',', '.')))
                                all_y.append(float(coords[i+1].replace(',', '.')))
                        elif figure['type'] == 3:
                            coords = figure['coordinates'].split(';')
                            cx = float(coords[0].replace(',', '.'))
                            cy = float(coords[1].replace(',', '.'))
                            all_x.append(cx)
                            all_y.append(cy)

                            if bounding_points:
                                all_x.append(bounding_points[0])
                                all_y.append(bounding_points[1])
                                all_x.append(bounding_points[2])
                                all_y.append(bounding_points[3])
                        elif figure['type'] == 4:
                            points = figure['coordinates'].split('/')
                            for point in points:
                                parts = point.split(';')
                                if len(parts) >= 2:
                                    all_x.append(float(parts[0].replace(',', '.')))
                                    all_y.append(float(parts[1].replace(',', '.')))
                except Exception as e:
                    logger.warning(f"Ошибка обработки объекта: {str(e)}")
            if figures:
                break

        if not figures:
            raise ValueError("Не удалось извлечь ни одного объекта из DXF")

        if not all_x or not all_y:
            raise ValueError("Нет координат для расчёта размеров детали")

        min_x = min(all_x)
        max_x = max(all_x)
        min_y = min(all_y)
        max_y = max(all_y)

        width = round(max_x - min_x, 3)
        height = round(max_y - min_y, 3)

        logger.info(f"Габариты детали: ширина={width} мм, высота={height} мм")

        return figures

    except Exception as e:
        logger.error(f"Ошибка при обработке файла: {str(e)}")
        raise

def process_entity(entity, multiplier=1.0) -> (Dict[str, object], List[float]):
    """Обрабатывает отдельную сущность DXF и возвращает фигуру и bounding points"""
    if not hasattr(entity, 'dxftype'):
        return None, None

    entity_type = entity.dxftype()

    def scale(num):
        return float(num) * multiplier

    if entity_type == 'LINE':
        coords = [
            format_coord(scale(entity.dxf.start.x)),
            format_coord(scale(entity.dxf.start.y)),
            format_coord(scale(entity.dxf.end.x)),
            format_coord(scale(entity.dxf.end.y))
        ]
        return {'type': 1, 'coordinates': ";".join(coords)}, None

    elif entity_type == 'CIRCLE':
        coords = [
            format_coord(scale(entity.dxf.center.x)),
            format_coord(scale(entity.dxf.center.y)),
            format_coord(scale(entity.dxf.radius))
        ]
        return {'type': 2, 'coordinates': ";".join(coords)}, None

    elif entity_type == 'ARC':
        cx = scale(entity.dxf.center.x)
        cy = scale(entity.dxf.center.y)
        r = scale(entity.dxf.radius)
        start_angle = entity.dxf.start_angle
        end_angle = entity.dxf.end_angle

        coords = [
            format_coord(cx),
            format_coord(cy),
            format_coord(r),
            format_coord(start_angle),
            format_coord(end_angle)
        ]

        start_rad = radians(start_angle)
        end_rad = radians(end_angle)

        start_x = cx + r * cos(start_rad)
        start_y = cy + r * sin(start_rad)
        end_x = cx + r * cos(end_rad)
        end_y = cy + r * sin(end_rad)

        return {'type': 3, 'coordinates': ";".join(coords)}, [start_x, start_y, end_x, end_y]

    elif entity_type in ['SPLINE', 'POLYLINE', 'LWPOLYLINE']:
        points = []
        if hasattr(entity, 'control_points'):
            points = entity.control_points
        elif hasattr(entity, 'points'):
            points = entity.points

        coords = [
            f"{format_coord(scale(p[0]))};{format_coord(scale(p[1]))}"
            for p in points
            if len(p) >= 2
        ]
        return {'type': 4, 'coordinates': "/".join(coords)}, None

    return None, None

@app.post("/parse-dxf")
async def parse_dxf_endpoint(file: UploadFile = File(...)):
    try:
        figures = await parse_dxf_file(file)
        return JSONResponse(content=figures)
    except Exception as e:
        raise HTTPException(
            status_code=400,
            detail=f"Ошибка обработки DXF: {str(e)}"
        )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8000,
        log_level="info"
    )
