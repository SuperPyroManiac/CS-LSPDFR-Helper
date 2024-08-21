FROM mcr.microsoft.com/dotnet/runtime:8.0-slim AS runtime

# Install required libraries and dependencies
RUN apt-get update && \
    apt-get install -y libc6 libtesseract-dev libleptonica-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Create a symbolic link (if needed)
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]