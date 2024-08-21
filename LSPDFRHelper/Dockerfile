# Use a slim Debian base image
FROM debian:stable-slim AS runtime

# Install .NET runtime and required libraries
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libtesseract-dev \
        libleptonica-dev \
        wget \
        ca-certificates \
    && wget https://download.visualstudio.microsoft.com/download/pr/4d0ad0b8-50b4-4e4b-b6a7-4507e4e7f7d1/c4f2380d1d8e29d7d8d02d8a2ff35c40/dotnet-runtime-8.0.0-linux-x64.tar.gz \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet-runtime-8.0.0-linux-x64.tar.gz -C /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-runtime-8.0.0-linux-x64.tar.gz \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY LSPDFRHelper.dll .

# Command to run your application
CMD ["dotnet", "LSPDFRHelper.dll"]
