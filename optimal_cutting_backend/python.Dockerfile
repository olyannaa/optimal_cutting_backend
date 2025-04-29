FROM python:3.11

EXPOSE 8000

WORKDIR /code

COPY ./optimal_cutting_backend/requirements.txt /code/requirements.txt

RUN pip install --upgrade pip
RUN pip install -r /code/requirements.txt
RUN pip install "fastapi[standard]"
RUN pip install python-multipart

COPY ./optimal_cutting_backend/parser_service.py /code/app/parser_service.py

CMD ["fastapi", "run", "app/parser_service.py", "--port", "8000"]