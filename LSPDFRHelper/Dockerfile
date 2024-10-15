# Use an official .NET runtime as a parent image.
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Install required libraries
RUN apt-get update && \
    apt-get install -y libc6 libleptonica-dev libtesseract-dev tesseract-ocr

# Create a symbolic link (if needed)
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Expose port 8055 to allow external connections
EXPOSE 8055

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]