#!/bin/bash

for file in *.yaml; do
  if [ -f "$file" ]; then
    folder_name="${file%.yaml}"
    mkdir -p "$folder_name"
    mv "$file" "$folder_name/"
    cd "$folder_name"
    ../services validate "$file"
    ../services markdown "$file"
    ../services ngsi "$file"
    cd ..
  fi
done


