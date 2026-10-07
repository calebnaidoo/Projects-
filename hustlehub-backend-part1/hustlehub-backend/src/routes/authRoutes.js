const express = require('express');
const { register, login, getProfile } = require('../controllers/authController');
const { registerRules, loginRules, handleValidationErrors } = require('../middleware/validateMiddleware');
const { protect } = require('../middleware/authMiddleware');

const router = express.Router();

// Public routes
router.post('/register', registerRules, handleValidationErrors, register);
router.post('/login', loginRules, handleValidationErrors, login);

// Protected route - demonstrates JWT middleware guarding a resource.
router.get('/profile', protect, getProfile);

module.exports = router;
