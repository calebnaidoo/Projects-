package com.betwise.app.ui.components

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import com.betwise.app.ui.theme.*

data class BottomNavItem(val route: String, val label: String, val icon: ImageVector)

val bottomNavItems = listOf(
    BottomNavItem("dashboard", "Home", Icons.Default.Home),
    BottomNavItem("session_list", "Sessions", Icons.Default.List),
    BottomNavItem("categories", "Categories", Icons.Default.GridView),
    BottomNavItem("reports", "Reports", Icons.Default.BarChart),
    BottomNavItem("rewards", "Rewards", Icons.Default.EmojiEvents)
)

@Composable
fun BetWiseBottomBar(currentRoute: String, onNavigate: (String) -> Unit) {
    NavigationBar(containerColor = SurfaceDark, tonalElevation = 0.dp) {
        bottomNavItems.forEach { item ->
            val selected = currentRoute == item.route
            NavigationBarItem(
                selected = selected,
                onClick = { if (!selected) onNavigate(item.route) },
                icon = { Icon(item.icon, contentDescription = item.label) },
                label = { Text(item.label) },
                colors = NavigationBarItemDefaults.colors(
                    selectedIconColor = GoldPrimary,
                    selectedTextColor = GoldPrimary,
                    indicatorColor = Color(0xFF2A1F00),
                    unselectedIconColor = TextGray,
                    unselectedTextColor = TextGray
                )
            )
        }
    }
}
