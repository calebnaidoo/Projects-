const { body, validationResult } = require('express-validator');
const AppError = require('../utils/AppError');

/**
 * Validation & sanitisation rules for registration.
 *
 * All input is treated as untrusted. Each field is:
 *   1. Trimmed/normalised (removes whitespace, escapes HTML) - mitigates
 *      stored XSS / injection style payloads.
 *   2. Constrained to an explicit allow-list of acceptable characters
 *      and lengths - rejects malformed or malicious input outright
 *      rather than trying to "clean" it.
 */
const registerRules = [
  body('username')
    .trim()
    .escape()
    .isLength({ min: 3, max: 30 })
    .withMessage('Username must be between 3 and 30 characters.')
    .matches(/^[a-zA-Z0-9_]+$/)
    .withMessage('Username may only contain letters, numbers, and underscores.'),

  body('email')
    .trim()
    .isEmail()
    .withMessage('A valid email address is required.')
    .normalizeEmail(),

  body('password')
    .isLength({ min: 8, max: 128 })
    .withMessage('Password must be at least 8 characters long.')
    .matches(/[a-z]/)
    .withMessage('Password must contain at least one lowercase letter.')
    .matches(/[A-Z]/)
    .withMessage('Password must contain at least one uppercase letter.')
    .matches(/[0-9]/)
    .withMessage('Password must contain at least one number.')
    .matches(/[^a-zA-Z0-9]/)
    .withMessage('Password must contain at least one special character.'),

  body('role')
    .optional()
    .trim()
    .isIn(['client', 'freelancer'])
    .withMessage('Role must be either "client" or "freelancer".'),
];

const loginRules = [
  body('email').trim().isEmail().withMessage('A valid email address is required.').normalizeEmail(),
  body('password').notEmpty().withMessage('Password is required.'),
];

/**
 * Collects any validation errors raised by the rules above and converts
 * them into a single, safe, client-facing 400 response - no internal
 * detail (stack traces, library names, file paths) is ever exposed.
 */
const handleValidationErrors = (req, res, next) => {
  const errors = validationResult(req);

  if (!errors.isEmpty()) {
    const messages = errors.array().map((e) => e.msg);
    return next(new AppError(messages.join(' '), 400));
  }

  return next();
};

module.exports = {
  registerRules,
  loginRules,
  handleValidationErrors,
};
