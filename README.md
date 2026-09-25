About this app
--
Domain - social media Posts and Comments  
The command-api project can be used to add and update Post and their Comments  
The query-api project can be used to query for Post and their Comments  
The ocelot-gateway project is an Api-gateway for both projects  

Workflow (assume some data already exists)
--
Edit message for a post via command-api project:
1. Request reaches EditMessageController of command-api project  
2. Events are pulled from event store - MongoDB  
3. Events are replayed to build aggregate  
4. "Edit message" validation occurs on aggregate  
5. "MessageUpdatedEvent" is raised  
6. The event record and outbox record are saved to MongoDB within transaction (Transactional Outbox) 
7. Background service (Outbox publisher) polls MongoDB for new outbox records every 5 seconds and publishes event to Kafka topic (at-least-once delivery)
8. Background service (Kafka consumer) running in query-api project consumes the same Kafka topic  
9. Event record is consumed by query-api project -> the changes are applied to corresponding MS SQL tables and ProcessedEvent table within transaction (Idempotent Consumer)

Run this app in docker
---
```powershell
cd SM-POST  
```
```powershell
docker-compose up -d
```

- In Docker  
  Images will be created/pulled:  
  - sm-post_command-api  
  - sm-post_query-api  
  - sm-post_ocelot-gateway  
  - confluentinc/cp-kafka:7.5.0  
  - confluentinc/cp-zookeeper:7.5.0  
  - mongo:7  
  - mssql/server:2019-latest  

- In Docker  
  Container group will be created:  
  - sm-post

Connections
---
MongoDB → localhost:27017

SQL Server → localhost, 1401  
   login: sa  
   password: StrongP@ssw0rd!

Kafka → localhost:9092

Test it
---
(SQL server takes 2-3 minutes to become available; other containers wait for it)

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







