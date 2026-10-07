package com.betwise.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.Badge
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RewardsScreen(viewModel: MainViewModel, onNavigate: (String) -> Unit) {
    val badges by viewModel.badges.collectAsState(initial = emptyList())

    val allBadges = listOf(
        Triple("First Session", "🥇", "Log your very first session"),
        Triple("Category Explorer", "🃏", "Use 3+ different categories"),
        Triple("7-Day Streak", "🔥", "Log sessions 7 days in a row"),
        Triple("Stayed Within Goal", "🎯", "Finish a month within your limit"),
        Triple("High Roller", "💎", "Log a single session with 500+ chips")
    )

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Rewards", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark)
            )
        },
        bottomBar = { BetWiseBottomBar(currentRoute = "rewards", onNavigate = onNavigate) }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(horizontal = 16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp),
            contentPadding = PaddingValues(vertical = 16.dp)
        ) {
            item {
                BetWiseCard {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text("🏆", fontSize = 36.sp)
                        Spacer(Modifier.width(14.dp))
                        Column {
                            Text(
                                "${badges.size} / ${allBadges.size} Earned",
                                color = GoldPrimary,
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            )
                            Text("Keep playing to unlock more!", color = TextGray, fontSize = 13.sp)
                        }
                    }
                }
            }

            item { SectionTitle("YOUR BADGES") }

            items(allBadges) { (name, emoji, desc) ->
                val earned = badges.firstOrNull { it.name == name }
                BadgeCard(
                    name = name,
                    emoji = emoji,
                    description = desc,
                    earned = earned
                )
            }
        }
    }
}

@Composable
private fun BadgeCard(
    name: String,
    emoji: String,
    description: String,
    earned: Badge?
) {
    val isEarned = earned != null
    Card(
        colors = CardDefaults.cardColors(
            containerColor = if (isEarned) CardDark else CardDark.copy(alpha = 0.5f)
        ),
        shape = RoundedCornerShape(14.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier.size(56.dp),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = emoji,
                    fontSize = 36.sp,
                    color = if (isEarned) Color.Unspecified else Color.Gray
                )
                if (!isEarned) {
                    Text(
                        text = "🔒",
                        fontSize = 18.sp,
                        modifier = Modifier.align(Alignment.BottomEnd)
                    )
                }
            }
            Spacer(Modifier.width(14.dp))
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    name,
                    color = if (isEarned) GoldPrimary else TextGray,
                    fontWeight = FontWeight.Bold,
                    fontSize = 15.sp
                )
                Text(description, color = TextGray, fontSize = 12.sp)
                if (isEarned) {
                    Spacer(Modifier.height(4.dp))
                    Text("Earned ${earned!!.earnedAt} ✅", color = GreenAccent, fontSize = 11.sp)
                }
            }
        }
    }
}
