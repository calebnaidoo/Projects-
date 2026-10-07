package com.betwise.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel
import java.time.LocalDate
import java.time.format.DateTimeFormatter

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GoalsScreen(viewModel: MainViewModel, onNavigateBack: () -> Unit) {
    val currentGoal by viewModel.currentGoal.collectAsState()
    val monthTotal by viewModel.monthTotal.collectAsState()

    var minChips by remember { mutableStateOf(currentGoal?.minChips?.toString() ?: "") }
    var maxChips by remember { mutableStateOf(currentGoal?.maxChips?.toString() ?: "") }
    var error by remember { mutableStateOf<String?>(null) }
    var saved by remember { mutableStateOf(false) }

    val currentMonth = LocalDate.now().format(DateTimeFormatter.ofPattern("MMMM yyyy"))

    LaunchedEffect(currentGoal) {
        currentGoal?.let {
            minChips = it.minChips.toString()
            maxChips = it.maxChips.toString()
        }
    }

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Goals", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Back", tint = TextWhite)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark)
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // Current month status
            BetWiseCard {
                SectionTitle("$currentMonth Status".uppercase())
                Spacer(Modifier.height(14.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceEvenly
                ) {
                    GoalStatColumn("Used", "$monthTotal 🪙", GoldPrimary)
                    GoalStatColumn(
                        "Min Goal",
                        if (currentGoal != null) "${currentGoal!!.minChips} 🎯" else "Not set",
                        GreenAccent
                    )
                    GoalStatColumn(
                        "Max Limit",
                        if (currentGoal != null) "${currentGoal!!.maxChips} 🔴" else "Not set",
                        RedAccent
                    )
                }

                currentGoal?.let { goal ->
                    Spacer(Modifier.height(16.dp))
                    GoldDivider()
                    Spacer(Modifier.height(12.dp))
                    val status = when {
                        monthTotal > goal.maxChips -> "⚠️ Over limit by ${monthTotal - goal.maxChips} chips"
                        monthTotal < goal.minChips -> "💡 ${goal.minChips - monthTotal} chips below minimum goal"
                        else -> "✅ On track! Great work."
                    }
                    Text(
                        status,
                        color = when {
                            monthTotal > goal.maxChips -> RedAccent
                            monthTotal < goal.minChips -> GoldVariant
                            else -> GreenAccent
                        },
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Medium
                    )
                }
            }

            // Set Goals
            BetWiseCard {
                SectionTitle("SET MONTHLY GOALS")
                Spacer(Modifier.height(16.dp))

                Text("Minimum Chip Goal 🎯", color = TextGray, fontSize = 13.sp)
                Spacer(Modifier.height(8.dp))
                OutlinedTextField(
                    value = minChips,
                    onValueChange = { if (it.all { c -> c.isDigit() }) { minChips = it; error = null; saved = false } },
                    label = { Text("Min Chips") },
                    leadingIcon = { Icon(Icons.Default.TrendingUp, null, tint = GreenAccent) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth(),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = GreenAccent,
                        unfocusedBorderColor = Color(0xFF444444),
                        focusedLabelColor = GreenAccent,
                        unfocusedLabelColor = TextGray,
                        cursorColor = GreenAccent,
                        focusedTextColor = TextWhite,
                        unfocusedTextColor = TextWhite
                    ),
                    shape = RoundedCornerShape(10.dp),
                    singleLine = true
                )

                Spacer(Modifier.height(14.dp))

                Text("Maximum Chip Limit 🔴", color = TextGray, fontSize = 13.sp)
                Spacer(Modifier.height(8.dp))
                OutlinedTextField(
                    value = maxChips,
                    onValueChange = { if (it.all { c -> c.isDigit() }) { maxChips = it; error = null; saved = false } },
                    label = { Text("Max Chips") },
                    leadingIcon = { Icon(Icons.Default.TrendingDown, null, tint = RedAccent) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth(),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = RedAccent,
                        unfocusedBorderColor = Color(0xFF444444),
                        focusedLabelColor = RedAccent,
                        unfocusedLabelColor = TextGray,
                        cursorColor = RedAccent,
                        focusedTextColor = TextWhite,
                        unfocusedTextColor = TextWhite
                    ),
                    shape = RoundedCornerShape(10.dp),
                    singleLine = true
                )

                if (error != null) {
                    Spacer(Modifier.height(8.dp))
                    Text("⚠️ $error", color = RedAccent, fontSize = 13.sp)
                }
                if (saved) {
                    Spacer(Modifier.height(8.dp))
                    Text("✅ Goals saved successfully!", color = GreenAccent, fontSize = 13.sp)
                }

                Spacer(Modifier.height(20.dp))
                GoldButton(
                    text = "SAVE GOALS",
                    onClick = {
                        val min = minChips.toIntOrNull()
                        val max = maxChips.toIntOrNull()
                        when {
                            min == null || min < 0 -> error = "Enter a valid minimum"
                            max == null || max <= 0 -> error = "Enter a valid maximum"
                            min >= max -> error = "Minimum must be less than maximum"
                            else -> {
                                viewModel.setGoal(min, max)
                                saved = true
                            }
                        }
                    }
                )
            }

            // Tips
            BetWiseCard {
                SectionTitle("💡 GOAL TIPS")
                Spacer(Modifier.height(10.dp))
                val tips = listOf(
                    "Set a realistic minimum to stay engaged",
                    "Your max limit helps you practise restraint",
                    "Goals reset each month automatically",
                    "Check your reports to see trends over time"
                )
                tips.forEach { tip ->
                    Row(modifier = Modifier.padding(vertical = 4.dp)) {
                        Text("•  ", color = GoldPrimary)
                        Text(tip, color = TextGray, fontSize = 13.sp)
                    }
                }
            }
        }
    }
}

@Composable
private fun GoalStatColumn(label: String, value: String, color: Color) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(value, color = color, fontWeight = FontWeight.Bold, fontSize = 15.sp)
        Spacer(Modifier.height(4.dp))
        Text(label, color = TextGray, fontSize = 12.sp)
    }
}
