#!/usr/bin/env bash

BASE_DIR=./ProtoEntities
OUTPUT_DIR="${BASE_DIR}"/Generated/

function generateContractFor {
    local protoFile="${1}";
    local name;
    
    name=$(basename "${protoFile}" .proto)
    
    # generate description file
    protoc -o "${BASE_DIR}"/"${name}".desc "${protoFile}"
    
    # generate C# contract
    protoc --csharp_out=${OUTPUT_DIR} \
           --csharp_opt=file_extension=.g.cs \
           "${protoFile}"
}

#====

echo "$(protoc --version) used"
mkdir -p ${OUTPUT_DIR}

for protoFile in "${BASE_DIR}"/*.proto; do
    echo "Processing ${protoFile}..."
    generateContractFor "${protoFile}" && \
    echo Done
done