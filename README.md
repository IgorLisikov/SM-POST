About this app
--
Domain - social media Posts and Comments  
The command-api project can be used to add and update Post and their Comments  
The query-api project can be used to query for Post and their Comments  
The ocelot-gateway project is an Api-gateway for both projects  

Workflow (assume some data already exists)
--
Edit message for a post via command-api project:
1. Request reaches EditMessageController  
2. Events are pulled from event store - MongoDB  
3. Events are replayed to build aggregate  
4. "Edit message" validation occurs on aggregate  
5. "MessageUpdatedEvent" is raised  
6. The event is stored in MongoDB  
7. The event is produced to Kafka topic  
8. query-api project has hosted service running that consumes the same Kafka topic  
9. The event is consumed by query-api project -> event handler is called  
10. Event handler calls repository to make updates to MS SQL database tables

Run this app in docker
---
```powershell
cd SM-POST  
```
```powershell
docker-compose up --build -d
```

- In Docker  
  Images will be created/pulled:  
  - sm-post_command-api  
  - sm-post_query-api  
  - sm-post_ocelot-gateway  
  - kafka:3.5  
  - zookeeper:3.9  
  - mongo:latest  
  - sql-server:2017-latest-ubuntu

- In Docker  
  Containers will be created:  
  - sm-post

Connections
---
MongoDB → localhost:27017

SQL Server → localhost, 1405  
   login: sa  
   password: StrongP@ssw0rd!

Kafka → localhost:9092

Test it
---
(SQL server takes 2-3 minutes to become available; wait, then stop and restart sm-post_query-api manually)

1. Direct access (bypassing Ocelot, for testing):

Command API (Add a Post):
```powershell
Invoke-WebRequest -Uri "http://localhost:5010/api/v1/NewPost" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"author":"John Doe","Message":"Hello"}'
```

Query API (Get all Posts):
```powershell
Invoke-WebRequest -Uri "http://localhost:5011/api/v1/PostLookup" `
  -Method GET
```
---
2. With Ocelot Gateway → http://localhost:7006

Command API (Add a Post):
```powershell
Invoke-WebRequest -Uri "http://localhost:7006/commands/NewPost" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"author": "Steve Johnson", "Message": "Hello 2"}'
```

Query API (Get all Posts):
```powershell
Invoke-WebRequest -Uri "http://localhost:7006/queries/PostLookup" `
  -Method GET
```







