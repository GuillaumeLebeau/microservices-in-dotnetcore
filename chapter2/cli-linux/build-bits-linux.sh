#!/bin/bash

declare -x path=$1

if [ -z "$path" ]; then 
    path="$(pwd)/../src";
fi

declare -a projectList=(
    "$path/ShoppingCart"
)

for project in "${projectList[@]}"
do
    echo -e "\e[33mWorking on $project"
    echo -e "\e[33m\tRemoving old publish output"
    pushd $project
    rm -rf obj/Docker/publish
    echo -e "\e[33m\tRestoring project"
    dotnet restore
    echo -e "\e[33m\tBuilding and publishing projects"
    dotnet publish -o obj/Docker/publish
    popd
done

## remove old docker images:
#images=$(docker images --filter=reference="eshop/*" -q)
#if [ -n "$images" ]; then
#    docker rm $(docker ps -a -q) -f
#    echo "Deleting eShop images in local Docker repo"
#    echo $images
#    docker rmi $(docker images --filter=reference="eshop/*" -q) -f
#fi


# No need to build the images, docker build or docker compose will
# do that using the images and containers defined in the docker-compose.yml file.