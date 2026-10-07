const jwt = require('jsonwebtoken');
const config = require('../config/env');

/**
 * Generates a signed JWT for an authenticated user.
 *
 * Design decisions:
 *  - The payload only contains the user's id and role. We deliberately do
 *    NOT embed the email, password hash, or any other personal/sensitive
 *    data in the token, because a JWT payload is base64-encoded, not
 *    encrypted, and can be read by anyone who has the token.
 *  - The token is short-lived (see JWT_EXPIRES_IN) to limit the damage
 *    window if a token is ever stolen (e.g. via XSS).
 *  - The signing secret is loaded from environment configuration and is
 *    never hard-coded in source.
 */
const generateToken = (user) => {
  const payload = {
    id: user.id,
    role: user.role,
  };

  return jwt.sign(payload, config.jwt.secret, {
    expiresIn: config.jwt.expiresIn,
    issuer: 'hustlehub-plus-api',
  });
};

module.exports = generateToken;
