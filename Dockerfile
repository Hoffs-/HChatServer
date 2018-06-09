FROM microsoft/dotnet:2.1-sdk-stretch
RUN apt-get -yq update
RUN apt-get -yqq install git protobuf-compiler

# Clone repo
ARG build_branch=master
RUN git clone -b ${build_branch} https://github.com/Hoffs/HChatServer /usr/local/src/HChatServer
WORKDIR /usr/local/src/HChatServer
RUN git submodule update --init --recursive

# Compile ChatProtos
WORKDIR /usr/local/src/HChatServer/submodules/ChatProtos
RUN chmod +x build_linux.sh
RUN ./build_linux.sh
WORKDIR /usr/local/src/HChatServer
RUN mkdir ChatProtos
RUN cp -r submodules/ChatProtos/Compiled/* ChatProtos/
RUN rm ChatProtos/Networking/Request.g.cs ChatProtos/Networking/ResponseStatus.g.cs ChatProtos/Networking/Response.g.cs
RUN rm -rf submodules/ChatProtos/Compiled
ARG release_type=Release

# Publish app
RUN dotnet publish -c ${release_type} -o published/

# Build run image
FROM microsoft/dotnet:2.1-runtime-stretch-slim
WORKDIR /root/
COPY --from=0 /usr/local/src/HChatServer/published/ .
ENV LISTEN_PORT=4400
CMD dotnet ChatServer.dll ${LISTEN_PORT}