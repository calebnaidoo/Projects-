package com.betwise.app.navigation

import androidx.compose.runtime.*
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.betwise.app.ui.screens.*
import com.betwise.app.viewmodel.AuthViewModel
import com.betwise.app.viewmodel.MainViewModel

@Composable
fun AppNavGraph(navController: NavHostController) {
    val authViewModel: AuthViewModel = viewModel()
    val mainViewModel: MainViewModel = viewModel()
    val currentUser by authViewModel.currentUser.collectAsState()

    fun goHome() {
        navController.navigate(Routes.DASHBOARD) {
            popUpTo(0) { inclusive = true }
        }
    }

    NavHost(navController = navController, startDestination = Routes.SPLASH) {

        // ── Splash ────────────────────────────────────────────────────────────
        composable(Routes.SPLASH) {
            SplashScreen(onFinished = {
                navController.navigate(Routes.LOGIN) {
                    popUpTo(Routes.SPLASH) { inclusive = true }
                }
            })
        }

        // ── Auth ──────────────────────────────────────────────────────────────
        composable(Routes.LOGIN) {
            LoginScreen(
                viewModel = authViewModel,
                onLoginSuccess = {
                    currentUser?.let { mainViewModel.setUser(it.id) }
                    goHome()
                },
                onNavigateToRegister = {
                    navController.navigate(Routes.REGISTER)
                }
            )
        }

        composable(Routes.REGISTER) {
            RegisterScreen(
                viewModel = authViewModel,
                onRegisterSuccess = {
                    currentUser?.let { mainViewModel.setUser(it.id) }
                    goHome()
                },
                onNavigateToLogin = { navController.popBackStack() }
            )
        }

        // ── Main screens ──────────────────────────────────────────────────────
        composable(Routes.DASHBOARD) {
            val user = currentUser ?: return@composable
            DashboardScreen(
                user = user,
                viewModel = mainViewModel,
                onNavigate = { navController.navigate(it) }
            )
        }

        composable(Routes.CATEGORIES) {
            CategoriesScreen(
                viewModel = mainViewModel,
                onNavigate = { navController.navigate(it) }
            )
        }

        composable(Routes.ADD_SESSION) {
            AddSessionScreen(
                viewModel = mainViewModel,
                onNavigateBack = { navController.popBackStack() }
            )
        }

        composable(Routes.SESSION_LIST) {
            SessionListScreen(
                viewModel = mainViewModel,
                onNavigate = { navController.navigate(it) }
            )
        }

        composable(Routes.GOALS) {
            GoalsScreen(
                viewModel = mainViewModel,
                onNavigateBack = { navController.popBackStack() }
            )
        }

        composable(Routes.REPORTS) {
            ReportsScreen(
                viewModel = mainViewModel,
                onNavigate = { navController.navigate(it) }
            )
        }

        composable(Routes.REWARDS) {
            RewardsScreen(
                viewModel = mainViewModel,
                onNavigate = { navController.navigate(it) }
            )
        }

        composable(Routes.SETTINGS) {
            val user = currentUser ?: return@composable
            SettingsScreen(
                user = user,
                onLogout = {
                    authViewModel.logout()
                    navController.navigate(Routes.LOGIN) {
                        popUpTo(0) { inclusive = true }
                    }
                },
                onNavigateBack = { navController.popBackStack() }
            )
        }
    }
}
