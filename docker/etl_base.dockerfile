
# docker build --progress=plain -t etl:base -f etl_base.dockerfile .

# install powershell, node, kcat, mc
FROM etl:core
RUN  apk add nodejs kcat
WORKDIR /opt/microsoft/powershell/7
RUN curl -k -L https://github.com/PowerShell/PowerShell/releases/download/v7.3.1/powershell-7.3.1-linux-alpine-x64.tar.gz | tar -xz \
   && chmod +x pwsh && ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh

# install mc tool (for s3), also compress with upx
RUN curl -L https://dl.min.io/client/mc/release/linux-amd64/mc > /usr/bin/mc && chmod +x /usr/bin/mc && apk add upx && upx /usr/bin/mc && apk del upx


