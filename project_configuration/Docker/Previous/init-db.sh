#!/bin/bash
set -e

# Create multiple databases
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE keycloak;
EOSQL

Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = ".nuget", ".nuget", "{F4AEBB8B-A367-424E-8B14-F611C9667A85}"
ProjectSection(SolutionItems) = preProject
    .nuget\NuGet.Config = .nuget\NuGet.Config
    .nuget\NuGet.exe = .nuget\NuGet.exe
    .nuget\NuGet.targets = .nuget\NuGet.targets
EndProjectSection
EndProject


