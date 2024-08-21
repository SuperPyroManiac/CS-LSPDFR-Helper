# Use an official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Install required libraries
RUN apt-get update && apt-get install -y libdl-dev
ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]