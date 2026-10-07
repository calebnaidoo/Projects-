#!/bin/bash
# Generates a self-signed SSL certificate for local development.
# Requires OpenSSL (pre-installed on macOS/Linux; on Windows use
# Git Bash, WSL, or install OpenSSL separately).
# Usage:  npm run generate-certs or: bash generate-certs.sh


set -e

CERT_DIR="./certs"
mkdir -p "$CERT_DIR"

if [ -f "$CERT_DIR/key.pem" ] && [ -f "$CERT_DIR/cert.pem" ]; then
  echo "Certificates already exist in $CERT_DIR - delete them first if you want to regenerate."
  exit 0
fi

openssl req -x509 -nodes -days 365 \
  -newkey rsa:2048 \
  -keyout "$CERT_DIR/key.pem" \
  -out "$CERT_DIR/cert.pem" \
  -subj "/C=ZA/ST=KwaZulu-Natal/L=Durban/O=HustleHubPlus/OU=Dev/CN=localhost"

echo ""
echo "Self-signed certificate generated:"
echo "  $CERT_DIR/key.pem"
echo "  $CERT_DIR/cert.pem"
echo ""
echo "NOTE: Browsers/Postman will flag this as untrusted since it is"
echo "self-signed. That is expected for local development - see README."
