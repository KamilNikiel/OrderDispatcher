# Order Dispatcher 

## Prerequisites

* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Ensure it is installed and running)

## How to Run

The entire environment (Workers, Dashboard, RabbitMQ, and SQL Server) is fully containerized.

1. Open your terminal and navigate to the root directory (where the `docker-compose.yml` file is located).
2. Run the following command to build the images and spin up the containers in the background:

   ```bash
   docker-compose up --build -d
