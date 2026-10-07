package com.betwise.app.repository

import android.content.Context
import com.betwise.app.data.dao.CategoryTotal
import com.betwise.app.data.database.BetWiseDatabase
import com.betwise.app.data.entities.*
import kotlinx.coroutines.flow.Flow
import java.security.MessageDigest

class BetWiseRepository(context: Context) {

    private val db = BetWiseDatabase.getInstance(context)
    private val userDao = db.userDao()
    private val categoryDao = db.categoryDao()
    private val sessionDao = db.sessionDao()
    private val goalDao = db.goalDao()
    private val badgeDao = db.badgeDao()

    // ── Auth ────────────────────────────────────────────────────────────────
    suspend fun register(fullName: String, username: String, contact: String, password: String): Result<User> {
        if (userDao.findByUsername(username) != null)
            return Result.failure(Exception("Username already exists"))
        val user = User(
            fullName = fullName,
            username = username,
            contactDetail = contact,
            passwordHash = hash(password)
        )
        val id = userDao.insert(user)
        return Result.success(user.copy(id = id.toInt()))
    }

    suspend fun login(username: String, password: String): Result<User> {
        val user = userDao.findByUsername(username)
            ?: return Result.failure(Exception("User not found"))
        return if (user.passwordHash == hash(password)) Result.success(user)
        else Result.failure(Exception("Invalid password"))
    }

    // ── Categories ──────────────────────────────────────────────────────────
    fun getCategories(userId: Int): Flow<List<Category>> = categoryDao.getByUser(userId)
    suspend fun addCategory(category: Category) = categoryDao.insert(category)
    suspend fun deleteCategory(category: Category) = categoryDao.delete(category)
    suspend fun getCategoryById(id: Int) = categoryDao.findById(id)

    // ── Sessions ────────────────────────────────────────────────────────────
    fun getSessions(userId: Int): Flow<List<Session>> = sessionDao.getByUser(userId)
    fun getSessionsByDateRange(userId: Int, from: String, to: String): Flow<List<Session>> =
        sessionDao.getByDateRange(userId, from, to)
    suspend fun addSession(session: Session): Long {
        val id = sessionDao.insert(session)
        checkAndAwardBadges(session.userId)
        return id
    }
    suspend fun deleteSession(session: Session) = sessionDao.delete(session)
    suspend fun getSessionById(id: Int) = sessionDao.findById(id)
    suspend fun totalChipsForMonth(userId: Int, monthPrefix: String): Int =
        sessionDao.totalChipsForMonth(userId, monthPrefix) ?: 0
    suspend fun categoryTotalsForMonth(userId: Int, monthPrefix: String): List<CategoryTotal> =
        sessionDao.categoryTotalsForMonth(userId, monthPrefix)

    // ── Goals ────────────────────────────────────────────────────────────────
    suspend fun setGoal(goal: Goal) = goalDao.insert(goal)
    suspend fun getGoalForMonth(userId: Int, monthYear: String) = goalDao.getForMonth(userId, monthYear)
    fun getAllGoals(userId: Int): Flow<List<Goal>> = goalDao.getAllForUser(userId)

    // ── Badges ───────────────────────────────────────────────────────────────
    fun getBadges(userId: Int): Flow<List<Badge>> = badgeDao.getByUser(userId)

    private suspend fun checkAndAwardBadges(userId: Int) {
        val today = java.time.LocalDate.now().toString()
        val sessions = mutableListOf<Session>()

        // First session
        if (badgeDao.countByName(userId, "First Session") == 0) {
            badgeDao.insert(Badge(userId = userId, name = "First Session",
                emoji = "🥇", description = "Logged your very first session!", earnedAt = today))
        }
        // Category explorer - check if more than 3 distinct categories used
        val totals = sessionDao.categoryTotalsForMonth(userId, today.substring(0, 7))
        if (totals.size >= 3 && badgeDao.countByName(userId, "Category Explorer") == 0) {
            badgeDao.insert(Badge(userId = userId, name = "Category Explorer",
                emoji = "🃏", description = "Used 3+ different categories!", earnedAt = today))
        }
    }

    // ── Util ─────────────────────────────────────────────────────────────────
    private fun hash(input: String): String {
        val bytes = MessageDigest.getInstance("SHA-256").digest(input.toByteArray())
        return bytes.joinToString("") { "%02x".format(it) }
    }
}
