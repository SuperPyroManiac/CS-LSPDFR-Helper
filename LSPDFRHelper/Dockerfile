# Use the .NET runtime slim image as the base image
FROM mcr.microsoft.com/dotnet/runtime:8.0-slim AS runtime

# Install required libraries
RUN apt-get update && \
    apt-get install -y \
        libtesseract-dev \
        libleptonica-dev \
        && rm -rf /var/lib/apt/lists/*

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your application
CMD ["dotnet", "LSPDFRHelper.dll"]
