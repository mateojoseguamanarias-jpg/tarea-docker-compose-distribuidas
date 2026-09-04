@echo off
title Sistema Distribuidos Vehiculos - Docker Compose
echo =========================================================================
echo Levantando RabbitMQ, Microservicio Categorias, Vehiculos y API Gateway...
echo =========================================================================
docker compose up --build
pause
