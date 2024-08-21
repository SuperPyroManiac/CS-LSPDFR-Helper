# Use an official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]