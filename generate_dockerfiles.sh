#!/usr/bin/env bash

TEMPLATE_FILE=Dockerfile.template

if [ ! -f "$TEMPLATE_FILE" ]; then
    echo "Error: template file '$TEMPLATE_FILE' not found"
    exit 1
fi

function generateFile {
    local concrete_file="Dockerfile-$1"

    if [ -f "$concrete_file" ]; then
        echo "File $concrete_file exists, recreating..."
        rm "$concrete_file"
    fi

    sed "s/{{PROJECT}}/$1/g" $TEMPLATE_FILE > "$concrete_file"
    echo "$concrete_file created"
}

#====

generateFile Producer && \
generateFile Consumer
