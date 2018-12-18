# Starting and working with the Pentaho analytics server

## Start Server

The image provides the ability to run in two main modes:
- docker compose service;
- docker swarm service

The `docker-compose` launch mode for multi-container applications provides a convenient launch of an image on a single server, but for a number of Linux distributions it requires the` docker-compose` pre-installation,
because the command is not part of the standard docker command set.
The `docker-compose` installation is described on the page [http://docker.crank.ru/docs/docker-compose/install-compose/] (http: //docker.crank.ru/docs/docker-compose/install- compose /).

The startup mode in cluster mode provides a convenient launch of an image on a cluster of servers (including on a cluster from a single server), but requires preliminary cluster initialization.
Cluster initialization is described at [https://docs.docker.com/engine/reference/commandline/swarm_init/] (http: //docs.docker.com/engine/reference/commandline/swarm_init/).

Both startup modes use the same configuration file `.env` and the same named volumes.
Using named volumes ensures that all settings and the current state of `pentaho` are saved when the container is restarted.


### Server TCP Port Configuration

The server configuration is described in the `.env` file.

The server uses the following TCP ports:
`` `
SERVER_HTTP_PORT = 8080
SERVER_AJP_PORT = 8009
SERVER_PROM_PORT = 1234
`` `

If these ports are already occupied in your system, you can redefine the specified variables in the `.env` file, specifying free ports.

### Getting the image

If you are running the image in `docker compose` mode, you need to first download the image from the repository with the` pull.sh` script:
```
$ pull.sh
```
If the image in the repository is updated to use it, you must re-run the command `pull.sh`.

When launching an image in `docker swarm` mode, this step can be skipped, since the image is downloaded automatically at the initial launch and
updating the image in the repository.

### Run in docker compose mode

 Running in docker compose mode is provided by the `composeStart.sh` docker script:
 `` `
 $ composeStart.sh
 `` `

 Initialization of the service occurs within 30-60 seconds.

 Stopping the service is provided by the `composeStop.sh` script:
 `` `
 $ composeStop.sh
 `` `

### Run in docker swarm mode

 Running in the docker swarm mode is provided by the docker script `swarmStart.sh`:
 `` `
 $ swarmStart.sh
 `` `

 Initialization of the service occurs within 30-60 seconds.

 Stopping the service is provided by the `swarmStop.sh` script:
 `` `
 $ swarmStop.sh
 ```
