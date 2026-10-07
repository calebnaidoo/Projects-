package com.betwise.app.data.database

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import com.betwise.app.data.dao.*
import com.betwise.app.data.entities.*

@Database(
    entities = [User::class, Category::class, Session::class, Goal::class, Badge::class],
    version = 1,
    exportSchema = false
)
abstract class BetWiseDatabase : RoomDatabase() {
    abstract fun userDao(): UserDao
    abstract fun categoryDao(): CategoryDao
    abstract fun sessionDao(): SessionDao
    abstract fun goalDao(): GoalDao
    abstract fun badgeDao(): BadgeDao

    companion object {
        @Volatile private var INSTANCE: BetWiseDatabase? = null

        fun getInstance(context: Context): BetWiseDatabase =
            INSTANCE ?: synchronized(this) {
                Room.databaseBuilder(
                    context.applicationContext,
                    BetWiseDatabase::class.java,
                    "betwise.db"
                ).build().also { INSTANCE = it }
            }
    }
}
