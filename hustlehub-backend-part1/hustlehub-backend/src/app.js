const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');

const config = require('./config/env');
const authRoutes = require('./routes/authRoutes');
const { notFound, globalErrorHandler } = require('./middleware/errorMiddleware');

const app = express();

// Security middleware 
// Helmet sets a range of protective HTTP headers by default 
app.use(helmet());

app.use(
  cors({
    origin: config.cors.origin,
    credentials: true,
  })
);

// Body parsing
// Limits protect against oversized payload / basic denial-of-service attempts.
app.use(express.json({ limit: '10kb' }));
app.use(express.urlencoded({ extended: true, limit: '10kb' }));

// Logging
if (config.nodeEnv !== 'test') {
  app.use(morgan(config.nodeEnv === 'development' ? 'dev' : 'combined'));
}

// Health check 
app.get('/api/health', (req, res) => {
  res.status(200).json({ success: true, message: 'HustleHub+ API is running.' });
});

// Routes
app.use('/api/auth', authRoutes);

// 404 and centralised error handling 
app.use(notFound);
app.use(globalErrorHandler);

module.exports = app;
