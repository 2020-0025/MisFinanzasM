#!/usr/bin/env bash
# exit on error
set -o errexit

dotnet restore
dotnet publish -c Release -o out