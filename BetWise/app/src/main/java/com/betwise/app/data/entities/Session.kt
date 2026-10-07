package com.betwise.app.data.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "sessions")
data class Session(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val userId: Int,
    val categoryId: Int,
    val title: String,
    val chipAmount: Int,
    val date: String,
    val startTime: String,
    val endTime: String,
    val description: String = "",
    val imagePath: String? = null
)
