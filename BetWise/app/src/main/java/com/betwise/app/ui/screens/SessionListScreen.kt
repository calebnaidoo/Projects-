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
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.betwise.app.data.entities.Session
import com.betwise.app.ui.components.*
import com.betwise.app.ui.theme.*
import com.betwise.app.viewmodel.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SessionListScreen(viewModel: MainViewModel, onNavigate: (String) -> Unit) {
    val sessions by viewModel.sessions.collectAsState(initial = emptyList())
    val categoryMap by viewModel.categoryMap.collectAsState()

    LaunchedEffect(Unit) { viewModel.loadCategoryMap() }

    Scaffold(
        containerColor = DarkBg,
        topBar = {
            TopAppBar(
                title = { Text("Sessions", color = GoldPrimary, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = SurfaceDark),
                actions = {
                    IconButton(onClick = { onNavigate("add_session") }) {
                        Icon(Icons.Default.Add, contentDescription = "Add", tint = GoldPrimary)
                    }
                }
            )
        },
        bottomBar = { BetWiseBottomBar(currentRoute = "session_list", onNavigate = onNavigate) }
    ) { padding ->
        if (sessions.isEmpty()) {
            Box(
                modifier = Modifier.fillMaxSize().padding(padding),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("🎲", fontSize = 56.sp)
                    Spacer(Modifier.height(12.dp))
                    Text("No sessions yet", color = TextGray, fontSize = 16.sp)
                    Spacer(Modifier.height(8.dp))
                    GoldButton(
                        text = "Log First Session",
                        onClick = { onNavigate("add_session") },
                        modifier = Modifier.width(200.dp)
                    )
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .padding(horizontal = 16.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp),
                contentPadding = PaddingValues(vertical = 16.dp)
            ) {
                item { SectionTitle("ALL SESSIONS  •  ${sessions.size} total") }
                items(sessions) { session ->
                    val cat = categoryMap[session.categoryId]
                    SessionCard(
                        session = session,
                        categoryName = cat?.name ?: "Unknown",
                        categoryIcon = cat?.icon ?: "🎰",
                        onDelete = { viewModel.deleteSession(session) }
                    )
                }
            }
        }
    }
}

@Composable
private fun SessionCard(
    session: Session,
    categoryName: String,
    categoryIcon: String,
    onDelete: () -> Unit
) {
    var showConfirm by remember { mutableStateOf(false) }

    Card(
        colors = CardDefaults.cardColors(containerColor = CardDark),
        shape = RoundedCornerShape(14.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.Top
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(session.title, color = TextWhite, fontWeight = FontWeight.Bold, fontSize = 15.sp)
                    Spacer(Modifier.height(4.dp))
                    Text("$categoryIcon $categoryName", color = TextGray, fontSize = 13.sp)
                }
                IconButton(onClick = { showConfirm = true }, modifier = Modifier.size(32.dp)) {
                    Icon(Icons.Default.Delete, contentDescription = "Delete", tint = RedAccent.copy(alpha = 0.6f))
                }
            }
            Spacer(Modifier.height(10.dp))
            GoldDivider()
            Spacer(Modifier.height(10.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column {
                    Text(session.date, color = TextGray, fontSize = 12.sp)
                    Text("${session.startTime} – ${session.endTime}", color = TextGray, fontSize = 12.sp)
                }
                ChipBadge(session.chipAmount)
            }
            if (session.description.isNotBlank()) {
                Spacer(Modifier.height(8.dp))
                Text(session.description, color = TextGray, fontSize = 12.sp, maxLines = 2)
            }
        }
    }

    if (showConfirm) {
        AlertDialog(
            onDismissRequest = { showConfirm = false },
            containerColor = SurfaceDark,
            title = { Text("Delete Session?", color = TextWhite) },
            text = { Text("Delete '${session.title}'?", color = TextGray) },
            confirmButton = {
                TextButton(onClick = { onDelete(); showConfirm = false }) {
                    Text("Delete", color = RedAccent)
                }
            },
            dismissButton = {
                TextButton(onClick = { showConfirm = false }) { Text("Cancel", color = TextGray) }
            }
        )
    }
}
