
Stack=pentaho
Net=${Stack}_default
while docker network inspect $Net >/dev/null 2>&1; do sleep 1; done

docker-compose config | docker stack deploy -c - $Stack
