from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import JSONResponse
import ezdxf
import io
import uvicorn
import logging
from typing import List, Dict

app = FastAPI()

# Настройка логирования
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

import tempfile

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


        figures = []

        # 1. Пытаемся взять объекты из ModelSpace
        msp = doc.modelspace()
        modelspace_entities = list(msp)
        if modelspace_entities:
            logger.info(f"Найдено {len(modelspace_entities)} объектов в ModelSpace")
            for entity in modelspace_entities:
                try:
                    figure = process_entity(entity)
                    if figure:
                        figures.append(figure)
                except Exception as e:
                    logger.warning(f"Ошибка обработки объекта: {str(e)}")
                    continue

        # 2. Если в ModelSpace ничего нет — смотрим в Layouts
        if not figures:
            logger.info("ModelSpace пустой, проверяем Layouts...")
            for layout in doc.layouts:
                layout_entities = list(layout)
                if layout_entities:
                    logger.info(f"Найдено {len(layout_entities)} объектов в Layout: {layout.name}")
                    for entity in layout_entities:
                        try:
                            figure = process_entity(entity)
                            if figure:
                                figures.append(figure)
                        except Exception as e:
                            logger.warning(f"Ошибка обработки объекта в Layout: {str(e)}")
                            continue

        # 3. Если и в Layouts ничего нет — смотрим в Blocks
        if not figures:
            logger.info("Layouts пустые, проверяем Blocks...")
            for block in doc.blocks:
                block_entities = list(block)
                if block_entities and block.name not in ("*Model_Space", "*Paper_Space"):
                    logger.info(f"Найдено {len(block_entities)} объектов в Block: {block.name}")
                    for entity in block_entities:
                        try:
                            figure = process_entity(entity)
                            if figure:
                                figures.append(figure)
                        except Exception as e:
                            logger.warning(f"Ошибка обработки объекта в Block: {str(e)}")
                            continue

        if not figures:
            raise ValueError("Не удалось извлечь ни одного объекта из DXF")

        logger.info(f"Успешно обработано объектов: {len(figures)}")
        return figures

    except Exception as e:
        logger.error(f"Ошибка при обработке файла: {str(e)}")
        raise



def process_entity(entity) -> Dict[str, object]:
    """Обрабатывает отдельную сущность DXF"""
    if not hasattr(entity, 'dxftype'):
        return None

    entity_type = entity.dxftype()
    coords = []

    if entity_type == 'LINE':
        coords = [
            format_coord(entity.dxf.start.x),
            format_coord(entity.dxf.start.y),
            format_coord(entity.dxf.end.x),
            format_coord(entity.dxf.end.y)
        ]
        return {'type': 1, 'coordinates': ";".join(coords)}

    elif entity_type == 'CIRCLE':
        coords = [
            format_coord(entity.dxf.center.x),
            format_coord(entity.dxf.center.y),
            format_coord(entity.dxf.radius)
        ]
        return {'type': 2, 'coordinates': ";".join(coords)}

    elif entity_type == 'ARC':
        coords = [
            format_coord(entity.dxf.center.x),
            format_coord(entity.dxf.center.y),
            format_coord(entity.dxf.radius),
            format_coord(entity.dxf.start_angle),
            format_coord(entity.dxf.end_angle)
        ]
        return {'type': 3, 'coordinates': ";".join(coords)}

    elif entity_type in ['SPLINE', 'POLYLINE', 'LWPOLYLINE']:
        points = []
        if hasattr(entity, 'control_points'):
            points = entity.control_points
        elif hasattr(entity, 'points'):
            points = entity.points

        coords = [
            f"{format_coord(p[0])};{format_coord(p[1])}"
            for p in points
            if len(p) >= 2
        ]
        return {'type': 4, 'coordinates': "/".join(coords)}

    return None

@app.post("/parse-dxf")
async def parse_dxf_endpoint(file: UploadFile = File(...)):
    try:
        result = await parse_dxf_file(file)
        return JSONResponse(content=result)
    except Exception as e:
        raise HTTPException(
            status_code=400,
            detail=f"Ошибка обработки DXF: {str(e)}"
        )

if __name__ == "__main__":
    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8000,
        log_level="info"
    )
