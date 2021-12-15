
Deploy with: https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-docker-cli?tabs=azure-cli


docker build . -t adventofqode.azurecr.io/advent-of-qode-server
docker push adventofqode.azurecr.io/advent-of-qode-server
