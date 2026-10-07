package com.betwise.app.data.dao

import androidx.room.*
import com.betwise.app.data.entities.Goal
import kotlinx.coroutines.flow.Flow

@Dao
interface GoalDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(goal: Goal): Long

    @Query("SELECT * FROM goals WHERE userId = :userId AND monthYear = :monthYear LIMIT 1")
    suspend fun getForMonth(userId: Int, monthYear: String): Goal?

    @Query("SELECT * FROM goals WHERE userId = :userId ORDER BY monthYear DESC")
    fun getAllForUser(userId: Int): Flow<List<Goal>>
}
