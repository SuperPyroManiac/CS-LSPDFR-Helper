FROM debian:stable-slim

# Install required dependencies
RUN apt-get update && \
    apt-get install -y \
        wget \
        libunwind8 \
        libicu70 \
        libc6 \
        libssl3 \
        libkrb5-3 \
        zlib1g \
        liblttng-ust0 \
        libcurl4 \
        libgc1c2 \
        libbrotli1 \
        liblzma5 \
        libzstd1 \
        && rm -rf /var/lib/apt/lists/*

# Download and install .NET runtime
RUN wget https://download.visualstudio.microsoft.com/download/pr/eb1f9c56-e00c-46d3-b07b-03f0384f6c62/9d8f4732fc1b8b3b3bbf7114a8d4f1a4/dotnet-runtime-8.0.0-linux-x64.tar.gz \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet-runtime-8.0.0-linux-x64.tar.gz -C /usr/share/dotnet \
    && rm dotnet-runtime-8.0.0-linux-x64.tar.gz

# Create a symbolic link (if needed)
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]