package com.betwise.app.data.dao

import androidx.room.*
import com.betwise.app.data.entities.Badge
import kotlinx.coroutines.flow.Flow

@Dao
interface BadgeDao {
    @Insert(onConflict = OnConflictStrategy.IGNORE)
    suspend fun insert(badge: Badge): Long

    @Query("SELECT * FROM badges WHERE userId = :userId ORDER BY earnedAt DESC")
    fun getByUser(userId: Int): Flow<List<Badge>>

    @Query("SELECT COUNT(*) FROM badges WHERE userId = :userId AND name = :name")
    suspend fun countByName(userId: Int, name: String): Int
}
