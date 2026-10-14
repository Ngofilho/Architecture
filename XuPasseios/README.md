# Xu Passeios Travel Agency


```mermaid
architecture-beta
	group frontend[Portal Porta 3000]
	service frontend2(internet)[Front End] in frontend

	group catalog(server)[Catalog]
	service products(database)[Product] in catalog	

	group checkout[Checkout]
	service checkout2(server)[Checkout] in checkout

	group cart[Cart]
	service cart2(server)[Cart] in cart		
	
	group backoffice(server)[Back Office]
	service order(server)[Order] in backoffice
	service payment(server)[Payment] in backoffice
	service logistics(server)[Logistics] in backoffice
	service marketing(server)[Marketing] in backoffice
	service queue(disk)[Message Queue] in backoffice

	frontend2:R <-- L:products
	frontend2:R <--> L:checkout2
	frontend2:R -- L:cart2
	checkout2:R --> L:queue
	queue:B -- T:order
	queue:B -- T:payment
	queue:B -- T:logistics
	queue:B -- T:marketing

	align column products checkout2 cart2
	align row	frontend2 checkout2 queue

	align row order payment logistics marketing
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
