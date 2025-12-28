# Xu Passeios Travel Agency


```mermaid
architecture-beta

	group messagequeue[Message Queue]
	group backoffice(server)[Back Office]
	group common[General]
	group frontend[Portal Porta 3000]
	group catalog(server)[Catalog]
	group checkout[Checkout]
	
	service messagequeueService[Queue] in messagequeue
	service frontend2[Front End] in frontend

	service order(server)[Order] in backoffice
	service payment(server)[Payment] in backoffice
	service logistics(server)[Logistics] in backoffice
	service marketing(server)[Marketing] in backoffice
	service checkout2(server)[Checkout] in checkout
	service products(database)[Product] in catalog
	
	service message[Message] in common		

	frontend2{group}:T --> B:checkout2{group}

	message{group}:R -- L:order{group}
	message{group}:R -- L:logistics{group}
	message{group}:R -- L:marketing{group}
    message{group}:R -- L:payment{group}
	message{group}:R -- L:checkout2{group}
	messagequeueService{group}:B -- T:payment{group}
	messagequeueService{group}:B -- T:order{group}
	messagequeueService{group}:B -- T:logistics{group}
	messagequeueService{group}:B -- T:marketing{group}

	

	checkout2{group}:T --> B:messagequeueService{group}
	products{group}:R --> L:frontend2{group}

```

<details>

<summary>Other(s)</summary>

<details>

<summary>How to execute</summary>

1. `cd XuPasseios`   
2. `docker-compose up`   
 

</details>

<details>

<summary>Reference(s)</summary>

[How to use certificates with Docker Container](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-10.0)   
[How to use certificates with Docker Compose](https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-10.0)   
[Set up Docker with TLS](https://www.labkey.org/Documentation/wiki-page.view?name=dockerTLS)  

</details>

</details>