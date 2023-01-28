# docker build --progress=plain -t etl:scheduler -f etl_scheduler.dockerfile .


# build etl 
FROM  mcr.microsoft.com/dotnet/sdk:7.0-alpine3.16 as dotnet
WORKDIR /etl 
RUN apk add ca-certificates && update-ca-certificates
RUN git clone https://github.com/mikeTWC1984/etl7 . && \
    dotnet publish -o etl -r linux-x64 --no-self-contained && \
    rm -rf etl/ref etl/ja refl/runtimes etl/*librdkafka*


FROM alpine:3.16 as build
RUN apk add --no-cache git bash nodejs npm 
WORKDIR /opt/cronicle
RUN git clone https://github.com/cronicle-edge/cronicle-edge . && ./bundle.sh


FROM etl:base 

COPY --from=dotnet /etl/etl /usr/local/share/powershell/Modules/etl
RUN ln -s /usr/lib/librdkafka.so.1 /usr/local/share/powershell/Modules/etl/librdkafka.so

# cronicle 

ENV CRONICLE_foreground=1
ENV CRONICLE_echo=1
ENV TZ=America/New_York 
ENV EDITOR=vi
ENV PATH "/opt/cronicle/bin:${PATH}"

COPY --from=build /opt/cronicle/dist /opt/cronicle

WORKDIR /opt/cronicle

RUN mkdir /opt/cronicle/data && chmod 0750 /opt/cronicle/conf /opt/cronicle/data \
    && adduser -D -h /home/cronicle cronicle

ENTRYPOINT ["/sbin/tini", "--"]

