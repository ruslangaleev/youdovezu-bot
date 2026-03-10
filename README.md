чтобы локально запустить фронт проект:
- заходим в папку /frontend
- выполняем npm i
- выполняем npm run start:testing. Проект запуститься на localhost:3000 

docker-compose.yml нужен только для локального запуска и  отладки с фронтом
docker-compose.prod.yml нужен для боевого запуска где еще и описан nginx и frontend