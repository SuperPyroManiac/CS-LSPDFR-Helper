# Use a slim Debian base image
FROM debian:stable-slim AS runtime

# Install dependencies and .NET runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libtesseract-dev \
        libleptonica-dev \
        wget \
        ca-certificates \
    && wget https://packages.microsoft.com/config/debian/12/prod.list \
    && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
    && wget -qO- https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && apt-get update \
    && apt-get install -y --no-install-recommends dotnet-runtime-8.0 \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

    # Set the working directory inside the container
    WORKDIR /app

    # Copy your compiled DLL and other necessary files into the container
    COPY LSPDFRHelper.dll .

    # Command to run your application
    CMD ["dotnet", "LSPDFRHelper.dll"]
