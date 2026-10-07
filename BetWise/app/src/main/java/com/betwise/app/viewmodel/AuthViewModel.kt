package com.betwise.app.viewmodel

import android.app.Application
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.betwise.app.data.entities.User
import com.betwise.app.repository.BetWiseRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class AuthViewModel(application: Application) : AndroidViewModel(application) {
    private val repo = BetWiseRepository(application)

    private val _currentUser = MutableStateFlow<User?>(null)
    val currentUser: StateFlow<User?> = _currentUser

    private val _authError = MutableStateFlow<String?>(null)
    val authError: StateFlow<String?> = _authError

    fun register(fullName: String, username: String, contact: String, password: String, onSuccess: () -> Unit) {
        viewModelScope.launch {
            val result = repo.register(fullName, username, contact, password)
            result.onSuccess { user ->
                _currentUser.value = user
                _authError.value = null
                onSuccess()
            }.onFailure { _authError.value = it.message }
        }
    }

    fun login(username: String, password: String, onSuccess: () -> Unit) {
        viewModelScope.launch {
            val result = repo.login(username, password)
            result.onSuccess { user ->
                _currentUser.value = user
                _authError.value = null
                onSuccess()
            }.onFailure { _authError.value = it.message }
        }
    }

    fun logout() { _currentUser.value = null }

    fun clearError() { _authError.value = null }
}
