#!/bin/bash 


# CA Keys

openssl genrsa -aes256 -passout pass:xxxx -out ca.pass.key 4096
openssl rsa -passin pass:xxxx -in ca.pass.key -out ca.key
rm ca.pass.key

# CA certificate

openssl req -new -x509 -days 3650 -key ca.key -out ca.pem


# User Keys

openssl genrsa -aes256 -passout pass:xxxx -out alice.pass.key 4096
openssl rsa -passin pass:xxxx -in alice.pass.key -out alice.key
rm alice.pass.key

# User certificate

openssl req -new -key alice.key -out alice.csr
openssl x509 -req -days 3650 -in alice.csr -CA ca.pem -CAkey ca.key -set_serial 01 -out alice.pem

cat alice.key alice.pem ca.pem > alice.full.pem
# winpty openssl ... or openssl ... -passout pass:1234
openssl pkcs12 -export -out alice.full.pfx -inkey alice.key -in alice.pem -certfile ca.pem