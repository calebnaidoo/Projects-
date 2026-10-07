/**
 * Centralised environment configuration
 * Loading all environment variables through a single module means:
 *  1. There is exactly one place that reads process.env.
 *  2. We can fail fast at startup if a required secret is missing, instead
 *  of discovering it later as an obscure runtime bug.
 */
const path = require('path');
require('dotenv').config();

const requiredInProduction = ['JWT_SECRET'];

const config = {
  nodeEnv: process.env.NODE_ENV || 'development',
  port: parseInt(process.env.PORT, 10) || 5000,

  jwt: {
    secret: process.env.JWT_SECRET || 'dev_only_insecure_secret_do_not_use_in_prod',
    expiresIn: process.env.JWT_EXPIRES_IN || '1h',
  },

  bcrypt: {
    saltRounds: parseInt(process.env.BCRYPT_SALT_ROUNDS, 10) || 12,
  },

  ssl: {
    keyPath: path.resolve(process.cwd(), process.env.SSL_KEY_PATH || './certs/key.pem'),
    certPath: path.resolve(process.cwd(), process.env.SSL_CERT_PATH || './certs/cert.pem'),
  },

  cors: {
    origin: (process.env.CORS_ORIGIN || 'https://localhost:3000').split(','),
  },
};

// Fail fast: never allow the app to boot in production with default/weak secrets.
if (config.nodeEnv === 'production') {
  requiredInProduction.forEach((key) => {
    if (!process.env[key]) {
      
      console.error(`FATAL: Missing required environment variable "${key}" in production.`);
      process.exit(1);
    }
  });
}

module.exports = config;
