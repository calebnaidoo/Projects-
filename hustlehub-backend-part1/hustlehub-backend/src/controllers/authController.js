const bcrypt = require('bcryptjs');
const config = require('../config/env');
const AppError = require('../utils/AppError');
const generateToken = require('../utils/generateToken');
const { findByEmail, createUser, toSafeUser } = require('../models/userModel');

/**
 * @route   POST /api/auth/register
 * @desc    Register a new user (client or freelancer)
 * @access  Public
 */
const register = async (req, res, next) => {
  try {
    const { username, email, password, role } = req.body;

    const existingUser = findByEmail(email);
    if (existingUser) {
      // Deliberately generic-ish message: confirms an account exists for
      // this flow (registration UX needs this), but login errors below
      // do NOT confirm/deny account existence - see login().
      return next(new AppError('An account with this email already exists.', 409));
    }

    const passwordHash = await bcrypt.hash(password, config.bcrypt.saltRounds);

    const newUser = createUser({ username, email, passwordHash, role });
    const token = generateToken(newUser);

    return res.status(201).json({
      success: true,
      message: 'User registered successfully.',
      data: {
        user: toSafeUser(newUser),
        token,
      },
    });
  } catch (err) {
    return next(err);
  }
};

/**
 * @route   POST /api/auth/login
 * @desc    Authenticate a user and issue a JWT
 * @access  Public
 */
const login = async (req, res, next) => {
  try {
    const { email, password } = req.body;

    const user = findByEmail(email);

    // SECURITY: use one identical, generic error for "no such user" and
    // "wrong password". Distinguishing between the two lets an attacker
    // enumerate valid email addresses on the platform.
    const invalidCredentialsError = new AppError('Invalid email or password.', 401);

    if (!user) {
      return next(invalidCredentialsError);
    }

    const passwordMatches = await bcrypt.compare(password, user.password);
    if (!passwordMatches) {
      return next(invalidCredentialsError);
    }

    const token = generateToken(user);

    return res.status(200).json({
      success: true,
      message: 'Login successful.',
      data: {
        user: toSafeUser(user),
        token,
      },
    });
  } catch (err) {
    return next(err);
  }
};

/**
 * @route   GET /api/auth/profile
 * @desc    Return the currently authenticated user's profile.
 *          Demonstrates that a route is correctly protected by JWT
 *          middleware and that the token payload maps back to a real user.
 * @access  Private (requires valid JWT)
 */
const getProfile = (req, res) => {
  return res.status(200).json({
    success: true,
    message: 'Profile retrieved successfully.',
    data: { user: req.user },
  });
};

module.exports = { register, login, getProfile };
