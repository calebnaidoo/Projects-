package com.betwise.app.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.betwise.app.data.entities.*
import com.betwise.app.repository.BetWiseRepository
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import java.time.LocalDate
import java.time.format.DateTimeFormatter

@OptIn(ExperimentalCoroutinesApi::class)
class MainViewModel(application: Application) : AndroidViewModel(application) {

    private val repo = BetWiseRepository(application)

    private val _userId = MutableStateFlow(0)

    fun setUser(id: Int) {
        _userId.value = id
        loadDashboard()
        loadCategoryMap()
    }

    val userId: Int get() = _userId.value

    // ── Flows derived from userId ─────────────────────────────────────────────
    val categories: Flow<List<Category>> = _userId
        .filter { it > 0 }
        .flatMapLatest { repo.getCategories(it) }
        .stateIn(viewModelScope, SharingStarted.Lazily, emptyList())

    val sessions: Flow<List<Session>> = _userId
        .filter { it > 0 }
        .flatMapLatest { repo.getSessions(it) }
        .stateIn(viewModelScope, SharingStarted.Lazily, emptyList())

    val badges: Flow<List<Badge>> = _userId
        .filter { it > 0 }
        .flatMapLatest { repo.getBadges(it) }
        .stateIn(viewModelScope, SharingStarted.Lazily, emptyList())

    val goals: Flow<List<Goal>> = _userId
        .filter { it > 0 }
        .flatMapLatest { repo.getAllGoals(it) }
        .stateIn(viewModelScope, SharingStarted.Lazily, emptyList())

    // ── Dashboard summary ─────────────────────────────────────────────────────
    private val _monthTotal = MutableStateFlow(0)
    val monthTotal: StateFlow<Int> = _monthTotal

    private val _currentGoal = MutableStateFlow<Goal?>(null)
    val currentGoal: StateFlow<Goal?> = _currentGoal

    fun loadDashboard() {
        val uid = _userId.value
        if (uid == 0) return
        viewModelScope.launch {
            val month = LocalDate.now().format(DateTimeFormatter.ofPattern("yyyy-MM"))
            _monthTotal.value = repo.totalChipsForMonth(uid, month)
            _currentGoal.value = repo.getGoalForMonth(uid, month)
        }
    }

    // ── Category map for display ──────────────────────────────────────────────
    private val _categoryMap = MutableStateFlow<Map<Int, Category>>(emptyMap())
    val categoryMap: StateFlow<Map<Int, Category>> = _categoryMap

    fun loadCategoryMap() {
        val uid = _userId.value
        if (uid == 0) return
        viewModelScope.launch {
            repo.getCategories(uid).collect { list ->
                _categoryMap.value = list.associateBy { it.id }
            }
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    fun addCategory(name: String, icon: String) {
        viewModelScope.launch {
            repo.addCategory(Category(userId = _userId.value, name = name, icon = icon))
        }
    }

    fun deleteCategory(category: Category) {
        viewModelScope.launch { repo.deleteCategory(category) }
    }

    fun addSession(session: Session) {
        viewModelScope.launch {
            repo.addSession(session)
            loadDashboard()
        }
    }

    fun deleteSession(session: Session) {
        viewModelScope.launch {
            repo.deleteSession(session)
            loadDashboard()
        }
    }

    fun setGoal(minChips: Int, maxChips: Int) {
        viewModelScope.launch {
            val month = LocalDate.now().format(DateTimeFormatter.ofPattern("yyyy-MM"))
            repo.setGoal(Goal(userId = _userId.value, monthYear = month,
                minChips = minChips, maxChips = maxChips))
            loadDashboard()
        }
    }
}
