package com.betwise.app.ui.screens

import androidx.compose.foundation.background
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
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.Category
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel
import java.time.LocalDate
import java.time.format.DateTimeFormatter

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReportsScreen(viewModel: MainViewModel, onNavigate: (String) -> Unit) {
    val sessions by viewModel.sessions.collectAsState(initial = emptyList())
    val categoryMap by viewModel.categoryMap.collectAsState()
    val monthTotal by viewModel.monthTotal.collectAsState()
    val currentGoal by viewModel.currentGoal.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.loadCategoryMap()
        viewModel.loadDashboard()
    }

    // Build category totals from collected sessions
    val categoryTotals = remember(sessions, categoryMap) {
        sessions
            .filter { it.date.startsWith(LocalDate.now().format(DateTimeFormatter.ofPattern("yyyy-MM"))) }
            .groupBy { it.categoryId }
            .map { (catId, sessList) ->
                categoryMap[catId] to sessList.sumOf { it.chipAmount }
            }
            .sortedByDescending { it.second }
    }

    val totalSessions = sessions.size
    val thisMonthSessions = sessions.count {
        it.date.startsWith(LocalDate.now().format(DateTimeFormatter.ofPattern("yyyy-MM")))
    }
    val currentMonth = LocalDate.now().format(DateTimeFormatter.ofPattern("MMMM yyyy"))

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Reports", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark)
            )
        },
        bottomBar = { BetWiseBottomBar(currentRoute = "reports", onNavigate = onNavigate) }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(horizontal = 16.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp),
            contentPadding = PaddingValues(vertical = 16.dp)
        ) {
            item {
                // Summary header card
                Card(
                    colors = CardDefaults.cardColors(containerColor = Color.Transparent),
                    shape = RoundedCornerShape(18.dp)
                ) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(
                                Brush.linearGradient(listOf(Color(0xFF2A1F00), Color(0xFF1A1500)))
                            )
                            .padding(20.dp)
                    ) {
                        Column {
                            Text("$currentMonth Report".uppercase(),
                                color = GoldPrimary, fontSize = 12.sp, letterSpacing = 1.sp)
                            Spacer(Modifier.height(14.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                ReportStat("Total Chips", "$monthTotal 🪙")
                                ReportStat("Sessions", "$thisMonthSessions 🎲")
                                ReportStat("All-Time", "$totalSessions 📋")
                            }
                        }
                    }
                }
            }

            item {
                // Goal progress
                currentGoal?.let { goal ->
                    BetWiseCard {
                        SectionTitle("GOAL PROGRESS")
                        Spacer(Modifier.height(12.dp))
                        val pct = if (goal.maxChips > 0)
                            (monthTotal.toFloat() / goal.maxChips * 100).toInt().coerceIn(0, 999)
                        else 0
                        val progress = (monthTotal.toFloat() / goal.maxChips).coerceIn(0f, 1f)
                        val color = if (monthTotal > goal.maxChips) RedAccent else GoldPrimary

                        Text("$pct% of monthly limit used", color = color, fontSize = 14.sp, fontWeight = FontWeight.SemiBold)
                        Spacer(Modifier.height(8.dp))
                        LinearProgressIndicator(
                            progress = { progress },
                            modifier = Modifier.fillMaxWidth().height(12.dp).clip(RoundedCornerShape(6.dp)),
                            color = color,
                            trackColor = Color(0xFF333333)
                        )
                        Spacer(Modifier.height(8.dp))
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text("Min: ${goal.minChips}", color = GreenAccent, fontSize = 12.sp)
                            Text("Max: ${goal.maxChips}", color = RedAccent, fontSize = 12.sp)
                        }
                    }
                } ?: BetWiseCard {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Icon(Icons.Default.Info, null, tint = GoldPrimary)
                        Spacer(Modifier.width(10.dp))
                        Text("No goal set for this month. Go to Goals to set one.", color = TextGray, fontSize = 13.sp)
                    }
                }
            }

            // Category breakdown
            if (categoryTotals.isNotEmpty()) {
                item { SectionTitle("CATEGORY BREAKDOWN") }
                val maxVal = categoryTotals.maxOfOrNull { it.second }?.toFloat() ?: 1f
                items(categoryTotals) { (cat, total) ->
                    CategoryBar(
                        category = cat,
                        total = total,
                        maxValue = maxVal
                    )
                }
            }

            // Recent sessions summary
            if (sessions.isNotEmpty()) {
                item { SectionTitle("RECENT ACTIVITY") }
                items(sessions.take(5)) { session ->
                    val cat = categoryMap[session.categoryId]
                    Card(
                        colors = CardDefaults.cardColors(containerColor = CardDark),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Row(
                            modifier = Modifier.padding(14.dp).fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column {
                                Text(session.title, color = TextWhite, fontSize = 14.sp, fontWeight = FontWeight.Medium)
                                Text("${cat?.icon ?: "🎰"} ${cat?.name ?: "Unknown"} • ${session.date}",
                                    color = TextGray, fontSize = 12.sp)
                            }
                            ChipBadge(session.chipAmount)
                        }
                    }
                }
            }
            item { Spacer(Modifier.height(8.dp)) }
        }
    }
}

@Composable
private fun ReportStat(label: String, value: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(value, color = GoldPrimary, fontWeight = FontWeight.Bold, fontSize = 16.sp)
        Text(label, color = TextGray, fontSize = 11.sp)
    }
}

@Composable
private fun CategoryBar(category: Category?, total: Int, maxValue: Float) {
    val progress = if (maxValue > 0) total / maxValue else 0f
    Card(
        colors = CardDefaults.cardColors(containerColor = CardDark),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(modifier = Modifier.padding(14.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    "${category?.icon ?: "🎰"} ${category?.name ?: "Unknown"}",
                    color = TextWhite,
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Medium
                )
                Text("$total chips", color = GoldPrimary, fontWeight = FontWeight.Bold, fontSize = 13.sp)
            }
            Spacer(Modifier.height(8.dp))
            LinearProgressIndicator(
                progress = { progress },
                modifier = Modifier.fillMaxWidth().height(8.dp).clip(RoundedCornerShape(4.dp)),
                color = GoldPrimary,
                trackColor = Color(0xFF333333)
            )
        }
    }
}
