*** Run this app in docker ***  
- In terminal:  
cd SM-POST  
docker-compose up --build -d

- In Docker  
  Images will be created:  
  - sm-post_command-api  
  - sm-post_query-api  
  - sm-post_ocelot-gateway  

- In Docker  
  Containers will be created:  
  - sm-post



*** Connections ***  
MongoDB → localhost:27017

SQL Server → localhost, 1405  
   login: sa  
   password: StrongP@ssw0rd!

Kafka → localhost:9092



*** Test it ***  
(SQL server takes 2-3 minutes to become available; wait, then stop and restart sm-post_query-api manually)

1. Direct access (bypassing Ocelot, for testing):  
Command API (Add a Post):  
- In terminal:  
Invoke-WebRequest -Uri "http://localhost:5010/api/v1/NewPost" `

  -Method POST `
  
  -ContentType "application/json" `
  
  -Body '{"author":"John Doe","Message":"Hello"}'

Query API (Get all Posts):  
- In terminal:  
Invoke-WebRequest -Uri "http://localhost:5011/api/v1/PostLookup" `  
  -Method GET


2. With Ocelot Gateway → http://localhost:7006  
Command API (Add a Post):  
- In terminal:  
Invoke-WebRequest -Uri "http://localhost:7006/commands/NewPost" `

  -Method POST `
  
  -ContentType "application/json" `
  
  -Body '{"author": "Steve Johnson", "Message": "Hello 2"}'

Query API (Get all Posts):  
- In terminal:  
Invoke-WebRequest -Uri "http://localhost:7006/queries/PostLookup" `  
  -Method GET








