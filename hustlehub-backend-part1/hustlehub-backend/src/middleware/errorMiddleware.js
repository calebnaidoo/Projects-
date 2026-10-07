const config = require('../config/env');

/**
 * notFound - catches any request to a route that doesn't exist and
 * forwards it to the global error handler as a clean 404, instead of
 * letting Express return its default (and more revealing) HTML error page.
 */
const notFound = (req, res, next) => {
  const error = new Error(`Route not found - ${req.originalUrl}`);
  error.statusCode = 404;
  error.isOperational = true; // a 404 is an expected, safe-to-report condition
  next(error);
};

/**
 * globalErrorHandler - the single place where every error in the
 * application ends up. This guarantees a consistent, safe response shape
 * and means sensitive internals (stack traces, file paths, library
 * versions, DB error text) are NEVER sent to the client.
 *
 * - "Operational" errors (AppError instances, e.g. bad input, invalid
 *   credentials) send their intended message and status code.
 * - Anything else (a genuine bug/crash) is logged in full server-side
 *   for debugging, but the client only ever receives a generic message.
 */
// eslint-disable-next-line no-unused-vars
const globalErrorHandler = (err, req, res, next) => {
  const statusCode = err.statusCode || 500;
  const isOperational = err.isOperational === true;

  // Full detail stays on the server console/logs only.
  // eslint-disable-next-line no-console
  console.error(`[ERROR] ${req.method} ${req.originalUrl} ->`, err.message);
  if (config.nodeEnv === 'development' && !isOperational) {
    // eslint-disable-next-line no-console
    console.error(err.stack);
  }

  res.status(statusCode).json({
    success: false,
    message: isOperational ? err.message : 'Something went wrong. Please try again later.',
  });
};

module.exports = { notFound, globalErrorHandler };
