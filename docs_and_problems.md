# Docker
We have a docker compose in the root folder that contains 6 containers:
- **sql-server** -> the sql server for all the databases
- **zookeeper** -> handles the kafka cluster
- **kafka** -> initializes kafka message broker
- **identity-api** -> runs the image of the *identity* service which is used to authorize the User
- **registy-api** -> runs the image of the *registry* service which is used to subscribe to particular Events
- **eventengine-api** -> runs the image of the *eventengine* service which is used to dispatch the Events to the Urls provided in the correspondig subscriptions

#### Problems
- The *sql-service* took a while to start and the other services couldn't find it, so when the other services started they immediately crashed trying to create de databases, so I had to implement a workaround with a *network* to make them communicate
- The name of the network is still `registry-net`, I forgot to change that
- I had problems with *kafka* so I also implemented an healthcheck with a retry mechanism to be sure that the service started before doing anything
- Not strictly a Docker problem but I have a folder `sql-scripts` that I planned to use to startup the database, however I discovered that SQL Server doesn't work that way, then I forgot to delete it

## Dockerfile
> Basically I want to separate the heavy building process by doing it in the *image creation* from the launch of the application. In this way in the `docker-compose` I just utilize the images to launch the corresponding containers.
I have a **Dockerfile** for each service. Basically what they all do is the following:
1. Copy the projects files (`.csproj`)
2. Restore the dependencies, it downloads all the necessary ones
3. Copy all the source files of the project
4. Build and publish
5. Launch the application

# LocalNuGet
> For the synchronous communication of `RegistryService` with the `IdentityService` I used a **NuGet local package**. In this way I basically made it into a library and when I build the project the dependencies are searched in the nuget repo online but also in `./LocalNuGet`.

# Services
## dentityService
> It is used by the the **RegistryService** to *authorize* the subscription requests, in this way we separate the authorization from the actual logic.

#### Problems
- The main problem is *architectural* because I implemented weak security and coupling between services to match the project requirement of having at least a synchronous communication between microservices. I would have used an asynchronous approach with *Kafka* and a replicated table of Users, in the **RegistryDb** without the token, but with the information `IsAuthorized`, in this way the Registry doesn't know the apitokens but if the **IdentityService** goes down the users can still make subscriptions.
- A consistent problem in my services is the absence of a **centralized Exception Handling**. I can implement it by adding a class that handles exceptions like `GlobalExceptionHandler()` and registering as a service with `app.addService.AddExceptionHandler<GlobalExceptionHandler>()` and registering it as the *first middleware* in `Program.cs` with the following `app.UseExceptionHandler()`. Doing so I can remove the try-catch blocks inside my code
- I left the `GenerateSecureHash()` function empty on purpose but I will need to implement it  

## RegistryService
> It is used by the clients to make *CRUD operations* on the **Subscriptions**.

#### Problems
- I should be more consisten with the names: the service is called **RegistryService** (both as folder and in docker-compose), 
the db instance is called **RegistryDB**, the context **SubscriptionDbContext**, but the subscription table is called **Subscription**
- In `SubscriptionDTo` I didn't put `[JsonIgnore]` in the field `DeletedAt` and I used as a default value `DateTime.UtcNow`
- In the `SubscriptionController` I should remove the manual authorization and implement a middleware that does that for all requests, in this way I can be transparent and apply the authorization in a more professional way, also the Controller wouldn't have to use the `IdentityClientHttp`.
- The authorization process should be like this: client calls `/subscribe` passing just the payload of the subscription -> the middleware calls IdentityService and creates an entry and a token, and returns {userId, Token} to the middleware -> the middleware sets the user id in the context (to make it accessible from the controller) an sets a custom response header `X-New-Authorization-Token` -> the controller does its things and returns -> in the next call the subscription is done by putting the token in the `Authrization` header of the request.


