#!/bin/bash
# Start Ollama and ensure embedding model is available

# Pull the nomic-embed-text model
echo "Pulling nomic-embed-text embedding model..."
ollama pull nomic-embed-text || echo "Model pull failed or already exists"

# Start Ollama server
echo "Starting Ollama server..."
exec ollama serve
