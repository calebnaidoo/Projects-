const fs = require('fs');
const https = require('https');
const app = require('./app');
const config = require('./config/env');


const startServer = () => {
  let sslOptions;

  try {
    sslOptions = {
      key: fs.readFileSync(config.ssl.keyPath),
      cert: fs.readFileSync(config.ssl.certPath),
    };
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error('\n[STARTUP ERROR] Could not find SSL certificate/key.');
    // eslint-disable-next-line no-console
    console.error(`Expected key at:  ${config.ssl.keyPath}`);
    // eslint-disable-next-line no-console
    console.error(`Expected cert at: ${config.ssl.certPath}`);
    // eslint-disable-next-line no-console
    console.error('Run "npm run generate-certs" (or see README) to create a local certificate.\n');
    process.exit(1);
  }

  https.createServer(sslOptions, app).listen(config.port, () => {
    // eslint-disable-next-line no-console
    console.log(`HustleHub+ API listening securely on https://localhost:${config.port}`);
    // eslint-disable-next-line no-console
    console.log(`Environment: ${config.nodeEnv}`);
  });
};

startServer();

// Guard against unhandled promise rejections crashing the process silently.
process.on('unhandledRejection', (err) => {
  // eslint-disable-next-line no-console
  console.error('UNHANDLED REJECTION! Shutting down...', err);
  process.exit(1);
});
