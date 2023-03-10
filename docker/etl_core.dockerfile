
# docker build --progress=plain -t etl:core -f etl_core.dockerfile .

FROM alpine:3.16 as core


# rebuild curl, to make sure it supports sftp
FROM core AS curl
WORKDIR /curl
RUN wget -O- https://github.com/curl/curl/archive/refs/tags/curl-7_81_0.tar.gz | tar -xz --strip-components=1 \
    && apk add bash alpine-sdk libtool libssh2-dev autoconf automake \
    && autoreconf -fi && ./configure --with-openssl --with-libssh2 && make && make install

# prebuild NTLM dependencies
FROM core as ntlm
RUN apk add --no-cache git curl make m4 autoconf automake gcc g++ krb5-dev openssl-dev gettext-dev libtool libxml2 libxslt libunistring-dev zlib-dev samba-dev
RUN git clone https://github.com/gssapi/gss-ntlmssp && cd gss-ntlmssp && autoreconf -f -i && ./configure --without-manpages --disable-nls && make install

# install core dependecies for powershell, kerberos, etc
FROM core 
RUN apk add --no-cache \
    ca-certificates \
    less \
    ncurses-terminfo-base \
    krb5-libs \
    libgcc \
    libintl \
    libssl1.1 \
    libstdc++ \
    tzdata \
    userspace-rcu \
    zlib \
    icu-libs \
    lttng-ust \
    tini util-linux ca-certificates tar openssl krb5 procps coreutils acl jq \
    cyrus-sasl cyrus-sasl-gssapiv2 libwbclient libssh2 bash 

#Install Curl
COPY --from=curl /usr/local/bin/curl /usr/local/bin/curl
COPY --from=curl /usr/local/lib/libcurl.so.4.7.0 /usr/local/lib/libcurl.so.4.7.0
RUN ln -s /usr/local/lib/libcurl.so.4.7.0 /usr/local/lib/libcurl.so.4

COPY --from=ntlm  /gss-ntlmssp/.libs/gssntlmssp.so /usr/local/lib/gssntlmssp.so
RUN mkdir -p /usr/etc/gss/mech.d && echo 'gssntlmssp_v1		1.3.6.1.4.1.311.2.2.10	        /usr/local/lib/gssntlmssp.so' >  /usr/etc/gss/mech.d/mech.ntlmssp.conf