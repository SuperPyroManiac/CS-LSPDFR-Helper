FROM debian:stable-slim
# Install required libraries
RUN apt-get update && \
    apt-get install -y libc6 libleptonica-dev libtesseract-dev tesseract-ocr

# Create a symbolic link (if needed)
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

# Set the working directory inside the container
WORKDIR /app

# Copy your compiled DLL and other necessary files into the container
COPY . .

# Command to run your Discord bot
CMD ["dotnet", "LSPDFRHelper.dll"]