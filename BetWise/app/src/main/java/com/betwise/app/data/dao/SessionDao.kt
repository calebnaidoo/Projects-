package com.betwise.app.data.dao

import androidx.room.*
import com.betwise.app.data.entities.Session
import kotlinx.coroutines.flow.Flow

@Dao
interface SessionDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(session: Session): Long

    @Delete
    suspend fun delete(session: Session)

    @Query("SELECT * FROM sessions WHERE userId = :userId ORDER BY date DESC, startTime DESC")
    fun getByUser(userId: Int): Flow<List<Session>>

    @Query("SELECT * FROM sessions WHERE userId = :userId AND date BETWEEN :from AND :to ORDER BY date DESC")
    fun getByDateRange(userId: Int, from: String, to: String): Flow<List<Session>>

    @Query("SELECT SUM(chipAmount) FROM sessions WHERE userId = :userId AND date LIKE :monthPrefix || '%'")
    suspend fun totalChipsForMonth(userId: Int, monthPrefix: String): Int?

    @Query("SELECT categoryId, SUM(chipAmount) as total FROM sessions WHERE userId = :userId AND date LIKE :monthPrefix || '%' GROUP BY categoryId")
    suspend fun categoryTotalsForMonth(userId: Int, monthPrefix: String): List<CategoryTotal>

    @Query("SELECT * FROM sessions WHERE id = :id LIMIT 1")
    suspend fun findById(id: Int): Session?
}

data class CategoryTotal(val categoryId: Int, val total: Int)
