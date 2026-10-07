const jwt = require('jsonwebtoken');
const config = require('../config/env');
const AppError = require('../utils/AppError');
const { findById } = require('../models/userModel');

/**
 * protect - verifies a JWT on every request to a protected route.
 *
 * Flow:
 *   1. Expect an "Authorization: Bearer <token>" header.
 *   2. Verify the token's signature and expiry against JWT_SECRET.
 *   3. Confirm the user referenced by the token still exists (handles
 *   the case where a user was deleted after a token was issued).
 *   4. Attach the authenticated user to req.user for downstream handlers
 */
const protect = (req, res, next) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return next(new AppError('Not authorised. Please log in to access this resource.', 401));
  }

  const token = authHeader.split(' ')[1];

  try {
    const decoded = jwt.verify(token, config.jwt.secret);

    const user = findById(decoded.id);
    if (!user) {
      return next(new AppError('Not authorised. Please log in to access this resource.', 401));
    }

    req.user = { id: user.id, role: user.role, email: user.email, username: user.username };
    return next();
  } catch (err) {
    return next(new AppError('Not authorised. Please log in to access this resource.', 401));
  }
};

module.exports = { protect };
