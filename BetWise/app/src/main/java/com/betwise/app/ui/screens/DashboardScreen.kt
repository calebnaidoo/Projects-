package com.betwise.app.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.User
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    user: User,
    viewModel: MainViewModel,
    onNavigate: (String) -> Unit
) {
    val monthTotal by viewModel.monthTotal.collectAsState()
    val currentGoal by viewModel.currentGoal.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.loadDashboard()
    }

    val maxChips = currentGoal?.maxChips ?: 1000
    val minChips = currentGoal?.minChips ?: 200
    val progress = if (maxChips > 0) (monthTotal.toFloat() / maxChips).coerceIn(0f, 1f) else 0f
    val isOverLimit = monthTotal > maxChips
    val isUnderMin = monthTotal < minChips

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = {
                    Column {
                        Text("Welcome back,", color = TextGray, fontSize = 12.sp)
                        Text(user.fullName, color = GoldPrimary, fontWeight = FontWeight.Bold)
                    }
                },
                actions = {
                    IconButton(onClick = { onNavigate("settings") }) {
                        Icon(Icons.Default.Settings, contentDescription = "Settings", tint = TextGray)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark)
            )
        },
        bottomBar = { BetWiseBottomBar(currentRoute = "dashboard", onNavigate = onNavigate) }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {

            // ── Chip Summary Card ──────────────────────────────────────────
            Card(
                colors = CardDefaults.cardColors(containerColor = Color.Transparent),
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(20.dp)
            ) {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(
                            Brush.linearGradient(
                                listOf(Color(0xFF2A1F00), Color(0xFF1A1500))
                            )
                        )
                        .padding(20.dp)
                ) {
                    Column {
                        Text("THIS MONTH", color = TextGray, fontSize = 11.sp, letterSpacing = 1.5.sp)
                        Spacer(Modifier.height(6.dp))
                        Row(verticalAlignment = Alignment.Bottom) {
                            Text(
                                text = "$monthTotal",
                                fontSize = 52.sp,
                                fontWeight = FontWeight.ExtraBold,
                                color = if (isOverLimit) RedAccent else GoldPrimary
                            )
                            Text(
                                text = "  chips",
                                fontSize = 18.sp,
                                color = TextGray,
                                modifier = Modifier.padding(bottom = 10.dp)
                            )
                        }

                        Spacer(Modifier.height(12.dp))

                        // Progress bar
                        LinearProgressIndicator(
                            progress = { progress },
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(10.dp)
                                .clip(RoundedCornerShape(5.dp)),
                            color = if (isOverLimit) RedAccent else GoldPrimary,
                            trackColor = Color(0xFF333333),
                            strokeCap = StrokeCap.Round
                        )

                        Spacer(Modifier.height(8.dp))

                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("Min: $minChips 🎯", color = GreenAccent, fontSize = 12.sp)
                            Text("Max: $maxChips 🔴", color = if (isOverLimit) RedAccent else TextGray, fontSize = 12.sp)
                        }

                        if (isOverLimit) {
                            Spacer(Modifier.height(8.dp))
                            Text(
                                "⚠️ You've exceeded your monthly limit!",
                                color = RedAccent,
                                fontSize = 12.sp,
                                fontWeight = FontWeight.Bold
                            )
                        } else if (!isUnderMin) {
                            Spacer(Modifier.height(8.dp))
                            Text(
                                "✅ On track with your goals!",
                                color = GreenAccent,
                                fontSize = 12.sp
                            )
                        }
                    }
                }
            }

            // ── Quick Actions ──────────────────────────────────────────────
            SectionTitle("QUICK ACTIONS")
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                QuickActionCard(
                    emoji = "➕",
                    label = "New Session",
                    modifier = Modifier.weight(1f),
                    onClick = { onNavigate("add_session") }
                )
                QuickActionCard(
                    emoji = "📋",
                    label = "Sessions",
                    modifier = Modifier.weight(1f),
                    onClick = { onNavigate("session_list") }
                )
            }
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                QuickActionCard(
                    emoji = "📊",
                    label = "Reports",
                    modifier = Modifier.weight(1f),
                    onClick = { onNavigate("reports") }
                )
                QuickActionCard(
                    emoji = "🎯",
                    label = "Goals",
                    modifier = Modifier.weight(1f),
                    onClick = { onNavigate("goals") }
                )
            }

            // ── Status ────────────────────────────────────────────────────
            SectionTitle("STATUS")
            BetWiseCard {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceEvenly
                ) {
                    StatItem(label = "Total Chips", value = "$monthTotal 🪙")
                    StatItem(label = "Goal", value = if (currentGoal != null) "Set ✅" else "Not Set ❌")
                    StatItem(label = "Status", value = when {
                        isOverLimit -> "Over 🔴"
                        isUnderMin -> "Under 🟡"
                        else -> "Good 🟢"
                    })
                }
            }
        }
    }
}

@Composable
private fun QuickActionCard(emoji: String, label: String, modifier: Modifier = Modifier, onClick: () -> Unit) {
    Card(
        onClick = onClick,
        modifier = modifier.height(80.dp),
        colors = CardDefaults.cardColors(containerColor = CardDark),
        shape = RoundedCornerShape(14.dp)
    ) {
        Column(
            modifier = Modifier.fillMaxSize(),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text(emoji, fontSize = 26.sp)
            Spacer(Modifier.height(4.dp))
            Text(label, color = TextWhite, fontSize = 12.sp, fontWeight = FontWeight.Medium)
        }
    }
}

@Composable
private fun StatItem(label: String, value: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(value, color = GoldPrimary, fontWeight = FontWeight.Bold, fontSize = 14.sp)
        Text(label, color = TextGray, fontSize = 11.sp)
    }
}
