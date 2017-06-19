#!bin/bash
set -e
dotnet restore
rm -rf $(pwd)/publish/shopping-cart
dotnet publish ShoppingCart/ShoppingCart.csproj -c Release -o $(pwd)/publish/shopping-cart